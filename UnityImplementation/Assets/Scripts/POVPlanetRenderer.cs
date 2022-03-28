using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate whether the mesh needs updating
        //double distanceToPlanet = DVec3.Distance(GalacticPosition, camera.GalacticPosition);
        //GenerateMesh();
    }


    private void GenerateMesh()
    {
        verticies = new List<Vector3>();
        triangles = new List<int>();

        if (Vector3.Dot(transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("UP");
            GenerateFace(transform.up);
        }
        if (Vector3.Dot(-transform.up, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("DOWN");
            GenerateFace(-transform.up);
        }
        if (Vector3.Dot(transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("RIGHT");
            GenerateFace(transform.right);
        }
        if (Vector3.Dot(-transform.right, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("LEFT");
            GenerateFace(-transform.right);
        }
        if (Vector3.Dot(transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("FORWARD");
            GenerateFace(transform.forward);
        }
        if (Vector3.Dot(-transform.forward, (transform.position - cam.transform.position).normalized) <= 0.25f)
        {
            Debug.Log("BACK");
            GenerateFace(-transform.forward);
        }

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
            //Debug.Log(newHeight);
            verticies[i] += m.normals[i] * newHeight;
            colours[i] = testTererainGradient.Evaluate(map(newHeight,0,MaximumTerrainHeight,0,1));
        }

        m.SetVertices(verticies);
        m.SetColors(colours);
        m.RecalculateNormals();
        m.RecalculateBounds();


        planetMeshFilter.mesh.Clear();
        planetMeshFilter.mesh = m;

        

    }
    
    
    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }

    void GenerateFace(Vector3 _normal)
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
        }

        

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


}
