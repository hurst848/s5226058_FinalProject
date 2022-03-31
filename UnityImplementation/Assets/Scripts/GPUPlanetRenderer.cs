using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
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

    private MeshFilter filter;
    private bool MeshUpdate;
    private Mesh TerrainMesh;
    private int renderNumber;

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
    struct vec3
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
        PlanetDataSize = sizeof(float) + (2 * (sizeof(int)));

        cam = GameObject.FindObjectOfType<CameraScript>();
        if (cam == null)
        {
            throw new System.Exception();
        }

        MeshUpdate = false;
        filter = GetComponent<MeshFilter>();
        StartCoroutine(GenerateVerticies());
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(GenerateVerticies());
        if (MeshUpdate)
        {
            filter.mesh = TerrainMesh;
            MeshUpdate = false;
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

            Debug.Log(pd[0].toString());

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


            MeshUpdate = true;

            planetDataBuffer.Dispose();
            normalDataBuffer.Dispose();
            vertexDataBuffer.Dispose();
            triangleDataBuffer.Dispose();
        }


       


        yield return null;
    }
}
