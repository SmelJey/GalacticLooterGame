using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for processing all issues related to current level.
/// </summary>
public class Level {
    public static readonly HashSet<MapObject> Walls = new HashSet<MapObject>(new MapObject[] { MapObject.Blockage, MapObject.GoldOre, MapObject.HealOre, MapObject.Wall });

    public static readonly int BorderSize = 30;

    public static readonly int MinHeight = 30;
    public static readonly int MinWidth = 30;

    public IMapGenerator generator;

    // For wall processing
    public List<List<char>> wallMap;
    public List<List<GameObject>> wallObjects;

    // For object placing (maybe better use list of objects?)
    public List<List<MapObject>> objectMap;

    // Assets for every char in matching map
    public Dictionary<MapObject, GameObject> wallAssets;
    public Dictionary<MapObject, List<GameObject>> objectsAssets;
    public Dictionary<MapObject, List<GameObject>> floorAssets;

    public int width;
    public int height;

    public Vector2 playerStart;
    public Vector2 exitPos;

    private static readonly List<Placement> Placements = new List<Placement> { new Placement(255, 255, '+'),
                                                                new Placement(255, 247, 'D'), new Placement(255, 253, 'C'),
                                                                new Placement(255, 127, 'B'), new Placement(255, 223, 'A'),
                                                                new Placement(221, 28, 'H'), new Placement(190, 28, 'H'),
                                                                new Placement(119, 7, 'G'), new Placement(175, 7, 'G'),
                                                                new Placement(221, 193, 'F'), new Placement(235, 193, 'F'),
                                                                new Placement(119, 112, 'E'), new Placement(250, 112, 'E'),
                                                                new Placement(215, 199, 'l'), new Placement(255, 215, 'l'),
                                                                new Placement(125, 124, 'r'), new Placement(255, 125, 'r'),
                                                                new Placement(95, 31, 'u'), new Placement(255, 95, 'u'),
                                                                new Placement(245, 241, 'd'), new Placement(255, 245, 'd'), };

    private int difficulty = 0;

    public Level(IMapGenerator mapGenerator, int difficulty) {
        this.generator = mapGenerator;
        this.difficulty = difficulty;
        Debug.Log($"Current difficulty {difficulty} {this.difficulty}");
        this.LevelSetup();
    }

    /// <summary>
    /// Tiles' id list.
    /// </summary>
    public enum MapObject {
        Floor,
        Wall,
        DestroyableWall,
        Blockage,
        GoldOre,
        Player,
        Exit,
        PortalRed,
        PortalGreen,
        PortalBlue,
        PortalMagenta,
        PortalCyan,
        PortalYellow,
        EnemyFighter,
        EnemyFighterSpawner,
        EnemyFlagman,
        HealOre,
        EnemySuicider,
        EnemySuiciderSpawner,
        EnemyBomber,
        EnemyBomberSpawner
    }

    /// <summary>
    /// Initialization of assets. Should be used in the end of level constructor.
    /// </summary>
    public void AssetsInit() {
        if (this.wallAssets == null) {
            this.wallAssets = new Dictionary<MapObject, GameObject>();
        }

        if (this.objectsAssets == null) {
            this.objectsAssets = new Dictionary<MapObject, List<GameObject>>();
        }

        if (this.floorAssets == null) {
            this.floorAssets = new Dictionary<MapObject, List<GameObject>>();
        }
    }

    /// <summary>
    /// Find path between two points on level map.
    /// </summary>
    /// <param name="start"> Starting point. </param>
    /// <param name="dest"> Destination. </param>
    /// <param name="resultPath"> Variable, where path will contain. </param>
    /// <returns> List of path coordinates or null, if unable to reach. </returns>
    public IEnumerator Pathfinding(Vector2Int start, Vector2Int dest, List<Vector2Int> resultPath) {
        var backtrack = new Dictionary<Vector2Int, Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(start);

        int cnt = 0;
        const int countPerRun = 100;

        while (q.Count > 0) {
            Vector2Int node = q.Dequeue();

            if (node == dest) {
                break;
            }

            cnt++;
            if (cnt >= countPerRun) {
                cnt = 0;
                yield return null;
            }

            for (int t = 1; t < 8; t += 2) {
                var checkNode = new Vector2Int(node.x + Utility.Dx[t], node.y + Utility.Dy[t]);

                if (backtrack.ContainsKey(checkNode) || Level.Walls.Contains(this.objectMap[checkNode.y][checkNode.x])) {
                    continue;
                }

                q.Enqueue(checkNode);
                backtrack.Add(checkNode, node);
            }

            for (int t = 0; t < 8; t += 2) {
                var checkNode = new Vector2Int(node.x + Utility.Dx[t], node.y + Utility.Dy[t]);

                if (backtrack.ContainsKey(checkNode) || Level.Walls.Contains(this.objectMap[checkNode.y][checkNode.x])) {
                    continue;
                }

                if ((!Level.Walls.Contains(this.objectMap[node.y + Utility.Dy[t]][node.x]))
                    || (!Level.Walls.Contains(this.objectMap[node.y][node.x + Utility.Dx[t]]))) {
                    q.Enqueue(checkNode);
                    backtrack.Add(checkNode, node);
                }
            }
        }

        resultPath.Clear();

        if (backtrack.ContainsKey(dest)) {
            while (dest != start) {
                resultPath.Add(new Vector2Int(dest.x, dest.y));
                dest = backtrack[dest];
            }

            resultPath.Add(new Vector2Int(dest.x, dest.y));
            resultPath.Reverse();
        }
    }

