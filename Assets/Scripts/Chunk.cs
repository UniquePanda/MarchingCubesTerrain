using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public NoiseGenerator noiseGenerator;
    public ComputeShader marchingCubesShader;

    struct Triangle {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }

    float[] weights;

    ComputeBuffer trianglesBuffer;
    ComputeBuffer trianglesCountBuffer;
    ComputeBuffer weightsBuffer;

    Mesh mesh;

    void Awake() {
        CreateBuffers();
    }

    void OnDestroy() {
        ReleaseBuffers();
    }

    void Start() {
        weights = noiseGenerator.GetNoise();

        mesh = new Mesh();
        UpdateMesh();
    }

    void CreateBuffers() {
        trianglesBuffer = new ComputeBuffer(5 * GridMetrics.PointsPerGrid, Triangle.SizeOf, ComputeBufferType.Append);
        trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerGrid, sizeof(float));
    }

    void ReleaseBuffers() {
        trianglesBuffer.Release();
        trianglesCountBuffer.Release();
        weightsBuffer.Release();
    }

    void UpdateMesh() {
        mesh = ConstructMesh();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    Mesh ConstructMesh() {
        int kernel = marchingCubesShader.FindKernel("March");

        marchingCubesShader.SetBuffer(kernel, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(kernel, "_Weights", weightsBuffer);

        marchingCubesShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        marchingCubesShader.SetFloat("_IsoLevel", 0.5f);

        weightsBuffer.SetData(weights);
        trianglesBuffer.SetCounterValue(0);

        marchingCubesShader.Dispatch(kernel, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread);

        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        trianglesBuffer.GetData(triangles);

        return CreateMeshFromTriangles(triangles);
    }

    int ReadTriangleCount() {
        int[] triangleCount = {0};
        ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
        trianglesCountBuffer.GetData(triangleCount);

        return triangleCount[0];
    }

    Mesh CreateMeshFromTriangles(Triangle[] triangles) {
        Vector3[] vertices = new Vector3[3 * triangles.Length];
        int[] triangleIndices = new int[vertices.Length];

        for (int i = 0; i < triangles.Length; i++) {
            int startIndex = i * 3;
            vertices[startIndex] = triangles[i].a;
            vertices[startIndex + 1] = triangles[i].b;
            vertices[startIndex + 2] = triangles[i].c;
            triangleIndices[startIndex] = startIndex;
            triangleIndices[startIndex + 1] = startIndex + 1;
            triangleIndices[startIndex + 2] = startIndex + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.RecalculateNormals();

        return mesh;
    }

    public void EditWeights(Vector3 hitPosition, float brushSize, bool add) {
        int kernel = marchingCubesShader.FindKernel("UpdateWeights");

        weightsBuffer.SetData(weights);
        marchingCubesShader.SetBuffer(kernel, "_Weights", weightsBuffer);

        marchingCubesShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        marchingCubesShader.SetVector("_HitPosition", hitPosition);
        marchingCubesShader.SetFloat("_BrushSize", brushSize);
        marchingCubesShader.SetFloat("_TerraformStrength", add ? 1.0f : -1.0f);

        marchingCubesShader.Dispatch(kernel, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread);

        weightsBuffer.GetData(weights);

        UpdateMesh();
    }
}
