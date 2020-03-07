using System;
using System.Collections.Generic;
using UnityEngine;
using MapObject = Level.MapObject;
using Random = UnityEngine.Random;

/// <summary>
/// Generator used to generate caves using Cellular Automaton.
/// </summary>
public sealed class CaveGenerator : IMapGenerator {
    public int width;
    public int height;

    private int seed;

    public CaveGenerator(int levelWidth, int levelHeight, int seed = -1) {
        if (seed != -1) {
            Random.InitState(seed);
        }

        Debug.Log("Random state is " + Random.state);

        this.width = levelWidth + Level.BorderSize * 2;
        this.height = levelHeight + Level.BorderSize * 2;
    }

    public void GenerateLevel(Level level) {
        level.width = this.width;
        level.height = this.height;
        this.CellularAutomatonGeneration(level);
    }

    public void PlaceOre(Level level, MapObject[] ores, int[] oreCounts, float[] chunkChances) {
        const int maxPerVein = 6;

        List<Vector2Int> deadends = this.FindDeadEnds(level);
        int deadendsIndx = 0;

        for (int ore = 0; ore < ores.Length; ore++) {
            float chance = (float)oreCounts[ore] / (level.width * level.height);
            while (oreCounts[ore] > 0) {
                int x, y;

                if (deadendsIndx < deadends.Count) {
                    x = deadends[deadendsIndx].x;
                    y = deadends[deadendsIndx].y;
                    deadendsIndx++;
                } else {
                    int attempts = 0;
                    int neighbours;
                    do {
                        if (attempts > 500) {
                            Debug.Log("Too many attempts to spawn ore");
                            return;
                        }

                        neighbours = 0;
                        x = (int)(Random.value * (level.width - 2)) + 1;
                        y = (int)(Random.value * (level.height - 2)) + 1;
                        attempts++;
                        for (int k = 0; k < 8; k++) {
                            if (level.objectMap[y + Utility.Dy[k]][x + Utility.Dx[k]] == MapObject.Wall) {
                                neighbours++;
                            }
                        }
                    } while (level.objectMap[y][x] != MapObject.Floor || neighbours < 3);
                }

                oreCounts[ore] -= this.VeinPlacement(level, x, y, ores[ore], Mathf.Min(oreCounts[ore], maxPerVein), chunkChances[ore]);
            }
        }
    }

    public void PlaceItems(Level level, int itemCount) {
        throw new NotImplementedException();
    }

    public void SetPlayerPosition(Level level) {
        const float spawnChance = 0.05f;

        float chance = Random.value;
        Vector2Int pos;
        if (chance > 0.5) {
            pos = this.SelectPosition(spawnChance, (int x, int width) => { return x; }, (int y, int height) => { return y; }, level);
        } else {
            pos = this.SelectPosition(spawnChance, (int x, int width) => { return x; }, (int y, int height) => { return height - y - 1; }, level);
        }

        level.playerStart = pos;
        level.objectMap[pos.y][pos.x] = MapObject.Player;
    }

    public void SetExitPosition(Level level) {
        const float spawnChance = 0.05f;

        float chance = Random.value;
        Vector2Int pos;
        if (chance > 0.5) {
            pos = this.SelectPosition(spawnChance, (int x, int width) => { return width - x - 1; }, (int y, int height) => { return height - y - 1; }, level);
        } else {
            pos = this.SelectPosition(spawnChance, (int x, int width) => { return width - x - 1; }, (int y, int height) => { return y; }, level);
        }

        level.objectMap[pos.y][pos.x] = MapObject.Exit;
    }

    public void PlaceSpawners(Level level, MapObject[] spawners, int[] counts) {
        for (int spawner = 0; spawner < spawners.Length; spawner++) {
            for (int i = 0; i < counts[spawner]; i++) {
                int y = Random.Range(0, level.height);
                int x = Random.Range(0, level.width);
                int tries = 0;

                while (level.objectMap[y][x] != MapObject.Floor) {
                    y = Random.Range(0, level.height);
                    x = Random.Range(0, level.width);
                    tries++;
                    if (tries > 100) {
                        Debug.LogError("Invalid map, too many tries to emplace spawner");
                    }
                }

                level.objectMap[y][x] = spawners[spawner];
            }
        }
    }

