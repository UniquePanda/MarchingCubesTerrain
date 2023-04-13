using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public ComputeShader noiseShader;

    [SerializeField] int seed = 1234;
    [SerializeField] float amplitude = 5.0f;
    [SerializeField] float frequency = 0.005f;
    [SerializeField] int octaves = 8;
    [SerializeField, Range(0f, 1f)] float groundPercent = 0.2f;

    ComputeBuffer weightsBuffer;

    void Awake() {
        CreateBuffer();
    }

    void OnDestroy() {
        ReleaseBuffer();
    }

    void CreateBuffer() {
        weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerGrid, sizeof(float));
    }

    void ReleaseBuffer() {
        weightsBuffer.Release();
    }

    public float[] GetNoise() {
        float[] noiseValues = new float[GridMetrics.PointsPerGrid];

        noiseShader.SetBuffer(0, "_Weights", weightsBuffer);

        noiseShader.SetInt("_Seed", seed);
        noiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetInt("_Octaves", octaves);
        noiseShader.SetFloat("_GroundPercent", groundPercent);

        noiseShader.Dispatch(0, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread, GridMetrics.PointsPerChunkPerThread);

        weightsBuffer.GetData(noiseValues);

        return noiseValues;
    }
}
