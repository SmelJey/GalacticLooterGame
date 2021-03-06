﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MapObject = Level.MapObject;

/// <summary>
/// Level generator used to read map from file.
/// </summary>
public class ReaderGenerator : IMapGenerator {
    private readonly string filePath;

    public ReaderGenerator(string path) {
        this.filePath = path;
    }

    public void GenerateLevel(Level level) {
        GameLogger.LogMessage($"Processing file {this.filePath}", "ReaderGenerator");
        var levelAsset = Resources.Load(this.filePath, typeof(TextAsset)) as TextAsset;

        try {
            if (levelAsset == null) {
                GameLogger.LogMessage("Opening custom level", "ReaderGenerator");
                using (var file = new BinaryReader(File.Open(this.filePath, FileMode.Open))) {
                    this.ProcessFile(file, level);
                }
            } else {
                GameLogger.LogMessage("Opening standart level", "ReaderGenerator");
                using (var file = new BinaryReader(new MemoryStream(levelAsset.bytes))) {
                    this.ProcessFile(file, level);
                }
            }
        } catch {
            GameLogger.LogError($"Incorrect input file", "ReaderGenerator");
            level.width = Level.MinWidth;
            level.height = Level.MinHeight;
            level.objectMap = new MapObject[level.height, level.width];
            for (int i = 0; i < level.height; i++) {
                for (int j = 0; j < level.width; j++) {
                    level.objectMap[i, j] = MapObject.Floor;
                }
            }
        }
    }

    // Next methods are empty because all this things are already on the map
    public void PlaceOre(Level level, MapObject[] ores, int[] oreCounts, float[] chunkChances) {
        return;
    }

    public void PlaceItems(Level level, int itemCount) {
        throw new System.NotImplementedException();
    }

    public void SetPlayerPosition(Level level) {
        return;
    }

    public void SetExitPosition(Level level) {
        return;
    }

    public void PlaceSpawners(Level level, MapObject[] spawners, int[] counts) {
        return;
    }

    private void ProcessFile(BinaryReader file, Level level) {
        var bytes = System.Text.Encoding.Default.GetString(file.ReadBytes(3));
        if (bytes != "LVL") {
            throw new System.Exception("$Incorrect file {bytes}");
        }

        file.ReadByte();

        level.width = file.ReadInt32();
        level.height = file.ReadInt32();

        GameLogger.LogMessage("Size: " + level.width + " " + level.height, "ReaderGenerator");
        file.ReadByte();

        level.objectMap = new MapObject[level.height, level.width];
        for (int i = 0; i < level.height; i++) {
            for (int j = 0; j < level.width; j++) {
                level.objectMap[i, j] = (MapObject)file.ReadInt32();
            }
        }
    }
}