    /// <summary>
    /// Add assets from <paramref name="arr"/> to wallAssets Dictionary.
    /// </summary>
    /// <param name="arr"> An array of assets. </param>
    /// <param name="alias"> Key in wallAssets dictionary. </param>
    public void AddWallAsset(GameObject arr, MapObject alias) {
        if (arr == null) {
            Debug.LogError($"LevelManager: Asset: {alias} is not set yet");
            return;
        }

        this.wallAssets[alias] = arr;
    }

    /// <summary>
    /// Add assets from <paramref name="arr"/> to objectAsset Dictionary.
    /// </summary>
    /// <param name="arr"> An array of assets. </param>
    /// <param name="alias"> Key in objectAssets dictionary. </param>
    public void AddObjectAsset(GameObject[] arr, MapObject alias) {
        if (arr == null || arr.Length == 0) {
            Debug.LogError($"LevelManager: Asset: {alias} is not set yet");
            return;
        }

        this.objectsAssets.Add(alias, new List<GameObject>());

        for (int i = 0; i < arr.Length; i++) {
            this.objectsAssets[alias].Add(arr[i]);
        }
    }

    /// <summary>
    /// Add assets from <paramref name="arr"/> to floorAsset Dictionary.
    /// </summary>
    /// <param name="arr"> An array of assets. </param>
    /// <param name="alias"> Key in floorAssets dictionary. </param>
    public void AddFloorAsset(GameObject[] arr, MapObject alias) {
        if (arr == null || arr.Length == 0) {
            Debug.LogError($"LevelManager: Asset: {alias} is not set yet");
            return;
        }

        this.floorAssets.Add(alias, new List<GameObject>());

        for (int i = 0; i < arr.Length; i++) {
            this.floorAssets[alias].Add(arr[i]);
        }
    }

    public void UpdateWallState(int x, int y) {
        int wallCode = 0;
        if (Walls.Contains(this.objectMap[y][x])) {
            wallCode += 8;
        }

        if (Walls.Contains(this.objectMap[y][x + 1])) {
            wallCode += 4;
        }

        if (Walls.Contains(this.objectMap[y + 1][x + 1])) {
            wallCode += 2;
        }

        if (Walls.Contains(this.objectMap[y + 1][x])) {
            wallCode += 1;
        }

        this.wallMap[y][x] = (char)wallCode;

        if (this.wallObjects[y][x] != null) {
            this.wallObjects[y][x].GetComponent<Wall>().UpdateSprite(this);
        }
    }

    private void LevelSetup() {
        this.generator.GenerateLevel(this);
        this.generator.SetPlayerPosition(this);
        this.generator.SetExitPosition(this);
        this.generator.PlaceOre(this, new MapObject[] { MapObject.GoldOre, MapObject.HealOre }, new int[] { 100, 25 }, new float[] { 0.25f, 0.1f });
        this.WallProc();
        this.FindObjects();

        int fightersCount = 14;
        int suicidersCount = 19;
        int bombersCount = 7;
        int entityCap = this.width * this.height / 100;

        switch (this.difficulty) {
            case 0: {
                fightersCount = 0;
                suicidersCount = 0;
                bombersCount = 0;
                break;
            }

            case 1: {
                fightersCount = 20;
                suicidersCount = 0;
                bombersCount = 0;
                break;
            }

            case 2: {
                fightersCount = 15;
                suicidersCount = 20;
                bombersCount = 0;
                break;
            }

            default: {
                int coef = Mathf.Min(this.difficulty, (int)(4f * (entityCap - fightersCount - suicidersCount - bombersCount) / 5f));
                fightersCount += coef / 2;
                suicidersCount += coef / 2;
                bombersCount += coef / 4;
                break;
            }
        }

        Debug.Log($"Fighters: {fightersCount} Suiciders: {suicidersCount} Bombers: {bombersCount}, Difficulty: {this.difficulty}");
        this.generator.PlaceSpawners(this, new MapObject[] { MapObject.EnemyFighterSpawner, MapObject.EnemySuiciderSpawner, MapObject.EnemyBomberSpawner }, new int[] { fightersCount, suicidersCount, bombersCount });

        this.AssetsInit();
    }

    /// <summary>
    /// Find player starting tile and exit ('P' and 'E' on objectMap).
    /// </summary>
    private void FindObjects() {
        for (int i = 0; i < this.height; i++) {
            for (int j = 0; j < this.width; j++) {
                if (this.objectMap[i][j] == MapObject.Player) {
                    this.objectMap[i][j] = MapObject.Floor;
                    this.playerStart = new Vector2(j, i);
                } else if (this.objectMap[i][j] == MapObject.Exit) {
                    this.objectMap[i][j] = MapObject.Floor;
                    this.exitPos = new Vector2(j, i);
                }
            }
        }
    }

    /// <summary>
    /// Initialize wallMap and make internal representation of walls.
    /// </summary>
    private void WallProc() {
        this.wallMap = new List<List<char>>();
        this.wallObjects = new List<List<GameObject>>();
        for (int i = 0; i < this.height - 1; i++) {
            this.wallMap.Add(new List<char>());
            this.wallObjects.Add(new List<GameObject>());
            for (int j = 0; j < this.width - 1; j++) {
                this.wallMap[i].Add((char)0);
                this.wallObjects[i].Add(null);
                this.UpdateWallState(j, i);
            }
        }
    }

    /// <summary>
    /// Struct for describing tile position relative to its neighbours.
    /// </summary>
    private struct Placement {
        /// <summary>
        /// Bit mask to see only important positions.
        /// </summary>
        public byte mask;

        /// <summary>
        /// Bit (neighbour) alignment for this type of with this mask.
        /// </summary>
        public byte disp;

        /// <summary>
        /// Type of tile for this alignment.
        /// </summary>
        public char type;

        public Placement(byte m, byte d, char t) {
            this.mask = m;
            this.disp = d;
            this.type = t;
        }
    }
}
