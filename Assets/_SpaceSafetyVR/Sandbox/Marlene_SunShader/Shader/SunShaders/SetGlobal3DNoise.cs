using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SetGlobal3DNoise : MonoBehaviour
{
    public enum TextureResolution
    {
        _32 = 32,
        _64 = 64,
        _96 = 96,
        _128 = 128,
        _256 = 256
    }

    [Header("Texture Settings")]
    [SerializeField] private TextureResolution resolution = TextureResolution._64;

    [Header("Noise Settings")]
    [SerializeField] private float scale = 1.0f;
    [SerializeField] private float amplitude = 1.0f;
    [SerializeField] private float strength = 0.5f;
    [SerializeField] private float frequency = 2.0f;
    [SerializeField] private float gridDims = 6.0f;
    [SerializeField] private int octaves = 3;

    [Header("Shader Global Name")]
    [SerializeField] private string globalTextureName = "_Worley3D";

 
    private void Start()
    {
        int res = (int)resolution;
        Texture3D  worleyTex = new Texture3D(res, res, res, TextureFormat.RGBAHalf, false);
        worleyTex.wrapMode = TextureWrapMode.Clamp;
        worleyTex.filterMode = FilterMode.Trilinear; 
        Color[]  colors = new Color[res * res * res];

        for (int z = 0; z < res; z++)
        {
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    Vector3 pos = new Vector3(x, y, z) / res;
                    Vector3 fbm = CalculateFbmWorleyNoise(pos, scale, amplitude, strength, frequency, gridDims, octaves);
                    colors[x + y * res + z * res * res] = new Color(fbm.x, fbm.y, fbm.z, 1f);
                }
            }
        }

        worleyTex.SetPixels(colors);
        worleyTex.Apply();

        Shader.SetGlobalTexture(globalTextureName, worleyTex);
    }

   /* private void Update()
    {
        for (int z = 0; z < res; z++)
        {
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    Vector3 pos = new Vector3(x, y, z) / res;
                    Vector3 fbm = CalculateFbmWorleyNoise(pos, scale, amplitude, strength, frequency, gridDims, octaves);
                    colors[x + y * res + z * res * res] = new Color(fbm.x, fbm.y, fbm.z, 1f);
                }
            }
        }

        worleyTex.SetPixels(colors);
        worleyTex.Apply();
    }*/

    private Vector3 CalculateFbmWorleyNoise(Vector3 pos, float scale, float amplitude, float strength, float frequency, float gridDims, int octaves)
    {
        Vector3 p = pos * scale;
        Vector3 fbm = Vector3.zero;

        for (int i = 0; i < octaves; i++)
        {
            fbm += Worley3D(p, gridDims) * amplitude;
            amplitude *= strength;
            p *= frequency;
        }

        return fbm;
    }

    private Vector3 Worley3D(Vector3 pos, float gridDims)
    {
        Vector3 cellPos = pos * gridDims;
        Vector3 index = Floor(cellPos);
        Vector3 fractPos = Frac(cellPos);

        float F1 = 1f, F2 = 1f;

        for (int z = -1; z <= 1; z++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector3 neighbor = new Vector3(x, y, z);
                    Vector3 feature = Random3(index + neighbor);
                    float dist = (neighbor + feature - fractPos).magnitude;

                    if (dist < F1)
                    {
                        F2 = F1;
                        F1 = dist;
                    }
                    else if (dist < F2)
                    {
                        F2 = dist;
                    }
                }
            }
        }

        return new Vector3(F1, F2, F2 - F1);
    }

    private Vector3 Random3(Vector3 p)
    {
        return Frac(new Vector3(
            Mathf.Sin(Vector3.Dot(p, new Vector3(127.1f, 311.7f, 74.7f))),
            Mathf.Sin(Vector3.Dot(p, new Vector3(269.5f, 183.3f, 246.1f))),
            Mathf.Sin(Vector3.Dot(p, new Vector3(113.5f, 271.9f, 124.6f)))
        ) * 43758.5453f);
    }

    private Vector3 Frac(Vector3 v)
    {
        return new Vector3(v.x - Mathf.Floor(v.x), v.y - Mathf.Floor(v.y), v.z - Mathf.Floor(v.z));
    }

    private Vector3 Floor(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
    }
}
