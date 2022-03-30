using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUPlanetRenderer : MonoBehaviour 
{
    // Start is called before the first frame update
    [SerializeField]
    public DVec3 GalacticPosition;

    public ComputeShader vertexGenerationShader;

    public int Radius;
    public int BaseResolution = 100;
    public float NoiseScale = 1000;
    public float MaximumTerrainHeight = 100;

    public float WaterLevel;
    public Gradient TestTererainGradient;

    private CameraScript cam;
    private MeshFilter planetMeshFilter;

    private int PlanetDataSize;

    struct PlanetData
    {
        public float radius;
        public int resolution;
        public int numNormals;
    };


    void Start()
    {
        PlanetDataSize = sizeof(float) + 2 * (sizeof(int));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GenerateVerticies()
    {
        List<Vector4> facesToBeRendered = new List<Vector4>();
        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.up); }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.up); }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.right); }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.right); }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.forward); }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.forward); }

        PlanetData pd = new PlanetData
        {
            numNormals = facesToBeRendered.Count,
            radius = Radius,
            resolution = BaseResolution
        };


        var planetDataBuffer = new ComputeBuffer(1, PlanetDataSize);
        planetDataBuffer.SetData(pd);
        vertexGenerationShader.SetVectors("Normals", facesToBeRendered.ToArray());


        


        yield return null;
    }
}