    private int VeinPlacement(Level level, int x, int y, MapObject veinType, int maxCount, float expandingChance) {
        int placed = 0;
        level.objectMap[y][x] = veinType;
        placed++;
        for (int k = 1; k < 8; k += 2) {
            if (level.objectMap[y + Utility.Dy[k]][x + Utility.Dx[k]] == MapObject.Floor && Random.value < expandingChance && placed < maxCount) {
                placed += this.VeinPlacement(level, x + Utility.Dx[k], y + Utility.Dy[k], veinType, maxCount - placed, expandingChance);
            }
        }

        return placed;
    }

    private List<Vector2Int> FindDeadEnds(Level level) {
        var buffer = new List<List<byte>>();
        for (int i = 0; i < level.height; i++) {
            buffer.Add(new List<byte>());
            for (int j = 0; j < level.width; j++) {
                if (level.objectMap[i][j] == MapObject.Wall) {
                    buffer[i].Add(2);
                } else {
                    buffer[i].Add(0);
                }
            }
        }

        var deadends = new List<Vector2Int>();

        var searchQueue = new Queue<Vector2Int>();
        searchQueue.Enqueue(new Vector2Int((int)level.playerStart.x, (int)level.playerStart.y));
        buffer[(int)level.playerStart.y][(int)level.playerStart.x] = 1;

        while (searchQueue.Count > 0) {
            bool isDeadend = true;
            var top = searchQueue.Dequeue();
            for (int k = 1; k < 8; k += 2) {
                var searchNode = new Vector2Int(top.x + Utility.Dx[k], top.y + Utility.Dy[k]);
                if (level.objectMap[searchNode.y][searchNode.x] != MapObject.Wall
                        && buffer[searchNode.y][searchNode.x] == 0) {
                    searchQueue.Enqueue(searchNode);
                    buffer[searchNode.y][searchNode.x] = 1;
                    isDeadend = false;
                }
            }

            for (int i = -2; i <= 2; i++) {
                for (int j = -2; j <= 2; j++) {
                    if (buffer[top.y + i][top.x + j] == 0) {
                        isDeadend = false;
                        break;
                    }
                }
            }

            if (isDeadend) {
                deadends.Add(top);
            }
        }

        for (int i = 0; i < deadends.Count; i++) {
            Vector2Int temp = deadends[i];
            int randomIndex = Random.Range(i, deadends.Count);
            deadends[i] = deadends[randomIndex];
            deadends[randomIndex] = temp;
        }

        return deadends;
    }

    /// <summary>
    /// Select position on <paramref name="level"/> by iterating through map from needed corner
    /// (set by <paramref name="xTransformer"/> and <paramref name="yTransformer"/>)
    /// with chance <paramref name="selectionChance"/>.
    /// </summary>
    /// <param name="selectionChance"> Selection chance. </param>
    /// <param name="xTransformer"> Function that selects side of corner by X. </param>
    /// <param name="yTransformer"> Function that selects side of corner by Y. </param>
    /// <param name="level"> Level on which position will be selected. </param>
    /// <returns> Selected position. </returns>
    private Vector2Int SelectPosition(float selectionChance, Func<int, int, int> xTransformer, Func<int, int, int> yTransformer, Level level) {
        for (int j = 0; j < level.width; j++) {
            for (int i = 0; i < level.height; i++) {
                int x = xTransformer(j, level.width);
                int y = yTransformer(i, level.height);
                if (level.objectMap[y][x] == MapObject.Floor && Random.value < selectionChance) {
                    return new Vector2Int(x, y);
                }
            }
        }

        for (int j = 0; j < level.width; j++) {
            for (int i = 0; i < level.height; i++) {
                int x = xTransformer(j, level.width);
                int y = yTransformer(i, level.height);
                if (level.objectMap[y][x] == MapObject.Floor) {
                    return new Vector2Int(x, y);
                }
            }
        }

        Debug.LogError("Cant set position");
        return Vector2Int.one;
    }

    /// <summary>
    /// Procedural generation of map via Cellular automaton.
    /// </summary>
    private void CellularAutomatonGeneration(Level level) {
        if (level.height < Level.MinHeight || level.width < Level.MinWidth) {
            Debug.LogError("Level is too small");
            level.height = Level.MinHeight;
            level.width = Level.MinWidth;
        }

        // Rate for random cellar placement
        const float initRate = 0.5f;

        // Initialization
        level.objectMap = new List<List<MapObject>>();
        for (int i = 0; i < level.height; i++) {
            level.objectMap.Add(new List<MapObject>());
            for (int j = 0; j < level.width; j++) {
                if (i < Level.BorderSize || i >= level.height - Level.BorderSize || j < Level.BorderSize || j >= level.width - Level.BorderSize) {
                    level.objectMap[i].Add(MapObject.Wall);
                    continue;
                }

                if (Random.value < initRate) {
                    level.objectMap[i].Add(MapObject.Wall);
                } else {
                    level.objectMap[i].Add(MapObject.Floor);
                }
            }
        }

        this.MakeAutomatonSteps(level.objectMap, 4, 4, 10);

        for (int i = 0; i < level.height; i++) {
            level.objectMap[i][0] = MapObject.Wall;
            level.objectMap[i][level.width - 1] = MapObject.Wall;
        }

        for (int i = 0; i < level.width; i++) {
            level.objectMap[0][i] = MapObject.Wall;
            level.objectMap[level.height - 1][i] = MapObject.Wall;
        }

        this.MakeConnectedCave(level);
    }

