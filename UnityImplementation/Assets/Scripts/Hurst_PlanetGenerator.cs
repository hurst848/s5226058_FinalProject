using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Hurst_PlanetGenerator : MonoBehaviour
{
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
    private List<int> Biome_Map;

    [Header("Atmosphere Settings")]
    public bool AtmosphereEnabled;
    public Atmosphere atmosphere;
    [SerializeField]
    public ForwardRendererData ForwardRenderData;
    public AtmosphereSettings AtmosSettings;
    public Camera MainCamera;

    private CameraScript Cam;
    private MeshFilter PlanetMeshFilter;

    private int PlanetDataSize;

    private MeshFilter Filter;
    private bool MeshUpdate;
    private Mesh TerrainMesh;
    private int RenderNumber;
    public bool render = false;


    private void Awake()
    {
        Filter = GetComponent<MeshFilter>();
        GalacticPosition = new DVec3(transform.position);
        PlanetDataSize = sizeof(float) + (2 * (sizeof(int)));
    }
    private void Start()
    {
        GenerateBiomes();
        Cam = GameObject.FindObjectOfType<CameraScript>();
        if (Cam == null)
        {
            throw new System.Exception();
        }

        MainCamera = Camera.main;
        AtmosphereEnabled = true;
        atmosphere = new Atmosphere();
        atmosphere.featureName = "NewBlitMaterialFeature";
        atmosphere.rendererData = ForwardRenderData;
        MeshUpdate = false;
        render = true;
    }
    private void Update()
    {
        if (render)
        {
            StartCoroutine(GenerateMesh());
            if (MeshUpdate)
            {
                Filter.mesh = TerrainMesh;
                MeshUpdate = false;
            }
            if (AtmosphereEnabled)
            {
                atmosphere.updateMaterial(this);
            } 
        }
    }

    private IEnumerator GenerateMesh()
    {
        //! Adds the planetary data buffer to the shader
            PlanetData[] pd = new PlanetData[1];
            pd[0].radius = Radius;
            pd[0].resolution = BaseResolution;
            pd[0].numNormals = 5;

            var planetDataBuffer = new ComputeBuffer(1, sizeof(float) + (2 * (sizeof(int))));
            planetDataBuffer.SetData(pd);
            vertexGenerationShader.SetBuffer(0, "Data", planetDataBuffer);
        //! Adds the face normals to the shader
            List<vec3> facesToBeRendered = new List<vec3>();
            Vector3 tmpl = -(transform.position - Camera.main.transform.position).normalized;
            facesToBeRendered.Add(vec3.toVec3(tmpl));

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
            var triangleDataBuffer = new ComputeBuffer(((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count, sizeof(int));
            vertexGenerationShader.SetBuffer(0, "Triangles", triangleDataBuffer);

        //! Add the biomes to the shader
            var biomeDataBuffer = new ComputeBuffer(Biomes_Settings.Count, 6 * sizeof(float));
            Biome.SerializedBiome[] sba = new Biome.SerializedBiome[Biomes_Settings.Count];
            for (int i = 0; i < Biomes_Settings.Count; i++) { sba[i] = Biomes_Settings[i].ReturnSerializedBiome(); }
            biomeDataBuffer.SetData(sba);
            vertexGenerationShader.SetBuffer(0, "Biomes", biomeDataBuffer);

        //! Add the biome map to the shader
            var biomeMapDataBuffer = new ComputeBuffer(BiomeMapResolution * BiomeMapResolution * 6, sizeof(int));
            biomeMapDataBuffer.SetData(Biome_Map);
            vertexGenerationShader.SetBuffer(0, "BiomeMap", biomeMapDataBuffer);

        //! Add biome offsets to shader
            var biomeOffsetDataBuffer = new ComputeBuffer(Biomes_Settings.Count, PlanetDataSize);
            vec3[] boa = new vec3[Biomes_Settings.Count];
            for (int i = 0; i < Biomes_Settings.Count; i++) { boa[i] = vec3.toVec3(Biomes_Settings[i].BiomeNoiseOffset); }
            biomeOffsetDataBuffer.SetData(boa);
            vertexGenerationShader.SetBuffer(0, "biomeOffsets", biomeOffsetDataBuffer);

        //! Add Misc varibles to the shader
            vertexGenerationShader.SetFloat("maximumTerrainHeight", MaximumTerrainHeight);
            vertexGenerationShader.SetFloat("Base_NoiseScale", BaseNoiseScale);
            vertexGenerationShader.SetVector("Base_Offset", BaseOffset);
            vertexGenerationShader.SetInt("LevelOfDetail", LevelOfDetail);
            vertexGenerationShader.SetInt("Biome_Resolution", BiomeMapResolution);
            vertexGenerationShader.SetFloat("AngleOff", Mathf.Sqrt(2) / 2);

        //! Get kernel ID and dispatch
            int ki = vertexGenerationShader.FindKernel("CSMain");
            vertexGenerationShader.Dispatch(ki, 8, 8, 1);

        //! Retrieve the Vertices from the completed shader
            Vector3[] verticies = new Vector3[BaseResolution * BaseResolution * facesToBeRendered.Count];
            vertexDataBuffer.GetData(verticies);

        //! Retrieve the Triangles from the completed shader
            int[] triangles = new int[((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count];
            triangleDataBuffer.GetData(triangles);

        //! Create and Update the Mesh
            TerrainMesh = new Mesh();
            TerrainMesh.indexFormat = IndexFormat.UInt32;
            TerrainMesh.SetVertices(verticies);
            TerrainMesh.SetTriangles(triangles, 0);
            TerrainMesh.OptimizeReorderVertexBuffer();
            TerrainMesh.RecalculateNormals();
            TerrainMesh.RecalculateBounds();

        //! Release and dipose of buffers 
            planetDataBuffer.Release();planetDataBuffer.Dispose();
            normalDataBuffer.Release(); normalDataBuffer.Dispose();
            vertexDataBuffer.Release(); vertexDataBuffer.Dispose();
            triangleDataBuffer.Release(); triangleDataBuffer.Dispose();
            biomeDataBuffer.Release(); biomeDataBuffer.Dispose();
            biomeMapDataBuffer.Release(); biomeMapDataBuffer.Dispose();
            biomeOffsetDataBuffer.Release(); biomeOffsetDataBuffer.Release();

        MeshUpdate = true;

        yield return null;
    }

    private void GenerateBiomes()
    {
        BiomeGenerator generator = new BiomeGenerator();
        generator.BiomeMapResolution = BiomeMapResolution;
        generator.Biomes = Biomes_Settings;
        generator.MaximumTerrainHeight = MaximumTerrainHeight;
        generator.Radius = Radius;
        generator.TemperatureWeight = TemperatureWeight;
        generator.GenerateBiomeMap();
        Biome_Map = new List<int>(generator.biomeMap);
    }

    public void MovePlanet(Vector3 _inp)
    {
        transform.position += _inp;
        GalacticPosition += _inp;
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
}
