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

    void CreateBuffer(int lod) {
        weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerGrid(lod), sizeof(float));
    }

    void ReleaseBuffer() {
        weightsBuffer.Release();
    }

    public float[] GetNoise(int lod) {
        CreateBuffer(lod);

        float[] noiseValues = new float[GridMetrics.PointsPerGrid(lod)];

        noiseShader.SetBuffer(0, "_Weights", weightsBuffer);

        noiseShader.SetInt("_Seed", seed);
        noiseShader.SetInt("_GroundLevel", GridMetrics.GroundLevel);
        noiseShader.SetInt("_Scale", GridMetrics.Scale);
        noiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(lod));
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetInt("_Octaves", octaves);
        noiseShader.SetFloat("_GroundPercent", groundPercent);

        noiseShader.Dispatch(0, GridMetrics.ThreadGroups(lod), GridMetrics.ThreadGroups(lod), GridMetrics.ThreadGroups(lod));

        weightsBuffer.GetData(noiseValues);

        ReleaseBuffer();

        return noiseValues;
    }
}
