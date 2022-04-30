using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lodtest : MonoBehaviour
{
    [Range(0,255)]
    public int LOD;

    public int baseResolution;
    public int size;

    private MeshFilter filter;

    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3 _normal = new Vector3(0, 1, 0);
        Vector3 AxisA = new Vector3(_normal.y, _normal.z, _normal.x);
        Vector3 AxisB = Vector3.Cross(_normal, AxisA);

        

        for (int i = 0; i < LOD; i++)
        {
            if (i + 1 != LOD)
            {
                float increment = 1.0f / (baseResolution + i);
                for (int x = 0; x < baseResolution; x++)
                {
                    int itr = x * baseResolution;
                    Vector2 percent = new Vector2(0, x) / (baseResolution - 1);
                    Vector3 pointOnUnitCube = _normal + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube * size;
                    verticies.Add(pointOnUnitSphere);
                    uvs.Add(new Vector2(x / baseResolution, 0 / baseResolution));
                    if (x < baseResolution - 1)
                    {
                        triangles.Add(itr);
                        triangles.Add(itr + (int)baseResolution);
                        triangles.Add(itr + (int)baseResolution + 1);

                        triangles.Add(itr);
                        triangles.Add(itr + (int)baseResolution + 1);
                        triangles.Add(itr + 1);


                    }
                }
            }
            else
            {

            }
        }



        for (int y = 0; y < baseResolution; y++)
        {
            for (int x = 0; x < baseResolution; x++)
            {

                int itr = x + (y * (int)baseResolution);
                Vector2 percent = new Vector2(y, x) / (baseResolution - 1);
                Vector3 pointOnUnitCube = _normal + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube * size;
                verticies.Add(pointOnUnitSphere);
                uvs.Add(new Vector2(x/baseResolution, y/baseResolution));
                if (x < baseResolution - 1 && y < baseResolution - 1)
                {
                    triangles.Add(itr);
                    triangles.Add(itr + (int)baseResolution);
                    triangles.Add(itr + (int)baseResolution + 1);


                    triangles.Add(itr);
                    triangles.Add(itr + (int)baseResolution + 1);
                    triangles.Add(itr + 1);

                }
            }
        }
        Mesh m = new Mesh();
        
        m.SetVertices(verticies);
        m.SetTriangles(triangles, 0);
        m.OptimizeReorderVertexBuffer();
        m.RecalculateNormals();
        m.RecalculateBounds();
        filter.mesh = m;


    }

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    private float map(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }
}
