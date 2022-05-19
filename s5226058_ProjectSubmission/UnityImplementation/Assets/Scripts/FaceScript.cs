using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceScript : MonoBehaviour
{
    public float BaseResolution = 100;
    private float radius;
    public float NoiseScale = 1000;
    private float MaximumTerrainHeight;

    Noise noise = new Noise();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTerrainFactors(float _maximumTerrainHeight)
    {
        MaximumTerrainHeight = _maximumTerrainHeight;
    }

    public void GenerateFace(Vector3 _normal, float _radius)
    {  
        radius = _radius;

        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles= new List<int>();

        Vector3 AxisA = new Vector3(_normal.y, _normal.z, _normal.x);
        Vector3 AxisB = Vector3.Cross(_normal, AxisA);

        float increment = 1.0f / BaseResolution;

        for (int y= 0; y < BaseResolution; y++)
        {
            for (int x = 0; x < BaseResolution; x++)
            {
             
                int itr = x + (y * (int)BaseResolution);
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

        Mesh m = new Mesh();
        m.SetVertices(verticies);
        m.SetTriangles(triangles, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();


        for (int x = 0; x < BaseResolution; x++)
        {
            for (int y = 0; y < BaseResolution; y++)
            {
                int index = (x * (int)BaseResolution) + y;
                verticies[index] += m.normals[index] * returnY(x, y);
            }
        }


        m.SetVertices(verticies);
        m.RecalculateNormals();
        m.RecalculateBounds();

        MeshFilter Filter = GetComponent<MeshFilter>();
        Filter.mesh.Clear();
        Filter.mesh = m;
    }


    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }


    float returnY(int _x, int _y)
    {
        float rtrn = 0.0f;

        float xcoord = (float)_x / (4096) * NoiseScale;
        float ycoord = (float)_y / (4096) * NoiseScale;

        rtrn = Mathf.PerlinNoise(xcoord, ycoord);

        rtrn = noise.Evaluate(new Vector3(xcoord, ycoord, 0));

        rtrn = map(rtrn, -1.0f, 1.0f, 0, MaximumTerrainHeight);

        return rtrn;
    }
}
