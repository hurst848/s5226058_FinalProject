using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(MeshFilter))]
public class GPUPlanetRenderer : MonoBehaviour
{
    // Size of Moon //
    /*
     * 1e+07
     * 1737400
     * 
     */



    // Start is called before the first frame update
    [SerializeField]
    public DVec3 GalacticPosition;

    [Header("Shaders")]
    public ComputeShader vertexGenerationShader;
    public ComputeShader terrainGenerationShader;


    [Header("Generation Settings")]
    public int Radius;
    public int BaseResolution = 100;
    public float MaximumTerrainHeight = 100;

    public float WaterLevel;
    public Gradient TestTererainGradient;

    [Header("LOD Settings")]
    [Range(1, 255)]
    public int LevelOfDetail = 1;

    [Header("Base Generation Parameters")]
    public float BaseNoiseScale = 1000;
    public Vector3 BaseOffset = new Vector3(0, 0, 0);

    [Header("BiomeSettings")]
    public List<Biome> Biomes_Settings;

    public int BiomeMapResolution = 50;
    [Range(0.0f, 2.0f)]
    public float TemperatureWeight; // Closer to zero favours Equator, closer to 2 favours height
    public int WindMapResolution;
    [Range(0,100)]
    public int NumberOfWindNodes;
    [Range(0.0f, 100.0f)]
    public float WindNodePowerMax;
    public int NumberOfWindIterations;
    public float WindMaxDeviation;
    private List<int> Biome_Map;

    [Header("Atmosphere Settings")]
    public bool AtmosphereEnabled;
    public Atmosphere atmosphere;
    [SerializeField]
    public ForwardRendererData data;
    public AtmosphereSettings atmosSettings;
    public Camera mainCamera;

    private CameraScript cam;
    private MeshFilter planetMeshFilter;

    private int PlanetDataSize;

    private MeshFilter filter;
    private bool MeshUpdate;
    private Mesh TerrainMesh;
    private int renderNumber;


    enum BiomeReference
    {
        Mountain,
        Aquatic,
        Grassland,
        Forest,
        Tundra
    }

    struct PlanetData
    {
        public float radius;
        public int resolution;
        public int numNormals;

        public string toString()
        {
            string rtrn =
                "Radius: " + radius.ToString() + " || " +
                "Resolution: " + resolution.ToString() + " || " +
                "NumNormals: " + numNormals.ToString();
            return rtrn;
        }
    };
    public struct vec3
    {
        public static vec3 toVec3(Vector3 _vec)
        {
            vec3 rtrn = new vec3();
            rtrn.x = _vec.x;
            rtrn.y = _vec.y;
            rtrn.z = _vec.z;
            return rtrn;
        }
        public string toString()
        {
            string rtrn =
                x.ToString() + " " +
                y.ToString() + " " +
                z.ToString() + " ";
            return rtrn;
        }

