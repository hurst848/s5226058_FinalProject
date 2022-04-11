using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalNoise
{
    public int Seed;
    public int Octaves = 2;
    public int OctaveStep = 10;
    private Random.State PastSeed;
    public FractalNoise()
    {
        PastSeed = Random.state;
        Seed = Random.Range(int.MinValue, int.MaxValue);
    }
    public FractalNoise(int _Seed)
    {
        PastSeed = Random.state;
        Seed = _Seed;
    }

    public float Evaluate(int x, int y)
    {
        float rtrn = 0.0f;
        
        for (int i = 0; i < Octaves; i++)
        {
            Random.InitState(Seed + (i * OctaveStep));
            float tmp = Random.Range(0.0f + (float)x, 10000.0f - (float)y);
            rtrn += tmp;
        }

        rtrn /= (float)Octaves;

        rtrn = map(rtrn, 0.0f, 10000.0f, 0.0f, 1.0f);
        
        Random.state = PastSeed;
        return rtrn;
    }

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }
}

public class PerlinFractalNoise
{
    public int Seed;
    public int Octaves = 2;
    public int OctaveStep = 10;
    private Random.State PastSeed;
    public PerlinFractalNoise()
    {
        PastSeed = Random.state;
        Seed = Random.Range(int.MinValue, int.MaxValue);
    }
    public PerlinFractalNoise(int _Seed)
    {
        PastSeed = Random.state;
        Seed = _Seed;
    }

    public float Evaluate(int x, int y)
    {
        float rtrn = 0.0f;

        for (int i = 0; i < Octaves; i++)
        {
            Random.InitState(Seed + (i * OctaveStep));
            float tmp = Mathf.PerlinNoise(x, y);
            rtrn += tmp;
        }

        rtrn /= (float)Octaves;

        rtrn = map(rtrn, 0.0f, 10000.0f, 0.0f, 1.0f);

        Random.state = PastSeed;
        return rtrn;
    }

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }
}
