using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlaneHeightMapGeneration : MonoBehaviour
{
    public MeshRenderer planeRenderer;
    private Noise noise;
    private FractalNoise fnoise;
    private PerlinFractalNoise pfNoise;

    [Header("Levels of Detail"), Range(0, 100)]
    public int LevelOfDetail = 1;

    [Header("Noise Settings")]
    public float offset = 0.0f;
    public float scale = 1000.0f;
    public int octaves = 5;
    // Start is called before the first frame update
    void Start()
    {
        fnoise = new FractalNoise();
        pfNoise = new PerlinFractalNoise();
        noise = new Noise();// Random.Range(int.MinValue, int.MaxValue) - Time.frameCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        int lodMappedVal = (int)map(LevelOfDetail, 0, 100, 10, 1024);
        Texture2D heightmapTexture = GenerateTexture(lodMappedVal);
        heightmapTexture.filterMode = FilterMode.Point;
        planeRenderer.material.mainTexture = heightmapTexture;

    }

    Texture2D GenerateTexture(int size)
    {
        fnoise.Octaves = octaves;

        Texture2D texture = new Texture2D(size, size);
        float pixColour = 0.0f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float noiseVal = Mathf.PerlinNoise(i / scale,j / scale);//noise.Evaluate(new Vector3(i / scale, j / scale, offset));
                pixColour = map(noiseVal, -1.0f, 1.0f, 0.0f, 1.0f);
                Color c = new Color(pixColour, pixColour, pixColour);

                texture.SetPixel(i, j, c);
            }
        }
        texture.Apply();
        return texture;
    }


    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }

}