    private void MakeAutomatonSteps(List<List<MapObject>> map, int deathLimit, int birthLimit, int stepsCount) {
        // Buffer init
        var buffer = new List<List<MapObject>>();
        for (int i = 0; i < map.Count; i++) {
            buffer.Add(new List<MapObject>());
            for (int j = 0; j < map[i].Count; j++) {
                buffer[i].Add(MapObject.Floor);
            }
        }

        for (int t = 0; t < stepsCount; t++) {
            for (int i = 0; i < map.Count; i++) {
                for (int j = 0; j < map[i].Count; j++) {
                    if (i <= 1 || j <= 1 || i >= map.Count - 2 || j >= map[i].Count - 2) {
                        buffer[i][j] = map[i][j];
                        continue;
                    }

                    int neighbours = 0;

                    for (int k = 0; k < 8; k++) {
                        if (map[i + Utility.Dy[k]][j + Utility.Dx[k]] == MapObject.Wall) {
                            neighbours++;
                        }
                    }

                    // If someone will make it simplier would it work as intended?
                    // Second condion with 't' is to prevent huge open spaces
                    if (map[i][j] == MapObject.Wall) {
                        if (neighbours >= deathLimit /*|| (neighbours == 0 && t < 3)*/) {
                            buffer[i][j] = MapObject.Wall;
                        } else {
                            buffer[i][j] = MapObject.Floor;
                        }
                    } else {
                        if (neighbours > birthLimit /*|| (neighbours == 0 && t < 3)*/) {
                            buffer[i][j] = MapObject.Wall;
                        } else {
                            buffer[i][j] = MapObject.Floor;
                        }
                    }
                }
            }

            Utility.Clone(buffer, map);
        }
    }

