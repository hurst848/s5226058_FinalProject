using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneHeightMapGeneration : MonoBehaviour
{
    public MeshRenderer planeRenderer;
    private Noise noise;

    [Header("Levels of Detail"), Range(0, 100)]
    public int LevelOfDetail;
    
    // Start is called before the first frame update
    void Start()
    {
        noise = new Noise(Random.Range(int.MinValue, int.MaxValue) - Time.frameCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        int lodMappedVal = (int)map(LevelOfDetail, 0, 100, 10, 1024);
        Texture2D heightmapTexture = new Texture2D(lodMappedVal, lodMappedVal);
        heightmapTexture.filterMode = FilterMode.Point;
        float pixColour = 0.0f;

        for (int i = 0; i < lodMappedVal; i++)
        {
            for (int j = 0; j < lodMappedVal; j++)
            {
                pixColour = map(noise.Evaluate(new Vector3(i * 1000, j* 1000, 0.0f)), -1.0f, 1.0f, 0.0f, 1.0f);
                heightmapTexture.SetPixel(i, j, Color.Lerp(Color.black, Color.white, pixColour));

            }
        }
        
        planeRenderer.material.mainTexture = heightmapTexture;

    }

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }

}
