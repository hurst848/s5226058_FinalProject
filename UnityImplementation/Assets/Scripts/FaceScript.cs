using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceScript : MonoBehaviour
{
    public float BaseResolution = 100;
    private float radius;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

        MeshFilter Filter = GetComponent<MeshFilter>();
        Filter.mesh.Clear();
        Filter.mesh = m;
    }
}
