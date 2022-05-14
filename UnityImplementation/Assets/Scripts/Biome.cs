using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HurstPlanetGenerator/Biome Settings")]
public class Biome : ScriptableObject
{
    [Header("Selection Parameters")]
    [Range(0.0f, 1.0f)]
    public float MinimumHeatSelection = 0.0f;
    [Range(0.0f, 1.0f)]
    public float MaximumHeatSelection = 1.0f;
    [Range(0.0f, 1.0f)]
    public float MinimumMoistureSelection = 0.0f;
    [Range(0.0f, 1.0f)]
    public float MaximumMoistureSelection = 1.0f;
    [Header("Generation Parameters")]
    public Vector3 BiomeNoiseOffset = new Vector3(0, 0);
    public float BiomeNoiseScale = 1.0f;
    [Range(0.0f, 1.0f)]
    public float BiomeRelativeAltitude = 1.0f;

    public SerializedBiome ReturnSerializedBiome()
    {
        SerializedBiome rtrn;
        //vec3 tmp; tmp.x = BiomeNoiseOffset.x; tmp.y = BiomeNoiseOffset.y; tmp.z = BiomeNoiseOffset.z;
        //rtrn.BNO = tmp;
        rtrn.BNS = BiomeNoiseScale;
        rtrn.MaxHS = MaximumHeatSelection;
        rtrn.MaxMS = MaximumMoistureSelection;
        rtrn.MinHS = MinimumHeatSelection;
        rtrn.MinMS = MinimumMoistureSelection;
        rtrn.BRA = BiomeRelativeAltitude;
        return rtrn;
    }

    public const int MemSize = 6 * sizeof(float);


    public struct vec3 { public float x; public float y; public float z; }

    public struct SerializedBiome
    {
        public float MinHS;
        public float MaxHS;
        public float MinMS;
        public float MaxMS;
        //public vec3 BNO;
        public float BNS;
        public float BRA;
    }
    

}


