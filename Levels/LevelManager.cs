using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using MapObject = Level.MapObject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Setting up levels, storing tile prefabs and so on.
/// </summary>
public class LevelManager : MonoBehaviour {
    public GameObject caveWall;

    // Floor Assets
    public GameObject[] caveFloor;

    // Object Assets
    public GameObject[] caveGoldOre;
    public GameObject[] cavePurpleOre;
    public GameObject[] caveBlockage;

    public GameObject[] enemyFighterSpawner;
    public GameObject[] enemyFighter;

    public GameObject[] enemySuicider;
    public GameObject[] enemySuiciderSpawner;

    public GameObject[] enemyBomber;
    public GameObject[] enemyBomberSpawner;

    public GameObject[] enemyBoss;

    public GameObject portal;

    public GameObject player;
    public GameObject exit;

    public Level currentLevel;

    private Transform levelGrid;

    private Dictionary<MapObject, GameObject> portalMap;

    /// <summary>
    /// Set up a scene.
    /// </summary>
    /// <param name="level"> Level id. </param>
    /// <param name="gen"> Generator for custom level. </param>
    public void SetupScene(int level, IMapGenerator gen = null) {
        IMapGenerator levelGenerator = gen;

        if (levelGenerator == null) {
            if (level == 5) {
                levelGenerator = new ReaderGenerator("Levels/NewBossLevel");
            } else {
                levelGenerator = new CaveGenerator(150, 150);
            }
        }

        Debug.Log($"Level number: {level}");

        this.currentLevel = new Level(levelGenerator, level);

        this.currentLevel.AddFloorAsset(this.caveFloor, MapObject.Floor);

        this.currentLevel.AddObjectAsset(this.caveGoldOre, MapObject.GoldOre);
        this.currentLevel.AddObjectAsset(this.cavePurpleOre, MapObject.HealOre);
        this.currentLevel.AddObjectAsset(this.caveBlockage, MapObject.Blockage);

        this.currentLevel.AddObjectAsset(this.enemyFighterSpawner, MapObject.EnemyFighterSpawner);
        this.currentLevel.AddObjectAsset(this.enemyFighter, MapObject.EnemyFighter);

        this.currentLevel.AddObjectAsset(this.enemySuiciderSpawner, MapObject.EnemySuiciderSpawner);
        this.currentLevel.AddObjectAsset(this.enemySuicider, MapObject.EnemySuicider);

        this.currentLevel.AddObjectAsset(this.enemyBomberSpawner, MapObject.EnemyBomberSpawner);
        this.currentLevel.AddObjectAsset(this.enemyBomber, MapObject.EnemyBomber);

        this.currentLevel.AddObjectAsset(this.enemyBoss, MapObject.EnemyFlagman);

        this.currentLevel.AddWallAsset(this.caveWall, MapObject.Wall);
        this.currentLevel.AddWallAsset(this.caveGoldOre[0], MapObject.GoldOre);
        this.currentLevel.AddWallAsset(this.cavePurpleOre[0], MapObject.HealOre);
        this.currentLevel.AddWallAsset(this.caveBlockage[0], MapObject.Blockage);

        Debug.Log("Added assets");

        this.LevelSetup(this.currentLevel);
    }

    /// <summary>
    /// Set up a <paramref name="level"/>.
    /// </summary>
    /// <param name="level"> Level to set up. </param>
    private void LevelSetup(Level level) {
        this.levelGrid = new GameObject("LevelGrid").transform;
        this.portalMap = new Dictionary<MapObject, GameObject>();

        for (int i = 0; i < level.height - 1; i++) {
            for (int j = 0; j < level.width - 1; j++) {
                var settingFloor = Utility.SelectRandomItem(level.floorAssets[MapObject.Floor]);
                var instanceFloor = Instantiate(settingFloor, new Vector3(j, i, 0f), Quaternion.identity) as GameObject;

                instanceFloor.transform.SetParent(this.levelGrid);

                if (level.wallMap[i][j] != 0) {
                    MapObject material = MapObject.Wall;
                    if (Level.Walls.Contains(level.objectMap[i][j]) && level.objectMap[i][j] != MapObject.Wall) {
                        material = level.objectMap[i][j];
                    } else if (Level.Walls.Contains(level.objectMap[i + 1][j]) && level.objectMap[i + 1][j] != MapObject.Wall) {
                        material = level.objectMap[i + 1][j];
                    } else if (Level.Walls.Contains(level.objectMap[i + 1][j + 1]) && level.objectMap[i + 1][j + 1] != MapObject.Wall) {
                        material = level.objectMap[i + 1][j + 1];
                    } else if (Level.Walls.Contains(level.objectMap[i + 1][j + 1]) && level.objectMap[i + 1][j + 1] != MapObject.Wall) {
                        material = level.objectMap[i][j + 1];
                    }

                    GameObject instanceWall = level.wallAssets[material].GetComponent<Wall>().InstantiateWall(j, i, level);

                    level.wallObjects[i][j] = instanceWall;
                    instanceWall.transform.SetParent(this.levelGrid);
                }

                if (!Level.Walls.Contains(level.objectMap[i][j]) && level.objectMap[i][j] != MapObject.Floor) {
                    if (level.objectMap[i][j] >= MapObject.PortalRed && level.objectMap[i][j] <= MapObject.PortalYellow) {
                        var instance = Instantiate(this.portal, new Vector3(j, i, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(this.levelGrid);
                        if (this.portalMap.ContainsKey(level.objectMap[i][j])) {
                            GameObject portalOut = this.portalMap[level.objectMap[i][j]];
                            instance.GetComponent<Portal>().SetupPortal(portalOut, level.objectMap[i][j]);
                        } else {
                            this.portalMap.Add(level.objectMap[i][j], instance);
                        }
                    } else {
                        GameObject settingObject = level.objectsAssets[level.objectMap[i][j]][Random.Range(0, level.objectsAssets[level.objectMap[i][j]].Count)];
                        var instanceObject = Instantiate(settingObject, new Vector3(j, i, 0f), Quaternion.identity) as GameObject;
                        instanceObject.name = settingObject.name;
                        instanceObject.transform.SetParent(this.levelGrid);
                    }
                }
            }
        }

        Debug.Log("Player pos: " + level.playerStart);
        Object.Instantiate(this.player, new Vector3(level.playerStart.x, level.playerStart.y, 0f), Quaternion.identity);
        Debug.Log("Exit pos: " + level.exitPos);
        Object.Instantiate(this.exit, new Vector3(level.exitPos.x, level.exitPos.y, 0f), Quaternion.identity);
    }
}