    /// <summary>
    /// Floodfill alg for connectivity check.
    /// </summary>
    /// <param name="x"> Starting x coordinate. </param>
    /// <param name="y"> Starting y coordinate. </param>
    /// <param name="colour"> Colour of the floodfill. </param>
    /// <param name="minCaveSize"> Caves with less size will be deleted. </param>
    /// <param name="buffer"> Temporal matrix (should be initialized up to level map capacity). </param>
    /// <returns> Null if size is less than <paramref name="minCaveSize"/> else new Room. </returns>
    private Room FloodFill(int x, int y, int colour, int minCaveSize, List<List<int>> buffer, Level level) {
        if (level.objectMap[y][x] == MapObject.Wall) {
            return null;
        }

        int count = 0;
        var edges = new List<Vector2Int>();
        var st = new Queue<Tuple<int, int>>();
        st.Enqueue(new Tuple<int, int>(y, x));

        while (st.Count > 0) {
            bool isEdge = false;
            Tuple<int, int> node = st.Dequeue();
            buffer[node.Item1][node.Item2] = colour;
            count++;

            for (int i = 1; i < 8; i += 2) {
                if (Utility.RangeCheck(node.Item1 + Utility.Dy[i], 0, level.height - 1) && Utility.RangeCheck(node.Item2 + Utility.Dx[i], 0, level.width - 1)) {
                    if ((level.objectMap[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == MapObject.Floor
                            || level.objectMap[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == MapObject.Blockage)
                            && buffer[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == -1) {
                        st.Enqueue(new Tuple<int, int>(node.Item1 + Utility.Dy[i], node.Item2 + Utility.Dx[i]));
                        buffer[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] = colour;
                    } else if (level.objectMap[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == MapObject.Wall && !isEdge) {
                        isEdge = true;
                        edges.Add(new Vector2Int(node.Item2, node.Item1));
                    }
                }
            }
        }

        if (count < minCaveSize) {
            st.Enqueue(new Tuple<int, int>(y, x));

            while (st.Count > 0) {
                Tuple<int, int> node = st.Dequeue();
                buffer[node.Item1][node.Item2] = -1;
                level.objectMap[node.Item1][node.Item2] = MapObject.Wall;

                for (int i = 0; i < 4; i++) {
                    if (Utility.RangeCheck(node.Item1 + Utility.Dy[i], 0, level.height - 1) && Utility.RangeCheck(node.Item2 + Utility.Dx[i], 0, level.width - 1)) {
                        if ((level.objectMap[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == MapObject.Floor
                                || level.objectMap[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == MapObject.Blockage)
                                && buffer[node.Item1 + Utility.Dy[i]][node.Item2 + Utility.Dx[i]] == colour) {
                            st.Enqueue(new Tuple<int, int>(node.Item1 + Utility.Dy[i], node.Item2 + Utility.Dx[i]));
                        }
                    }
                }
            }

            return null;
        }

        return new Room(count, edges, colour);
    }

    /// <summary>
    /// Find all rooms on level map and return them in <paramref name="remRooms"/>.
    /// </summary>
    /// <param name="remRooms"> Found rooms. </param>
    private void ConnectivityCheck(out List<Room> remRooms, Level level) {
        const int minCaveSize = 1;

        remRooms = new List<Room>();
        int colour = 0;

        var buffer = new List<List<int>>();
        for (int i = 0; i < level.height; i++) {
            buffer.Add(new List<int>());
            for (int j = 0; j < level.width; j++) {
                buffer[i].Add(-1);
            }
        }

        for (int i = 0; i < level.height; i++) {
            for (int j = 0; j < level.width; j++) {
                if (level.objectMap[i][j] == MapObject.Floor && buffer[i][j] == -1) {
                    Room newRoom = this.FloodFill(j, i, colour, minCaveSize, buffer, level);
                    if (newRoom != null) {
                        remRooms.Add(newRoom);
                    }

                    colour++;
                }
            }
        }
    }

    /// <summary>
    /// Make connection between <paramref name="room1"/> and <paramref name="room2"/>.
    /// </summary>
    /// <param name="room1"> First room to connect. </param>
    /// <param name="room2"> Second room to connect. </param>
    private void MakeConnection(ref Room room1, ref Room room2, Level level) {
        room1.AddConnection(ref room2);
        room2.AddConnection(ref room1);

        Tuple<Vector2Int, Vector2Int> closestTiles = room1.FindClosestTiles(ref room2);

        Vector2Int minXTile = closestTiles.Item1.x < closestTiles.Item2.x ? closestTiles.Item1 : closestTiles.Item2;
        Vector2Int maxXTile = closestTiles.Item1.x >= closestTiles.Item2.x ? closestTiles.Item1 : closestTiles.Item2;

        var material = MapObject.Floor;
        float materialRate = Random.value;
        if (materialRate < 0.7 && room1.FindDistance(ref room2) < 3) {
            material = MapObject.Blockage;
        }

        for (int i = minXTile.x + 1; i < maxXTile.x; i++) {
            float curStep = Random.value;
            if (curStep < 0.5) {
                for (int j = minXTile.y - 1; j <= minXTile.y + 1; j++) {
                    if (Utility.RangeCheck(j, 0, level.height - 1)) {
                        level.objectMap[j][i] = material;
                    }
                }
            } else if (curStep < 0.75) {
                for (int j = minXTile.y - 2; j <= minXTile.y + 1; j++) {
                    if (Utility.RangeCheck(j, 0, level.height - 1)) {
                        level.objectMap[j][i] = material;
                    }
                }
            } else {
                for (int j = minXTile.y - 1; j <= minXTile.y + 2; j++) {
                    if (Utility.RangeCheck(j, 0, level.height - 1)) {
                        level.objectMap[j][i] = material;
                    }
                }
            }
        }

        for (int i = Mathf.Min(minXTile.y, maxXTile.y) + 1; i < Mathf.Max(minXTile.y, maxXTile.y); i++) {
            float curStep = Random.value;
            if (curStep < 0.5) {
                for (int j = maxXTile.x - 1; j <= maxXTile.x + 1; j++) {
                    if (Utility.RangeCheck(j, 0, level.width - 1)) {
                        level.objectMap[i][j] = material;
                    }
                }
            } else if (curStep < 0.75) {
                for (int j = maxXTile.x - 2; j <= maxXTile.x + 1; j++) {
                    if (Utility.RangeCheck(j, 0, level.width - 1)) {
                        level.objectMap[i][j] = material;
                    }
                }
            } else {
                for (int j = maxXTile.x - 1; j <= maxXTile.x + 2; j++) {
                    if (Utility.RangeCheck(j, 0, level.width - 1)) {
                        level.objectMap[i][j] = material;
                    }
                }
            }
        }

        level.objectMap[minXTile.y][maxXTile.x] = material;

        ////for (int i = 0; i < 8; i++) {
        ////    if (Utility.RangeCheck(minXTile.y + Utility.Dy[i], 0, level.height - 1) && Utility.RangeCheck(maxXTile.x + Utility.Dx[i], 0, level.width - 1)) {
        ////        level.objectMap[minXTile.y + Utility.Dy[i]][maxXTile.x + Utility.Dx[i]] = material;
        ////    }
        ////}
    }

    /// <summary>
    /// Connect all rooms on map to one cave.
    /// </summary>
    private void MakeConnectedCave(Level level) {
        const int connectionsPerTurn = 5;

        List<Room> remRooms;
        do {
            this.ConnectivityCheck(out remRooms, level);
            if (remRooms.Count < 2) {
                break;
            }

            var roomPairs = new List<Tuple<float, Room, Room>>();

            for (int i = 0; i < remRooms.Count; i++) {
                for (int j = i + 1; j < remRooms.Count; j++) {
                    var tmp = remRooms[j];
                    roomPairs.Add(new Tuple<float, Room, Room>(remRooms[i].FindDistance(ref tmp), remRooms[i], remRooms[j]));
                }
            }

            roomPairs.Sort(delegate(Tuple<float, Room, Room> a, Tuple<float, Room, Room> b) {
                return a.Item1.CompareTo(b.Item1);
            });

            for (int i = 0; i < Mathf.Min(connectionsPerTurn, roomPairs.Count); i++) {
                Room room1 = roomPairs[i].Item2, room2 = roomPairs[i].Item3;
                this.MakeConnection(ref room1, ref room2, level);
            }
        } while (remRooms.Count > 1);
    }

    /// <summary>
    /// Class to describe connected cave on map.
    /// </summary>
    public class Room {
        private readonly int indx;

        /// <summary>
        /// Outline of the room.
        /// </summary>
        private readonly List<Vector2Int> edgeTiles;
        private readonly HashSet<int> connectedRooms;
        private readonly Dictionary<int, Tuple<float, Vector2Int, Vector2Int>> distances;

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// </summary>
        /// <param name="roomSize"> Size of room. </param>
        /// <param name="edges"> List of edge tiles. </param>
        /// <param name="i"> Index of room. </param>
        public Room(int roomSize, List<Vector2Int> edges, int i) {
            this.connectedRooms = new HashSet<int>();
            this.edgeTiles = edges;
            this.indx = i;
            this.distances = new Dictionary<int, Tuple<float, Vector2Int, Vector2Int>>();
        }

        public void AddConnection(ref Room room) {
            this.connectedRooms.Add(room.indx);
        }

        public bool IsConnected(ref Room room) {
            return this.connectedRooms.Contains(room.indx);
        }

        /// <summary>
        /// Checking distance to the room or (if not calculated yet) calculating it with closest tiles.
        /// </summary>
        /// <param name="room"> Room which distance is checking. </param>
        /// <returns> Distance from this room to <paramref name="room"/>. </returns>
        public float FindDistance(ref Room room) {
            if (!this.distances.ContainsKey(room.indx)) {
                var t1 = default(Vector2Int);
                var t2 = default(Vector2Int);
                float distance = float.PositiveInfinity;
                foreach (Vector2Int tile1 in this.edgeTiles) {
                    foreach (Vector2Int tile2 in room.edgeTiles) {
                        float newDistance = (tile1 - tile2).magnitude;
                        if (newDistance < distance) {
                            t1 = tile1;
                            t2 = tile2;
                            distance = newDistance;
                        }
                    }
                }

                this.distances.Add(room.indx, new Tuple<float, Vector2Int, Vector2Int>(distance, t1, t2));
            }

            return this.distances[room.indx].Item1;
        }

        /// <summary>
        /// Find closest tiles between two rooms. If not calculated, <see cref="FindDistance(ref Room)"/> is launching.
        /// </summary>
        /// <param name="room"> Room which tiles are checking. </param>
        /// <returns> Tuple with Item1 from this room and Item2 from <paramref name="room"/>. </returns>
        public Tuple<Vector2Int, Vector2Int> FindClosestTiles(ref Room room) {
            if (!this.distances.ContainsKey(room.indx)) {
                this.FindDistance(ref room);
            }

            return new Tuple<Vector2Int, Vector2Int>(this.distances[room.indx].Item2, this.distances[room.indx].Item3);
        }
    }
}
