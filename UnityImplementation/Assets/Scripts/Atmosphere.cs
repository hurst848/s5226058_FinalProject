using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    public bool Active = true;
    public Shader shader;
    protected Material material;


    [Range(0, 1)]
    public float AtmosphereScale = 0.2f;

    public Color Colour;
    public Vector4 testParams;
    public Gradient Falloff;
    public int GradientRes = 10;
    public int NumberOfSteps = 10;
    public Texture2D BlueNoise;

    ComputeBuffer buffer;
    Texture2D falloffTexture;
    
    private GPUPlanetRenderer planet;

    struct Sphere
    {
        public Vector3 Centre;
        public float Radius;
        public float WaterLevel;

        public static int Size()
        {
            // This may break
            return sizeof(float) * 5;
        }
    }

    public Material GetMaterial()
    {
        if (material == null || material.shader != shader)
        {
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
            }
            material = new Material(shader);
        }

        Sphere sphere = new Sphere()
        { 
            Centre = planet.transform.position,
            Radius = planet.Radius,
            WaterLevel = planet.WaterLevel
        };

        buffer = new ComputeBuffer(1, Sphere.Size());
        buffer.SetData(new Sphere[] { sphere });
        material.SetBuffer("spheres", buffer);
        material.SetVector("params", testParams);
        material.SetColor("_Colour", Colour);
        material.SetFloat("planetRadius", planet.Radius);

        //planet
        material.SetTexture("_Falloff", falloffTexture);
        material.SetTexture("_BlueNoise", BlueNoise);
        material.SetInt("numSteps", NumberOfSteps);
        return material;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Release()
    {
        buffer.Release();
    }
}
