using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//https://github.com/NedMakesGames/RendererFeatureBlitMat/blob/master/Assets/Rendering/Desaturate/BlitMaterialFeature.cs
public class Atmosphere
{
    [SerializeField] public ForwardRendererData rendererData = null;
    [SerializeField] public string featureName = null;

    private ScriptableRendererFeature feature;
    private bool TryGetFeature()
    {
        feature = rendererData.rendererFeatures.Where((f) => f.name == featureName).FirstOrDefault();
        if (feature == null)
        {
            Debug.Log("NULL"); return false;
        }
        return true; ;
    }

    public void updateMaterial(Hurst_PlanetGenerator renderer)
    {
        bool test = TryGetFeature();
        if (test)
        {
            var bmf = feature as BlitMaterialFeature;
            var mat = bmf.Material;
            renderer.AtmosSettings.SetProperties(mat, renderer.Radius, renderer.transform.position);
        }
    }

}