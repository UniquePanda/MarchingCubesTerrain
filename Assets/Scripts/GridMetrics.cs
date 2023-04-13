public static class GridMetrics
{
    public const int NumThreads = 8;
    public const int PointsPerChunk = 32;
    public const int PointsPerGrid = PointsPerChunk * PointsPerChunk * PointsPerChunk;
    public const int PointsPerChunkPerThread = PointsPerChunk / NumThreads;
}
