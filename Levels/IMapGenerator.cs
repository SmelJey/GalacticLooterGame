// header

/// <summary>
/// Interface for Map-Constructors: Readers from file, generators, etc.
/// </summary>
public interface IMapGenerator {
    void GenerateLevel(Level level);

    void PlaceOre(Level level, Level.MapObject[] ores, int[] oreCounts, float[] chunkChances);

    void PlaceItems(Level level, int itemCount);

    void SetPlayerPosition(Level level);

    void SetExitPosition(Level level);

    void PlaceSpawners(Level level, Level.MapObject[] spawners, int[] counts);
}
