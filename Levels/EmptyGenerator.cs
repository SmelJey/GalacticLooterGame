using System.Collections.Generic;
using UnityEngine;
using MapObject = Level.MapObject;

/// <summary>
/// Creates empty field of sizes <see cref="width"/> and <see cref="height"/>.
/// </summary>
public class EmptyGenerator : IMapGenerator {
    public int width;
    public int height;

    public EmptyGenerator(int levelWidth, int levelHeight) {
        this.width = levelWidth + Level.BorderSize * 2;
        this.height = levelHeight + Level.BorderSize * 2;
        GameLogger.LogMessage($"Generator params: {this.width} x {this.height}", "EmptyGenerator");
    }

    public void GenerateLevel(Level level) {
        level.width = this.width;
        level.height = this.height;

        if (level.height < Level.MinHeight || level.width < Level.MinWidth) {
            GameLogger.LogError("Level is too small", "EmptyGenerator");
            level.height = Level.MinHeight;
            level.width = Level.MinWidth;
        }

        level.width = this.width;
        level.height = this.height;

        level.objectMap = new MapObject[height, width];
        for (int i = 0; i < level.height; i++) {
            for (int j = 0; j < level.width; j++) {
                if (i < Level.BorderSize || i >= level.height - Level.BorderSize || j < Level.BorderSize || j >= level.width - Level.BorderSize) {
                    level.objectMap[i, j] = MapObject.Wall;
                } else {
                    level.objectMap[i, j] = MapObject.Floor;
                }
            }
        }
    }

    public void PlaceItems(Level level, int itemCount) {
        return;
    }

    public void PlaceOre(Level level, MapObject[] ores, int[] oreCounts, float[] chunkChances) {
        return;
    }

    public void PlaceSpawners(Level level, MapObject[] spawners, int[] counts) {
        return;
    }

    public void SetExitPosition(Level level) {
        return;
    }

    public void SetPlayerPosition(Level level) {
        level.objectMap[Level.BorderSize, Level.BorderSize] = MapObject.Player;
    }
}
