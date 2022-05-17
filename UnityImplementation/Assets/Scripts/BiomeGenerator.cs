using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public List<Biome> Biomes;

    public int BiomeMapResolution = 50;
    public float Radius;
    public float MaximumTerrainHeight;
    public float TemperatureWeight= 1.0f;

    private List<float> temperatureData;
    public int[] biomeMap;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateBiomeMap()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        PerlinWindSimulation pws = new PerlinWindSimulation();
        pws.NoiseScale = 50.1f;
        pws.GenerateWind(Random.Range(0, 10000), BiomeMapResolution);

        temperatureData = new List<float>();

        biomeMap = new int[BiomeMapResolution * BiomeMapResolution * 6];
        float NoiseScale = 10;
        Vector3[] normals = new Vector3[] { new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, -1) };
        //! Set the poles
        Vector3 NorthPole = new Vector3(0, 1, 0) * Radius;
        Vector3 SouthPole = new Vector3(0, -1, 0) * Radius;
        Vector3 EquatorPole = new Vector3(1, 0, 0) * Radius;
        float MaximumDistance = Mathf.Abs((NorthPole - EquatorPole).magnitude);
        int titr = 0;
        for (int i = 0; i < 6; i++)
        {
            // Calculate temperature at location
            Vector3 _normal = normals[i];
            Vector3 AxisA = new Vector3(_normal.y, _normal.z, _normal.x);
            Vector3 AxisB = Vector3.Cross(_normal, AxisA);

            float increment = 1.0f / BiomeMapResolution;

            for (int y = 0; y < BiomeMapResolution; y++)
            {
                for (int x = 0; x < BiomeMapResolution; x++)
                {

                    int itr = x + (y * BiomeMapResolution);
                    Vector2 percent = new Vector2(y, x) / (BiomeMapResolution - 1);
                    Vector3 pointOnUnitCube = _normal + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = (pointOnUnitCube).normalized;
                    Vector3 TerrainVertex = pointOnUnitSphere * mapValues(snoise(pointOnUnitSphere * NoiseScale), 0.0f, 1.0f, Radius, Radius + MaximumTerrainHeight);

                    Vector3 pointOnGround = pointOnUnitSphere * Radius;
                    float DistanceToNorthPole = Mathf.Abs((NorthPole - pointOnGround).magnitude);
                    float DistanceToSouthPole = Mathf.Abs((SouthPole - pointOnGround).magnitude);
                    float DistanceToPole = 0.0f;
                    if (DistanceToNorthPole <= MaximumDistance) { DistanceToPole = DistanceToNorthPole; }
                    else { DistanceToPole = DistanceToSouthPole; }
                    DistanceToPole = mapValues(DistanceToPole, 0.0f, MaximumDistance, 0.0f, 2 - TemperatureWeight);
                    //! Calculate the Height value
                    float HeightFromGround = mapValues((TerrainVertex).magnitude - Radius, 0.0f, MaximumTerrainHeight, 0.0f, TemperatureWeight);
                    //! Combine values and map to a 0.0 to 1.0 value
                    float TemperatureAtVertex = (DistanceToPole + HeightFromGround) / 2.0f;
                    temperatureData.Add(TemperatureAtVertex);
                    titr++;
                }
            }
        }
        Debug.Log("asdfasdf");

        // Set the biome map to -1 (default value)
        for (int i = 0; i < BiomeMapResolution * BiomeMapResolution * 6; i++) { biomeMap[i] = -1; }
        // Compute Biome from Wind and temperature
        for (int i = 0; i < BiomeMapResolution * BiomeMapResolution * 6; i++)
        {
            float temperature = temperatureData[i];
            float moisture = ((1 - temperature) + pws.Wind[i]) / 2.0f;
            int[] validBiomes = new int[50]; for (int j = 0; j < 50; j++) { validBiomes[j] = -1; }
            int numPossibleBiomes = 0;
            for (int biomeItr = 0; biomeItr < Biomes.Count; biomeItr++)
            {
                if (moisture >= Biomes[biomeItr].MinimumMoistureSelection && moisture <= Biomes[biomeItr].MaximumMoistureSelection) // Check if it is the correct moisture setting
                {
                    if (temperature > Biomes[biomeItr].MinimumHeatSelection && temperature < Biomes[biomeItr].MaximumHeatSelection) // Check if it is the correct heat setting
                    {
                        validBiomes[numPossibleBiomes] = biomeItr;
                        numPossibleBiomes++;
                    }
                }

            }
            if (numPossibleBiomes == 1)
            {
                biomeMap[i] = validBiomes[0];
            }
            else if (numPossibleBiomes > 1)
            {
                int bestBiome = -1;
                int maxNumNeighbours = -1;
                for (int j = 0; j < numPossibleBiomes; j++)
                {
                    int numberOfNeigbours = 0;
                    Vector2Int coord = getXYCoord(i);
                    if (getBiome(new Vector2Int(coord.x + 1, coord.y)) == validBiomes[j] || getBiome(new Vector2Int(coord.x + 1, coord.y)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x + 1, coord.y - 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x + 1, coord.y - 1)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x, coord.y - 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x, coord.y - 1)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x - 1, coord.y - 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x - 1, coord.y - 1)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x - 1, coord.y)) == validBiomes[j] || getBiome(new Vector2Int(coord.x - 1, coord.y)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x - 1, coord.y + 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x - 1, coord.y + 1)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x, coord.y + 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x, coord.y + 1)) == -1) { maxNumNeighbours++; }
                    if (getBiome(new Vector2Int(coord.x + 1, coord.y + 1)) == validBiomes[j] || getBiome(new Vector2Int(coord.x + 1, coord.y + 1)) == -1) { maxNumNeighbours++; }
                    if (numberOfNeigbours > maxNumNeighbours) { maxNumNeighbours = numberOfNeigbours; bestBiome = validBiomes[j]; }
                }
                biomeMap[i] = bestBiome;
            }
            else
            {
                biomeMap[i] = -1;
            }
        }

        Debug.Log("asdfasdf");

    }

    private float mapValues(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }
    private float snoise(Vector3 _inp)
    {
        float rtrn = 0.0f;
        rtrn = Mathf.PerlinNoise(_inp.x + 0.01f, _inp.y + 0.01f);
        return rtrn;

    }

    private Vector2Int getXYCoord(int _itr)
    {
        if (_itr < BiomeMapResolution * BiomeMapResolution)
        {
            return new Vector2Int(BiomeMapResolution + (_itr % BiomeMapResolution), _itr / BiomeMapResolution);
        }
        if (_itr >= BiomeMapResolution * BiomeMapResolution && _itr < BiomeMapResolution * BiomeMapResolution * 4)
        {
            return new Vector2Int(_itr % BiomeMapResolution * 3 - (BiomeMapResolution * BiomeMapResolution), _itr / BiomeMapResolution);
        }
        else
        {
            return new Vector2Int(BiomeMapResolution + (_itr % BiomeMapResolution), _itr / BiomeMapResolution);
        }
        return new Vector2Int(-1, -1);
    }
    private int getBiome(Vector2Int _coord)
    {
        if (_coord.x == -1 && _coord.y == -1)
        {
            return -1;
        }
        if (_coord.x >= BiomeMapResolution && _coord.x < BiomeMapResolution * 2 && _coord.y < BiomeMapResolution)
        {
            return biomeMap[(_coord.y * BiomeMapResolution) + (_coord.x + BiomeMapResolution)];
        }
        if (_coord.y >= BiomeMapResolution && _coord.y < BiomeMapResolution * 2)
        {
            return biomeMap[_coord.x + (_coord.y * BiomeMapResolution)];
        }
        if (_coord.y >= BiomeMapResolution * 2 && _coord.x >= BiomeMapResolution && _coord.x < BiomeMapResolution * 2)
        {
            return biomeMap[(_coord.y * BiomeMapResolution) + (_coord.x + BiomeMapResolution)];
        }
        return -1;
    }
}


public class PerlinWindSimulation
{

    public List<float> Wind;

    public float NoiseScale = 10;

    private Vector2[] WindMap;

    public void GenerateWind(int _seed, int _resolution)
    {
        Wind = new List<float>();
        for (int y = 0; y < _resolution * 4; y++)
        {
            for (int x = 0; x < _resolution * 3; x++)
            {
                if (y < _resolution && x >= _resolution && x < _resolution * 2) { Wind.Add(Mathf.PerlinNoise(((float)x + (float)_seed + 0.01f) * (NoiseScale), ((float)y + (float)_seed + 0.01f) / (NoiseScale * _seed))); }
                if (y >= _resolution && y < _resolution * 2) { Wind.Add(Mathf.PerlinNoise(((float)x + (float)_seed + 0.01f) * (NoiseScale), ((float)y + (float)_seed + 0.01f) / (NoiseScale * _seed))); }
                if (y >= _resolution * 2 && x >= _resolution && x < _resolution * 2) { Wind.Add(Mathf.PerlinNoise(((float)x + (float)_seed + 0.01f) * (NoiseScale), ((float)y + (float)_seed + 0.01f) / (NoiseScale * _seed))); }
            }
        }
    }

}


