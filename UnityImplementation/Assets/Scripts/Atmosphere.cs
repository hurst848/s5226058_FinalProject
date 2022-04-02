using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*public class AtmosphereSettings : ScriptableObject
{
    public Shader atmosphereShader;
    public int InScatteringPoints = 10;
    public int OpticalDepthPoints = 10;
    public float DensityFalloff = 4;
    public float AtmosphereScale = 1;
    public Vector3 WaveLengths = new Vector3(700, 530, 440);
    public float ScatteringStrength = 1;

    public void SetProperties(Material m, float bodyRadius)
    {
        float scatterR = Mathf.Pow(400 / wavelengths.x, 4) * ScatteringStrength;
        float scatterG = Mathf.Pow(400 / wavelengths.y, 4) * ScatteringStrength;
        float scatterB = Mathf.Pow(400 / wavelengths.z, 4) * ScatteringStrength;

        Vector3 ScatteringCoefficant = new Vector3(scatterR, scatterB, scatterG);

        m.SetVector("scatteringCoefficients", ScatteringCoefficant);
        m.SetInt("numInScatteringPoints", InScatteringPoints);
        m.SetInt("numOpticalDepthPoints", OpticalDepthPoints);
        m.SetFloat("atmosphereRadius", (1 + atmosphereScale) * bodyRadius);
        m.SetFloat("planetRadius", bodyRadius);
        m.SetFloat("densityFalloff", DensityFalloff);

    }
}*/