        public float x;
        public float y;
        public float z;
    };
    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        GalacticPosition = new DVec3(transform.position);
        PlanetDataSize = sizeof(float) + (2 * (sizeof(int)));
    }

    void Start()
    {
        GenerateBiomes();
        cam = GameObject.FindObjectOfType<CameraScript>();
        if (cam == null)
        {
            throw new System.Exception();
        }

        mainCamera = Camera.main;
        AtmosphereEnabled = true;
        atmosphere = new Atmosphere();
        atmosphere.featureName = "NewBlitMaterialFeature";
        atmosphere.rendererData = data;
        MeshUpdate = false;        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(GenerateVerticiesLOD());
        if (MeshUpdate)
        {
            filter.mesh = TerrainMesh;
            MeshUpdate = false;
        }
        if (AtmosphereEnabled)
        {
            //atmosphere.updateMaterial(this);
        }

    }

    IEnumerator GenerateVerticies()
    {
        int renderChance = 0;
        List<vec3> facesToBeRendered = new List<vec3>();
        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(transform.up)); renderChance += 1; }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(-transform.up)); renderChance += 10; }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(transform.right)); renderChance += 100; }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(-transform.right)); renderChance += 1000; }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(transform.forward)); renderChance += 10000; }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(vec3.toVec3(-transform.forward)); renderChance += 100000; }
        if (renderChance != renderNumber)
        {            
            renderNumber = renderChance;
            PlanetData[] pd = new PlanetData[1];
            pd[0].numNormals = facesToBeRendered.Count;
            pd[0].radius = Radius;
            pd[0].resolution = BaseResolution;

            CameraViewFructrum cvf = new CameraViewFructrum(mainCamera);

            var planetDataBuffer = new ComputeBuffer(1, PlanetDataSize);
            planetDataBuffer.SetData(pd);
            vertexGenerationShader.SetBuffer(0, "Data", planetDataBuffer);

            var normalDataBuffer = new ComputeBuffer(facesToBeRendered.Count, 3 * sizeof(float));
            normalDataBuffer.SetData(facesToBeRendered);
            vertexGenerationShader.SetBuffer(0, "Normals", normalDataBuffer);

            var vertexDataBuffer = new ComputeBuffer(BaseResolution * BaseResolution * facesToBeRendered.Count, 3 * sizeof(float));
            vertexGenerationShader.SetBuffer(0, "Verticies", vertexDataBuffer);

            var triangleDataBuffer = new ComputeBuffer(((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count, sizeof(int));
            vertexGenerationShader.SetBuffer(0, "Triangles", triangleDataBuffer);

            vertexGenerationShader.SetVector("cameraPosition", cam.transform.position);

            vertexGenerationShader.SetVector("CamRight", cvf.Right);
            vertexGenerationShader.SetVector("CamLeft", cvf.Left);
            vertexGenerationShader.SetVector("CamUp", cvf.Up);
            vertexGenerationShader.SetVector("CamDown", cvf.Down);

            int ki = vertexGenerationShader.FindKernel("CSMain");
            vertexGenerationShader.Dispatch(ki, 8, 8, 1);

            Vector3[] verticies = new Vector3[BaseResolution * BaseResolution * facesToBeRendered.Count];
            vertexDataBuffer.GetData(verticies);

            int[] triangles = new int[((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count];
            triangleDataBuffer.GetData(triangles);


            TerrainMesh = new Mesh();
            TerrainMesh.SetVertices(verticies);
            TerrainMesh.SetTriangles(triangles, 0);
            TerrainMesh.OptimizeReorderVertexBuffer();

            TerrainMesh.RecalculateNormals();
            TerrainMesh.RecalculateBounds();

            planetDataBuffer.Dispose();
            normalDataBuffer.Dispose();
            vertexDataBuffer.Dispose();
            triangleDataBuffer.Dispose();

            yield return new WaitForEndOfFrame();

            // Add Noise and Terrain Features

            /*var terrainVertexDataBuffer = new ComputeBuffer(TerrainMesh.vertices.Length, 3 * sizeof(float));
            vec3[] vertData = new vec3[TerrainMesh.vertices.Length];
            var terrainNormalDataBuffer = new ComputeBuffer(TerrainMesh.normals.Length, 3 * sizeof(float));
            vec3[] normData = new vec3[TerrainMesh.normals.Length];

            for (int i = 0; i < normData.Length; i++)
            {
                vertData[i] = vec3.toVec3(TerrainMesh.vertices[i]);
                normData[i] = vec3.toVec3(TerrainMesh.normals[i]);
            }

            terrainGenerationShader.SetBuffer(0, "Verticies", terrainVertexDataBuffer);
            terrainGenerationShader.SetBuffer(0, "Normals", terrainNormalDataBuffer);

            terrainGenerationShader.SetInt("vertCount", vertData.Length);
            terrainGenerationShader.SetFloat("Radius", Radius);
            terrainGenerationShader.SetFloat("MaxTerrainHeight", MaximumTerrainHeight);

            ki = terrainGenerationShader.FindKernel("CSMain");
            terrainGenerationShader.Dispatch(ki, 8, 8, 1);

            verticies = new Vector3[TerrainMesh.vertices.Length];
            terrainVertexDataBuffer.GetData(verticies);

            TerrainMesh.SetVertices(verticies);
            TerrainMesh.SetTriangles(triangles, 0);
            TerrainMesh.RecalculateNormals();
            TerrainMesh.RecalculateBounds();

            terrainNormalDataBuffer.Dispose();
            terrainVertexDataBuffer.Dispose();*/

            MeshUpdate = true;
        }





        yield return null;
    }

    IEnumerator GenerateVerticiesLOD()
    {
        //! Adds the planetary data buffer to the shader
        PlanetData[] pd = new PlanetData[1];
        pd[0].radius = Radius;
        pd[0].resolution = BaseResolution;
        pd[0].numNormals = 5;

        var planetDataBuffer = new ComputeBuffer(1, sizeof(float) + (2 * (sizeof(int))));
        planetDataBuffer.SetData(pd);
        vertexGenerationShader.SetBuffer(0, "Data", planetDataBuffer);

        //! Adds the single normal to the shader
        List<vec3> facesToBeRendered = new List<vec3>();
        Vector3 tmpl = -(transform.position - Camera.main.transform.position).normalized;
        facesToBeRendered.Add(vec3.toVec3(tmpl));


        /*Vector3 perpendicularA;
        if (tmpl != Vector3.up) { perpendicularA = Vector3.Cross(tmpl, Vector3.up); }
        else { perpendicularA = Vector3.Cross(tmpl, Vector3.right); }
        Vector3 perpendicularB = Vector3.Cross(tmpl, perpendicularA);

        facesToBeRendered.Add(vec3.toVec3(perpendicularA));
        facesToBeRendered.Add(vec3.toVec3(-perpendicularA));
        facesToBeRendered.Add(vec3.toVec3(perpendicularB));
        facesToBeRendered.Add(vec3.toVec3(-perpendicularB));*/

        Vector3 AxisA = new Vector3(tmpl.y, tmpl.z, tmpl.x);
        Vector3 AxisB = Vector3.Cross(tmpl, AxisA);
        facesToBeRendered.Add(vec3.toVec3(AxisA.normalized));
        facesToBeRendered.Add(vec3.toVec3(-AxisA.normalized));
        facesToBeRendered.Add(vec3.toVec3(AxisB.normalized));
        facesToBeRendered.Add(vec3.toVec3(-AxisB.normalized));


        var normalDataBuffer = new ComputeBuffer(facesToBeRendered.Count, 3 * sizeof(float));
        normalDataBuffer.SetData(facesToBeRendered);
        vertexGenerationShader.SetBuffer(0, "Normals", normalDataBuffer);

        //! Add emtpy vertex buffer to the shader
        var vertexDataBuffer = new ComputeBuffer(BaseResolution * BaseResolution * facesToBeRendered.Count, 3 * sizeof(float));
        vertexGenerationShader.SetBuffer(0, "Verticies", vertexDataBuffer);

        //! Add empty triangle buffer to the shader
        var triangleDataBuffer = new ComputeBuffer(((BaseResolution - 1) * (BaseResolution - 1)) *6* facesToBeRendered.Count, sizeof(int));
        vertexGenerationShader.SetBuffer(0, "Triangles", triangleDataBuffer);

        //! Add Misc varibles to the shader
        vertexGenerationShader.SetFloat("maximumTerrainHeight", MaximumTerrainHeight);

        vertexGenerationShader.SetFloat("Base_NoiseScale", BaseNoiseScale);
        vertexGenerationShader.SetVector("Base_Offset", BaseOffset);
        vertexGenerationShader.SetInt("LevelOfDetail", LevelOfDetail);


        //! Get kernel ID and dispatch
        int ki = vertexGenerationShader.FindKernel("CSMain");
        vertexGenerationShader.Dispatch(ki, 8, 8, 1);

        //! Retrive the Vertices and triangels from the completed shader
        Vector3[] verticies = new Vector3[BaseResolution * BaseResolution * facesToBeRendered.Count];
        vertexDataBuffer.GetData(verticies);

        int[] triangles = new int[((BaseResolution - 1) * (BaseResolution - 1)) * 6* facesToBeRendered.Count];
        triangleDataBuffer.GetData(triangles);

        List<Color> colourz = new List<Color>(10000);
        for (int i = 0; i < colourz.Count; i++) { colourz[i] = Color.red; }
        

        //! Update the mesh
        TerrainMesh = new Mesh();
        TerrainMesh.indexFormat = IndexFormat.UInt32;
        TerrainMesh.SetVertices(verticies);
        TerrainMesh.SetTriangles(triangles, 0);
        TerrainMesh.SetColors(colourz);
        TerrainMesh.OptimizeReorderVertexBuffer();
        TerrainMesh.RecalculateNormals();
        TerrainMesh.RecalculateBounds();

        //! Release and dispose of all the shader buffers
        planetDataBuffer.Release();
        planetDataBuffer.Dispose();
        normalDataBuffer.Dispose();
        vertexDataBuffer.Dispose();
        triangleDataBuffer.Dispose();

        MeshUpdate = true;
        yield return new WaitForEndOfFrame();

        yield return null;
    }


    void GenerateBiomes()
    {

        BiomeGenerator generator = new BiomeGenerator();
        generator.BiomeMapResolution = BiomeMapResolution;
        generator.Biomes = Biomes_Settings;
        generator.MaximumTerrainHeight = MaximumTerrainHeight;
        generator.Radius = Radius;
        generator.TemperatureWeight = TemperatureWeight;
        generator.GenerateBiomeMap();
        Biome_Map = new List<int>(generator.biomeMap);

        int numdb = 0;
        for (int i = 0; i < Biome_Map.Count; i++)
        {
            if (Biome_Map[i] != -1)
            {
                numdb++;
            }
        }
        Debug.Log(numdb);

    }


    public void MovePlanet(Vector3 _inp)
    {
        transform.position += _inp;
        GalacticPosition += _inp;
    }



}











