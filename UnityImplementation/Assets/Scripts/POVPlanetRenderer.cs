using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class POVPlanetRenderer : MonoBehaviour
{
    [SerializeField]
    public DVec3 GalacticPosition;

    public int radius;
    public int BaseResolution = 100;
    
    private CameraScript camera;
    private MeshFilter planetMeshFilter;


    private List<Vector3> verticies;
    private List<int> triangles;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindObjectOfType<CameraScript>();
        if (camera == null)
        {
            throw new System.Exception();
        }
        planetMeshFilter = GetComponent<MeshFilter>();

        verticies = new List<Vector3>();
        triangles = new List<int>();
        
        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate whether the mesh needs updating
        //double distanceToPlanet = DVec3.Distance(GalacticPosition, camera.GalacticPosition);
        GenerateMesh();
    }


    private void GenerateMesh()
    {
        if (Vector3.Dot(transform.up, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(transform.up);
        }
        if (Vector3.Dot(-transform.up, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(-transform.up);
        }
        if (Vector3.Dot(transform.right, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(transform.right);
        }
        if (Vector3.Dot(-transform.right, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(-transform.right);
        }
        if (Vector3.Dot(transform.forward, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(transform.forward);
        }
        if (Vector3.Dot(-transform.forward, (transform.position - camera.transform.position).normalized) <= 0.25f)
        {
            GenerateFace(-transform.forward);
        }


        Mesh m = new Mesh();

        m.SetVertices(verticies);
        m.SetTriangles(triangles,0);
        m.RecalculateNormals();
        m.RecalculateBounds();

        planetMeshFilter.mesh = m;

        verticies.Clear();
        triangles.Clear();

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
                int itr = (x + (y * (int)BaseResolution)) + strt;
                Vector2 percent = new Vector2(y, x) / (BaseResolution - 1);
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

    

}
