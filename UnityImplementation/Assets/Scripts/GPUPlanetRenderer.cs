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
    public ComputeShader biomeGenertaionShader;

    [Header("Generation Settings")]
    public int Radius;
    public int BaseResolution = 100;
    public float MaximumTerrainHeight = 100;

    public float WaterLevel;
    public Gradient TestTererainGradient;

    [Header("LOD Settings")]
    [Range(0, 255)]
    int LevelOfDetail = 0;

    [Header("Base Generation Parameters")]
    public float BaseNoiseScale = 1000;
    public Vector3 BaseOffset = new Vector3(0, 0, 0);

    [Header("BiomeSettings")]
    public int BiomeMapResolution = 50;
    private WindSimultaion wind;
    [Range(0.0f, 2.0f)]
    public float TemperatureWeight; // Closer to zero favours Equator, closer to 2 favours height
    public int WindMapResolution;
    [Range(0,100)]
    public int NumberOfWindNodes;
    [Range(0.0f, 100.0f)]
    public float WindNodePowerMax;
    public int NumberOfWindIterations;
    public float WindMaxDeviation;

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

    void Start()
    {

        GenerateBiomes();
        GalacticPosition = new DVec3(transform.position);
        PlanetDataSize = sizeof(float) + (2 * (sizeof(int)));
        cam = GameObject.FindObjectOfType<CameraScript>();
        if (cam == null)
        {
            throw new System.Exception();
        }
        // Wind Setup and Run
        wind = new WindSimultaion();
        wind.MaxDeviation = WindMaxDeviation;
        wind.NumberOfWindIterations = NumberOfWindIterations;
        wind.NumberOfWindNodes = NumberOfWindNodes;
        wind.WindMapResolution = WindMapResolution;
        wind.WindNodePowerMax = WindNodePowerMax;
        StartCoroutine(wind.GenerateWind());

        mainCamera = Camera.main;
        AtmosphereEnabled = true;
        atmosphere = new Atmosphere();
        atmosphere.featureName = "NewBlitMaterialFeature";
        atmosphere.rendererData = data;
        MeshUpdate = false;
        filter = GetComponent<MeshFilter>();
        
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
            atmosphere.updateMaterial(this);
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
        pd[0].numNormals = 1;

        var planetDataBuffer = new ComputeBuffer(1, PlanetDataSize);
        planetDataBuffer.SetData(pd);
        vertexGenerationShader.SetBuffer(0, "Data", planetDataBuffer);

        //! Adds the single normal to the shader
        List<vec3> facesToBeRendered = new List<vec3>();
        Vector3 tmpl = -(transform.position - Camera.main.transform.position).normalized;
        facesToBeRendered.Add(vec3.toVec3(tmpl));
        Vector3 AxisA = new Vector3(tmpl.y, tmpl.z, tmpl.x);
        Vector3 AxisB = Vector3.Cross(tmpl, AxisA);
        facesToBeRendered.Add(vec3.toVec3(AxisA));
        facesToBeRendered.Add(vec3.toVec3(-AxisA));
        facesToBeRendered.Add(vec3.toVec3(AxisB));
        facesToBeRendered.Add(vec3.toVec3(-AxisB));


        var normalDataBuffer = new ComputeBuffer(facesToBeRendered.Count, 3 * sizeof(float));
        normalDataBuffer.SetData(facesToBeRendered);
        vertexGenerationShader.SetBuffer(0, "Normals", normalDataBuffer);

        //! Add emtpy vertex buffer to the shader
        var vertexDataBuffer = new ComputeBuffer(BaseResolution * BaseResolution * facesToBeRendered.Count, 3 * sizeof(float));
        vertexGenerationShader.SetBuffer(0, "Verticies", vertexDataBuffer);

        //! Add empty triangle buffer to the shader
        var triangleDataBuffer = new ComputeBuffer(((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count, sizeof(int));
        vertexGenerationShader.SetBuffer(0, "Triangles", triangleDataBuffer);

        //! Add Misc varibles to the shader
        vertexGenerationShader.SetVector("cameraPosition", cam.transform.position);
        vertexGenerationShader.SetFloat("maximumTerrainHeight", MaximumTerrainHeight);

        vertexGenerationShader.SetFloat("Base_NoiseScale", BaseNoiseScale);
        vertexGenerationShader.SetVector("Base_Offset", BaseOffset);

        vertexGenerationShader.SetFloat("TemperatureWeight", TemperatureWeight);

        //! Get kernel ID and dispatch
        int ki = vertexGenerationShader.FindKernel("CSMain");
        vertexGenerationShader.Dispatch(ki, 8, 8, 1);

        //! Retrive the Vertices and triangels from the completed shader
        Vector3[] verticies = new Vector3[BaseResolution * BaseResolution * facesToBeRendered.Count];
        vertexDataBuffer.GetData(verticies);

        int[] triangles = new int[((BaseResolution - 1) * (BaseResolution - 1)) * 6 * facesToBeRendered.Count];
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
        biomeGenertaionShader.SetFloat("TemperatureWeight",TemperatureWeight);
        biomeGenertaionShader.SetInt("BiomeMapResolution", BiomeMapResolution);
        biomeGenertaionShader.SetFloat("Radius", Radius);
        biomeGenertaionShader.SetFloat("NoiseScale", BaseNoiseScale);
        biomeGenertaionShader.SetVector("Offset", BaseOffset);
        biomeGenertaionShader.SetFloat("MaximumTerrainHeight", MaximumTerrainHeight);

        var biomeTemperatureBuffer = new ComputeBuffer(BiomeMapResolution * BiomeMapResolution * 6, 1 * sizeof(float));
        biomeGenertaionShader.SetBuffer(0, "temperatureData", biomeTemperatureBuffer);

        var biomeWindBuffer = new ComputeBuffer(BiomeMapResolution * BiomeMapResolution * 6, 2 * sizeof(float));
        biomeGenertaionShader.SetBuffer(0, "windData", biomeWindBuffer);

        //! Get kernel ID and dispatch
        int ki = biomeGenertaionShader.FindKernel("CSMain");
        biomeGenertaionShader.Dispatch(ki, 8, 8, 1);

        biomeTemperatureBuffer.Release(); biomeTemperatureBuffer.Dispose();
        biomeWindBuffer.Release(); biomeWindBuffer.Dispose();

    }


    public void MovePlanet(Vector3 _inp)
    {
        transform.position += _inp;
        GalacticPosition += _inp;
    }



}

public class WindSimultaion
{
    public int WindMapResolution;
    public int NumberOfWindNodes;
    public float WindNodePowerMax;
    public int NumberOfWindIterations;
    public float MaxDeviation;

    public Texture2D Tex;
    private bool running = false;
    public Vector2[] WindMap;

    private List<Vector2Int> tmpWindStorage;
    private Dictionary<Vector2Int, bool> windDictionary;

    Vector2[] UnitCircle =
    {
        new Vector2(0,1),                                   // North
        new Vector2(0.5f, Mathf.Sqrt(3) / 2),               // North-North-East
        new Vector2(Mathf.Sqrt(2) / 2,Mathf.Sqrt(2) / 2),   // North-East
        new Vector2(Mathf.Sqrt(3) / 2, 0.5f),               // North-East-East
        new Vector2(1,0),                                   // East
        new Vector2(Mathf.Sqrt(3) / 2, -0.5f),              // South-East-East
        new Vector2(Mathf.Sqrt(2) / 2,-Mathf.Sqrt(2) / 2),  // South-East
        new Vector2(0.5f, -Mathf.Sqrt(3) / 2),              // South-South-East
        new Vector2(0,-1),                                  // South
        new Vector2(-0.5f, -Mathf.Sqrt(3) / 2),             // South-South-West
        new Vector2(-Mathf.Sqrt(2) / 2,-Mathf.Sqrt(2) / 2), // South-West
        new Vector2(-Mathf.Sqrt(3) / 2, -0.5f),             // South-West-West
        new Vector2(-1,0),                                  // West
        new Vector2(-Mathf.Sqrt(3) / 2, 0.5f),              // North-West-West
        new Vector2(-Mathf.Sqrt(2) / 2,Mathf.Sqrt(2) / 2),  // North-West
        new Vector2(-0.5f, Mathf.Sqrt(3) / 2)               // North-North-West
    };

    public IEnumerator GenerateWind()
    {
        int XRes = WindMapResolution * 3;
        int YRes = WindMapResolution * 4;
        Tex = new Texture2D(XRes, YRes);
        for (int x = 0; x < XRes; x++) { for (int y = 0; y < YRes; y++) { Tex.SetPixel(x, y, Color.black); } }
        Tex.Apply();
        tmpWindStorage = new List<Vector2Int>();

        WindMap = new Vector2[(WindMapResolution * 3) * (WindMapResolution * 4)];
        for (int i = 0; i < WindMap.Length; i++) { WindMap[i] = Vector2.zero; }
        List<Vector2Int> windNodeOrigins = new List<Vector2Int>();
        for (int i = 0; i < NumberOfWindNodes; i++)
        {
            Vector2 rand = Random.insideUnitCircle;
            rand *= Random.Range(-WindNodePowerMax, WindNodePowerMax);
            Vector2 coord = new Vector2(Random.Range(0, (WindMapResolution * 3)), Random.Range(0, (WindMapResolution * 4)));
            if (IsWithinMap((int)coord.x, (int)coord.y))
            {
                WindMap[((int)coord.x + ((int)coord.y * (WindMapResolution * 3)))] = rand;
                windNodeOrigins.Add(new Vector2Int((int)coord.x, (int)coord.y));
            }
            else { i--; }
        }

        for (int i = 0; i < windNodeOrigins.Count; i++)
        {
            windDictionary = new Dictionary<Vector2Int, bool>();
            List<Vector2Int> windToBeSpread = new List<Vector2Int>();
            windToBeSpread.Add(windNodeOrigins[i]);
            for (int j = 0; j < NumberOfWindIterations; j++)
            {
                tmpWindStorage = new List<Vector2Int>();
                for (int node = 0; node < windToBeSpread.Count; node++)
                {
                    Texture2D StoredCalculated = new Texture2D(XRes, YRes);
                    Vector2 CurrentSample = WindMap[GetCoordinate(windToBeSpread[node].x, windToBeSpread[node].y)];
                    if (IsWithinMap(windToBeSpread[node].x, windToBeSpread[node].y) && IsNotOnEdge(windToBeSpread[node].x, windToBeSpread[node].y))
                    {
                        SpreadWind(CurrentSample, windToBeSpread[node].x, windToBeSpread[node].y, ref StoredCalculated);
                    }
                    else if (IsWithinMap(windToBeSpread[node].x, windToBeSpread[node].y) && !IsNotOnEdge(windToBeSpread[node].x, windToBeSpread[node].y)){}
                    else
                    {
                        Tex.SetPixel(windToBeSpread[node].x, windToBeSpread[node].y, new Color(0, 0, 0));
                    }
                    StoredCalculated.Apply();
                }
                windToBeSpread.Clear();
                windToBeSpread = new List<Vector2Int>();
                windToBeSpread = tmpWindStorage;
            }
            for (int j = 0; j < WindMap.Length; j++)
            {
                if (WindMap[j] != Vector2.zero)
                {
                    Tex.SetPixel(j % XRes, j / XRes, new Color(1, 1, 1));
                }
            }
            Tex.filterMode = FilterMode.Point;
            Tex.wrapMode = TextureWrapMode.Clamp;
            Tex.Apply();
        }
        running = false;
        yield return null;
    }
    private void SpreadWind(Vector2 _currentSample, int _x, int _y, ref Texture2D _storedCalculated)
    {
        Vector2 normalizedSample = _currentSample.normalized;

        float degrees = Vector2.Angle(UnitCircle[0], normalizedSample);
        bool des = false;
        if (degrees < 22.5f) // Facing North
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y + 1), out des))//_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y + 1);
                Vector2 a = WindMap[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y + 1), out des))//_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2Int tmp = new Vector2Int(_x, _y + 1);
                Vector2 b = WindMap[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y + 1), out des))//_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y + 1);
                Vector2 c = WindMap[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (_currentSample.x > 0 && degrees >= 22.5 && degrees < 67.5) // Facing North East
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y + 1), out des))//_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2Int tmp = new Vector2Int(_x, _y + 1);
                Vector2 b = WindMap[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y + 1), out des))//_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y + 1);
                Vector2 c = WindMap[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y), out des))//_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y);
                Vector2 e = WindMap[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }

        }
        else if (_currentSample.x > 0 && degrees >= 67.5 && degrees < 112.5) // Facing East
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y + 1), out des))//_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y + 1);
                Vector2 c = WindMap[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y), out des))//_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y);
                Vector2 e = WindMap[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y - 1), out des))//_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y - 1);
                Vector2 h = WindMap[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (_currentSample.x > 0 && degrees >= 112.5 && degrees < 157.5) // Facing South East
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y), out des))//_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y);
                Vector2 e = WindMap[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y - 1), out des))//_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y - 1);
                Vector2 h = WindMap[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y - 1), out des))//_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2Int tmp = new Vector2Int(_x, _y - 1);
                Vector2 g = WindMap[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (degrees >= 157.5) // Facing South
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y - 1), out des))//_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y - 1);
                Vector2 f = WindMap[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y - 1), out des))//_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2Int tmp = new Vector2Int(_x, _y - 1);
                Vector2 g = WindMap[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x + 1, _y - 1), out des))//_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2Int tmp = new Vector2Int(_x + 1, _y - 1);
                Vector2 h = WindMap[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 112.5 && degrees < 157.5) // Facing South West
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y - 1), out des))//_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2Int tmp = new Vector2Int(_x, _y - 1);
                Vector2 g = WindMap[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y - 1), out des))//_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y - 1);
                Vector2 f = WindMap[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y), out des))//_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y);
                Vector2 d = WindMap[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 67.5 && degrees < 112.5) // Facing West
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y - 1), out des))//_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y - 1);
                Vector2 f = WindMap[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y), out des))//_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y);
                Vector2 d = WindMap[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y + 1), out des))//_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y + 1);
                Vector2 a = WindMap[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 22.5 && degrees < 67.5) // Facing North West
        {
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y), out des))//_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y);
                Vector2 d = WindMap[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x - 1, _y + 1), out des))//_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2Int tmp = new Vector2Int(_x - 1, _y + 1);
                Vector2 a = WindMap[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
            if (!windDictionary.TryGetValue(new Vector2Int(_x, _y + 1), out des))//_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2Int tmp = new Vector2Int(_x, _y + 1);
                Vector2 b = WindMap[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                WindMap[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
                tmpWindStorage.Add(tmp);
                windDictionary.Add(tmp, true);
            }
        }

    }
    int GetCoordinate(int _x, int _y)
    {
        return _x + (_y * (WindMapResolution * 3));
    }
    Vector2 AddDeviation(Vector2 _inp)
    {
        Vector2 rtrn = new Vector2();
        float newMag = _inp.magnitude;
        int attribute = Random.Range(0, 4);
        rtrn = new Vector2(_inp.x + Random.Range(-MaxDeviation, MaxDeviation), _inp.y + Random.Range(-MaxDeviation, MaxDeviation));
        rtrn = rtrn.normalized * newMag;
        return rtrn;

    }
    private bool IsWithinMap(int _x, int _y)
    {

        if (_x < WindMapResolution && _y < WindMapResolution) { return false; }
        if (_x > WindMapResolution * 2 && _y < WindMapResolution) { return false; }
        if (_x < WindMapResolution && _y > WindMapResolution * 2) { return false; }
        if (_x > WindMapResolution * 2 && _y > WindMapResolution * 2) { return false; }

        return true;

    }
    private bool IsNotOnEdge(int _x, int _y)
    {
        if (_y == 0) { return false; }
        if (_x == WindMapResolution && _y < WindMapResolution) { return false; }
        if (_x == (WindMapResolution * 2) && _y < WindMapResolution) { return false; }
        if ((_x < WindMapResolution || _x > 2 * WindMapResolution) && _y == WindMapResolution) { return false; }
        if (_x == 0 || _x == 3 * WindMapResolution - 1) { return false; }
        if (_y == WindMapResolution * 2 && (_x < WindMapResolution || _x > 2 * WindMapResolution)) { return false; }
        if (_x == WindMapResolution && _y > 2 * WindMapResolution) { return false; }
        if (_x == 2 * WindMapResolution && _y > 2 * WindMapResolution) { return false; }
        if (_y == 4 * WindMapResolution - 1) { return false; }
        return true;

    }



}

[CreateAssetMenu(menuName = "HurstPlanetGenerator/Biome")]
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
    public float BiomeRelativeAltitude= 1.0f; 

    public SerializedBiome ReturnSerializedBiome()
    {
        SerializedBiome rtrn;
        vec3 tmp; tmp.x = BiomeNoiseOffset.x; tmp.y = BiomeNoiseOffset.y; tmp.z = BiomeNoiseOffset.z;
        rtrn.BNO = tmp;
        rtrn.BNS = BiomeNoiseScale;
        rtrn.MaxHS = MaximumHeatSelection;
        rtrn.MaxMS = MaximumMoistureSelection;
        rtrn.MinHS = MinimumHeatSelection;
        rtrn.MinMS = MinimumMoistureSelection;
        rtrn.BRA = BiomeRelativeAltitude;
        return rtrn;
    }


    public struct vec3 { public float x; public float y; public float z; }

    public struct SerializedBiome
    {
        public float MinHS;
        public float MaxHS;
        public float MinMS;
        public float MaxMS;
        public vec3 BNO;
        public float BNS;
        public float BRA;
    }

}





