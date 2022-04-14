using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlaneHeightMapGeneration : MonoBehaviour
{
    public MeshRenderer planeRenderer;
    private MeshFilter filter;
    private Noise noise;
    private FractalNoise fnoise;
    private PerlinFractalNoise pfNoise;

    public ComputeShader shader;

    [Header("Levels of Detail"), Range(0, 100)]
    public int LevelOfDetail = 1;
    [Range(1, 10)]
    public int LevelsOfDetail = 1;

    [Header("Noise Settings")]
    public float offset = 0.0f;
    public float scale = 1000.0f;
    public int octaves = 5;
    // Start is called before the first frame update
    void Start()
    {
        fnoise = new FractalNoise();
        pfNoise = new PerlinFractalNoise();
        noise = new Noise();// Random.Range(int.MinValue, int.MaxValue) - Time.frameCount);
        filter = transform.GetComponent<MeshFilter>();

        GenerateLODMesh();

        int lodMappedVal = (int)map(LevelOfDetail, 0, 100, 10, 1024);
        Texture2D heightmapTexture = GenerateTexture(lodMappedVal);
        heightmapTexture.filterMode = FilterMode.Point;
        planeRenderer.material.mainTexture = heightmapTexture;
    }

    // Update is called once per frame
    void Update()
    {
        //GenerateLODMesh();
        GenerateLODMeshGPU();
    }

    private void OnValidate()
    {
        int lodMappedVal = (int)map(LevelOfDetail, 0, 100, 10, 1024);
        Texture2D heightmapTexture = GenerateTexture(lodMappedVal);
        heightmapTexture.filterMode = FilterMode.Point;
        planeRenderer.material.mainTexture = heightmapTexture;

    }

    Texture2D GenerateTexture(int size)
    {
        fnoise.Octaves = octaves;

        Texture2D texture = new Texture2D(size, size);
        float pixColour = 0.0f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float noiseVal = Mathf.PerlinNoise(i / scale,j / scale);//noise.Evaluate(new Vector3(i / scale, j / scale, offset));
                pixColour = map(noiseVal, -1.0f, 1.0f, 0.0f, 1.0f);
                Color c = new Color(pixColour, pixColour, pixColour);

                texture.SetPixel(i, j, c);
            }
        }
        texture.Apply();
        return texture;
    }


    public void GenerateLODMesh()
    {
        float BaseResolution = 10;
        float radius = 10.0f;
        Vector3 _normal = new Vector3(0, 1, 0);
        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> _uvs = new List<Vector2>();
        Vector3 AxisA = new Vector3(_normal.y, _normal.z, _normal.x);
        Vector3 AxisB = Vector3.Cross(_normal, AxisA);



        for (int i = 0; i < LevelsOfDetail; i++)
        {
            float increment = 1.0f / (BaseResolution);// + (BaseResolution * (2 * i)));
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {

                    int itr = x + (y * (int)BaseResolution);
                    Vector2 percent = new Vector2(y, x) / (BaseResolution - 1);
                    Vector3 pointOnUnitCube = _normal + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
                    _uvs.Add(new Vector2(map(x, 0, BaseResolution, 0, 1), map(y, 0, BaseResolution, 0,1)));
                    if (x < BaseResolution - 1 && y < BaseResolution - 1)
                    {
                        triangles.Add(itr);
                        triangles.Add(itr + (int)BaseResolution);
                        triangles.Add(itr + (int)BaseResolution + 1);


                        triangles.Add(itr);
                        triangles.Add(itr + (int)BaseResolution + 1);
                        triangles.Add(itr + 1);

                    }
                }
            }

            Mesh m = new Mesh();
            m.SetVertices(verticies);
            m.SetTriangles(triangles, 0);
            m.SetUVs(0, _uvs);
            m.RecalculateNormals();
            m.RecalculateBounds();


            filter.mesh = m;
        }
    }

    public void GenerateLODMeshGPU()
    {
        var vertexBufferData = new ComputeBuffer(100, 3 * sizeof(float));
        shader.SetBuffer(0, "Verticies", vertexBufferData);

        var uvBufferData = new ComputeBuffer(100, 2 * sizeof(float));
        shader.SetBuffer(0, "UVs", uvBufferData);

        var triangleBufferData = new ComputeBuffer(486, sizeof(int));
        shader.SetBuffer(0, "Triangles", triangleBufferData);
        
        Vector3 tmp = new Vector3();
        tmp = -(transform.position - Camera.main.transform.position).normalized;
        
        shader.SetVector("NormalGP", tmp); //(transform.position - Camera.main.transform.position).normalized);
        shader.SetFloat("NFFP", 0.5f);
        shader.SetFloat("Radius", 100.0f);
        shader.SetInt("Resolution", 10);

        int ki = shader.FindKernel("CSMain");
        shader.Dispatch(ki, 8, 8, 1);

        Vector3[] verticies = new Vector3[100];
        vertexBufferData.GetData(verticies);

        Vector2[] uvs = new Vector2[100];
        uvBufferData.GetData(uvs);

        int[] triangles = new int[486];
        triangleBufferData.GetData(triangles);

        Mesh m = new Mesh();
        m.SetVertices(verticies);
        m.SetTriangles(triangles, 0);
        m.SetUVs(0, uvs);
        m.RecalculateNormals();
        m.RecalculateBounds();
        filter.mesh = m;

        vertexBufferData.Release();
        vertexBufferData.Dispose();
        uvBufferData.Release();
        uvBufferData.Dispose();
        triangleBufferData.Release();
        triangleBufferData.Dispose();

    }
    
    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }

}
