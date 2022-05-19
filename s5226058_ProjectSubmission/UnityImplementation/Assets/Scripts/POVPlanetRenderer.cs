using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;



public class POVPlanetRenderer : MonoBehaviour
{
    [SerializeField]
    public DVec3 GalacticPosition;

    public int radius;
    public int BaseResolution = 100;
    public float noiseScale = 1000;
    public float MaximumTerrainHeight = 100;
    public Gradient testTererainGradient;


    private CameraScript cam;
    private MeshFilter planetMeshFilter;



    private List<Vector3> verticies;
    private List<int> triangles;
    private Color[] colours;
    private Noise noise;
    private bool Generated = false;
    bool running;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindObjectOfType<CameraScript>();
        if (cam == null)
        {
            throw new System.Exception();
        }
        planetMeshFilter = GetComponent<MeshFilter>();

        noise = new Noise();

        StartCoroutine(DirtyGenerateMesh());
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate whether the mesh needs updating
        //double distanceToPlanet = DVec3.Distance(GalacticPosition, camera.GalacticPosition);
        
        if (Generated)
        {
            //StartCoroutine(GenerateMesh());
        }
        
    }

    IEnumerator MTGenerateMesh()
    {
        List<Vector3> facesToBeRendered = new List<Vector3>();
        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.up); }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.up); }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.right); }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.right); }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(transform.forward); }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f) { facesToBeRendered.Add(-transform.forward); }

        var dataarry = new NativeArray<POVPlanetRenderer.MeshData>(facesToBeRendered.Count, Allocator.TempJob);
        for (int i = 0; i < dataarry.Length; i++)
        {
            dataarry[i] = new MeshData(facesToBeRendered[i], BaseResolution, radius);
        }
        
        vertexGenerationJob job = new vertexGenerationJob()
        {
            Faces = dataarry
        };
        JobHandle handle = job.Schedule(facesToBeRendered.Count, 1);
        handle.Complete();
        
        dataarry.Dispose();
       



        yield return null;
    }


    IEnumerator GenerateMesh()
    {
        Generated = false;
        verticies = new List<Vector3>();
        triangles = new List<int>();

        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("UP");
            StartCoroutine(GenerateFace(transform.up));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("UP DONE");
        }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("DOWN");
            StartCoroutine(GenerateFace(-transform.up));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("DOWN DONE");
        }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("RIGHT");
            StartCoroutine(GenerateFace(transform.right));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("RIGHT DONE");
        }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("LEFT");
            StartCoroutine(GenerateFace(-transform.right));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("LEFT DONE");
        }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("FORWARD");
            StartCoroutine(GenerateFace(transform.forward));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("FORWARD DONE");
        }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            running = true;
            Debug.Log("BACK");
            StartCoroutine(GenerateFace(-transform.forward));
            StartCoroutine(WaitUnitlDone());
            Debug.Log("BACK DONE");
        }

        Debug.Log(verticies.Count);
        colours = new Color[verticies.Count];
        
        Mesh m = new Mesh();

        m.SetVertices(verticies);
        m.SetTriangles(triangles,0);
        m.RecalculateNormals();
        m.RecalculateBounds();


        for (int i = 0; i < verticies.Count; i++)
        {
            Vector3 tmp = verticies[i];
            float newHeight = returnY(tmp);

            verticies[i] += m.normals[i] * newHeight;
            colours[i] = testTererainGradient.Evaluate(map(newHeight, 0, MaximumTerrainHeight, 0, 1));
            
        }


        m.SetVertices(verticies);
        m.SetColors(colours);
        m.RecalculateNormals();
        m.OptimizeReorderVertexBuffer();
        m.RecalculateBounds();


        planetMeshFilter.mesh.Clear();
        planetMeshFilter.mesh = m;

        Generated = true;
        yield return null;

    }

    IEnumerator DirtyGenerateMesh()
    {
        Generated = false;
        verticies = new List<Vector3>();
        triangles = new List<int>();

        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("UP");
            Vector3 AxisA = new Vector3(transform.up.x,transform.up.z, transform.up.x);
            Vector3 AxisB = Vector3.Cross(transform.up, AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = transform.up + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("UP DONE");
        }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("DOWN");
            Vector3 AxisA = new Vector3(-(transform.up).x, -(transform.up).z, -(transform.up).x);
            Vector3 AxisB = Vector3.Cross(-(transform.up), AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = -(transform.up) + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("DOWN DONE");
        }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("RIGHT");
            Vector3 AxisA = new Vector3(transform.right.x, transform.right.z, transform.right.x);
            Vector3 AxisB = Vector3.Cross(transform.right, AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = transform.right + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("RIGHT DONE");
        }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("LEFT");
            Vector3 AxisA = new Vector3(-(transform.right).x, -(transform.right).z, -(transform.right).x);
            Vector3 AxisB = Vector3.Cross(-(transform.right), AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = -(transform.right) + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("LEFT DONE");
        }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("FORWARD");
            Vector3 AxisA = new Vector3(transform.forward.x, transform.forward.z, transform.forward.x);
            Vector3 AxisB = Vector3.Cross(transform.forward, AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = transform.forward + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("FORWARD DONE");
        }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("BACKWARD");
            Vector3 AxisA = new Vector3(-(transform.forward).x, -(transform.forward).z, -(transform.forward).x);
            Vector3 AxisB = Vector3.Cross(-(transform.forward), AxisA);
            int strt = verticies.Count;
            for (int y = 0; y < BaseResolution; y++)
            {
                for (int x = 0; x < BaseResolution; x++)
                {
                    int itr = (x + (y * BaseResolution)) + strt;
                    Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                    Vector3 pointOnUnitCube = -(transform.forward) + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                    verticies.Add(pointOnUnitSphere);
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
                yield return new WaitForEndOfFrame();

            }
            Debug.Log("FACE_COMPLETED");
            Debug.Log("FORWARD DONE");
        }

        Debug.Log(verticies.Count);
        colours = new Color[verticies.Count];

        Mesh m = new Mesh();

        m.SetVertices(verticies);
        m.SetTriangles(triangles, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();


        for (int i = 0; i < verticies.Count; i++)
        {
            Vector3 tmp = verticies[i];
            float newHeight = returnY(tmp);

            verticies[i] += m.normals[i] * newHeight;
            colours[i] = testTererainGradient.Evaluate(map(newHeight, 0, MaximumTerrainHeight, 0, 1));
            if (i%100 == 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }


        m.SetVertices(verticies);
        m.SetColors(colours);
        m.RecalculateNormals();
        m.OptimizeReorderVertexBuffer();
        m.RecalculateBounds();


        planetMeshFilter.mesh.Clear();
        planetMeshFilter.mesh = m;

        Generated = true;
        yield return null;

    }




    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }

    IEnumerator GenerateFace(Vector3 _normal)
    {
        Vector3 AxisA = new Vector3(_normal.y, _normal.z, _normal.x);
        Vector3 AxisB = Vector3.Cross(_normal, AxisA);
        int strt = verticies.Count;
        for (int y = 0; y < BaseResolution; y++)
        {
            for (int x = 0; x < BaseResolution; x++)
            {
                int itr = (x + (y * BaseResolution)) + strt;
                Vector2 percent = new Vector2(y, x) / (float)(BaseResolution - 1);
                Vector3 pointOnUnitCube = _normal + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                verticies.Add(pointOnUnitSphere);
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
            //yield return new WaitForEndOfFrame();

        }
        Debug.Log("FACE_COMPLETED");
        running = false;
        yield return null;
    }

    float returnY(Vector3 vertex)
    {
        float rtrn = 0.0f;

        float xcoord = vertex.x / (4096) * noiseScale;
        float ycoord = vertex.y / (4096) * noiseScale;
        float zcoord = vertex.z / (4096) * noiseScale;

        rtrn = noise.Evaluate(new Vector3(xcoord, ycoord, zcoord));

        rtrn = map(rtrn, -1.0f, 1.0f, 0.0f, MaximumTerrainHeight);
        Debug.Log(rtrn);
        return rtrn;
    }

    IEnumerator WaitUnitlDone()
    {
        while(running)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return null; 
    }


    public struct MeshData
    {
        public float3 norm;

        public NativeArray<float3> verts;
        public NativeArray<int> tris;
        public NativeArray<Color> clrs;
        public int res;
        public float rad;
        public MeshData(Vector3 _normal, int _res, float _rad)
        {
            rad = _rad;
            res = _res;
            norm = _normal;
            verts = new NativeArray<float3>(res * res, Allocator.TempJob);
            tris = new NativeArray<int>((res - 1) * (res - 1) * 6, Allocator.TempJob);
            clrs = new NativeArray<Color>(verts.Length, Allocator.TempJob);
        }

        public void Calculate()
        {
            float3 AxisA = new Vector3(norm.y, norm.z, norm.x);
            float3 AxisB = Vector3.Cross(norm, AxisA);
            int strt = verts.Length;
            int trinum = 0;
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    int itr = (x + (y * res)) + strt;
                    float2 percent = new Vector2(y, x) / (float)(res - 1);
                    float3 pointOnUnitCube = norm + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    float3 pointOnUnitSphere = (pointOnUnitCube / Mathf.Sqrt(pointOnUnitCube.x * pointOnUnitCube.x + pointOnUnitCube.y * pointOnUnitCube.y + pointOnUnitCube.z * pointOnUnitCube.z )) * rad;
                    verts[(x * res) + (y * res)] = (pointOnUnitSphere);
                    if (x < res - 1 && y < res - 1)
                    {
                        tris[trinum] = itr;
                        tris[trinum + 1] = (itr + (int)res);
                        tris[trinum + 2] = (itr + (int)res + 1);

                        tris[trinum + 3] = (itr);
                        tris[trinum + 4] = (itr + (int)res + 1);
                        tris[trinum + 5] = (itr + 1);

                        trinum += 6;

                        // add the counter
                    }

                }
            }

            verts.Dispose();
            tris.Dispose();
            clrs.Dispose();
        }
    }


}

public struct vertexGenerationJob : IJobParallelFor
{
    public NativeArray<POVPlanetRenderer.MeshData> Faces;

    public void Execute(int _index)
    {
        POVPlanetRenderer.MeshData data = Faces[_index];
        data.Calculate();
        Faces[_index] = data;
    }

}
