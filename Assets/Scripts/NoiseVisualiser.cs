using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVisualiser : MonoBehaviour
{
    public NoiseGenerator noiseGenerator;

    float[] weights;

    void Start() {
        weights = noiseGenerator.GetNoise();
    }

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
}
