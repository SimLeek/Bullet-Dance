using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudField : DensityGenerator
{
    [Header("Noise")]
    public int seed;
    public int numOctaves = 4;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float noiseScale = 1;
    public float noiseWeight = 1;
    public float weightMultiplier = 1;
    public Vector3 xyzScale = Vector3.one;

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing)
    {
        buffersToRelease = new List<ComputeBuffer>();

        // Noise parameters
        var prng = new System.Random(seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++)
        {
            offsets[i] = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1) * offsetRange;
        }

        var offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
        offsetsBuffer.SetData(offsets);
        buffersToRelease.Add(offsetsBuffer);

        densityShader.SetVector("centre", new Vector4(centre.x, centre.y, centre.z));
        densityShader.SetInt("octaves", Mathf.Max(1, numOctaves));
        densityShader.SetFloat("lacunarity", lacunarity);
        densityShader.SetFloat("persistence", persistence);
        densityShader.SetFloat("noiseScale", noiseScale);
        densityShader.SetFloat("noiseWeight", noiseWeight);
        densityShader.SetBuffer(0, "offsets", offsetsBuffer);
        densityShader.SetFloat("weightMultiplier", weightMultiplier);
        densityShader.SetVector("xyzScale", new Vector4(xyzScale.x, xyzScale.y, xyzScale.z));

        return base.Generate(pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}
