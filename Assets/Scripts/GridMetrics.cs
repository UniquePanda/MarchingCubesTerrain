public static class GridMetrics
{
    public const int NumThreads = 8;
    public const int Scale = 16;
    public const int GroundLevel = Scale / 2;

    public static int[] LODs = {8, 16, 32, 64, 128};
    public static int LastLod = LODs.Length - 1;

    public static int PointsPerChunk(int lod) {
        return LODs[lod];
    }

    public static int PointsPerGrid(int lod) {
        return PointsPerChunk(lod) * PointsPerChunk(lod) * PointsPerChunk(lod);
    }
    public static int ThreadGroups(int lod) {
        return PointsPerChunk(lod) / NumThreads;
    }
}
