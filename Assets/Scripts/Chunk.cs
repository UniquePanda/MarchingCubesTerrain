using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
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

    void Awake() {
        CreateBuffers();
    }

    void OnDestroy() {
        ReleaseBuffers();
    }

    void Start() {
        weights = noiseGenerator.GetNoise();

        meshFilter.sharedMesh = ConstructMesh();
    }

    /*
    void OnDrawGizmos() {
        if (weights == null || weights.Length == 0) {
            return;
        }

        for (int x = 0; x < GridMetrics.PointsPerChunk; x++) {
            for (int y = 0; y < GridMetrics.PointsPerChunk; y++) {
                for (int z = 0; z < GridMetrics.PointsPerChunk; z++) {
                    int index = x + GridMetrics.PointsPerChunk * (y + GridMetrics.PointsPerChunk * z);
                    float noiseValue = weights[index];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
                    Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * 0.2f);
                }
            }
        }
    }
    */

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

    Mesh ConstructMesh() {
        marchingCubesShader.SetBuffer(0, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(0, "_Weights", weightsBuffer);

        marchingCubesShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        marchingCubesShader.SetFloat("_IsoLevel", 0.5f);

        weightsBuffer.SetData(weights);
        trianglesBuffer.SetCounterValue(0);

        marchingCubesShader.Dispatch(0, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread);

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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.RecalculateNormals();

        return mesh;
    }
}
