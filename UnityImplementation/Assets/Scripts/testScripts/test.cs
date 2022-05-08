using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class test : MonoBehaviour
{
    public int WindMapResolution;
    [Range(0, 100)]
    public int NumberOfWindNodes;
    [Range(0.0f, 100.0f)]
    public float WindNodePowerMax;
    public int NumberOfWindIterations;
    public float MaxDeviation;

    public ComputeShader windSolverShader;
    public Texture2D Tex;


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
        public static Vector3 toVector3(vec3 _vec)
        {
            Vector3 rtrn = new Vector3();
            rtrn.x = _vec.x; rtrn.y = _vec.y; rtrn.z = _vec.z;
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
        public static vec3 addVec3(vec3 _a, vec3 _b)
        {
            vec3 rtrn;
            rtrn.x = _a.x + _b.x;
            rtrn.y = _a.y + _b.y;
            rtrn.z = _a.z + _b.z;
            return rtrn;
        }

        public float x;
        public float y;
        public float z;
    };
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {

            vec3[] texturepoints = new vec3[(WindMapResolution * 3) * (WindMapResolution * 4)];
            for (int i = 0; i < texturepoints.Length; i++)
            {
                texturepoints[i] = new vec3();
            }
            for (int i = 0; i < NumberOfWindNodes; i++)
            {
                Vector3 rand = Random.onUnitSphere;
                rand *= Random.Range(-WindNodePowerMax, WindNodePowerMax);
                Vector2 coord = new Vector2(Random.Range(0, (WindMapResolution * 3)), Random.Range(0, (WindMapResolution * 4)));
                texturepoints[((int)coord.x + ((int)coord.y * (WindMapResolution * 3)))] = vec3.toVec3(rand);
            }




            var texturePointDataBuffer = new ComputeBuffer(texturepoints.Length, 3 * sizeof(float));
            texturePointDataBuffer.SetData(texturepoints);
            windSolverShader.SetBuffer(0, "WindMap", texturePointDataBuffer);

            windSolverShader.SetInt("WindMapResolution", WindMapResolution);
            windSolverShader.SetInt("NumberOfWindIterations", NumberOfWindIterations);
            windSolverShader.SetFloat("DeviationMax", MaxDeviation);


            int ki = windSolverShader.FindKernel("CSMain");
            windSolverShader.Dispatch(ki, 8, 8, 1);

            Tex = new Texture2D((WindMapResolution * 3), (WindMapResolution * 4));
            Tex.filterMode = FilterMode.Point;
            Tex.wrapMode = TextureWrapMode.Clamp;
            vec3[] texture = new vec3[(WindMapResolution * 3) * (WindMapResolution * 4)];
            texturePointDataBuffer.GetData(texture);
            int itr = 0;
            string outputlol = "";
            for (int x = 0; x < (WindMapResolution * 3); x++)
            {
                for (int y = 0; y < (WindMapResolution * 4); y++)
                {
                    outputlol += x.ToString() + " " + vec3.toVector3(texture[itr]).magnitude.ToString() + " " + y + "\n";
                    if (vec3.toVector3(texture[itr]) == Vector3.zero)
                    {
                        Tex.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        Vector3 cmask = (vec3.toVector3(texture[itr]));
                        Tex.SetPixel(x, y, new Color(cmask.x, cmask.y, cmask.z));
                    }
                    itr++;
                }
            }
            Tex.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = Tex;
            File.WriteAllText("test.xyz", outputlol);


        }
        if (Input.GetKey(KeyCode.Return))
        {
            StartCoroutine(CPUVersion());
        }
    }
    IEnumerator CPUVersion()
    {
        vec3[] texturepoints = new vec3[(WindMapResolution * 3) * (WindMapResolution * 4)];
        for (int i = 0; i < texturepoints.Length; i++)
        {
            vec3 tmp = new vec3(); tmp.x = 0; tmp.y = 0; tmp.z = 0; 
            texturepoints[i] = new vec3();
        }
        for (int i = 0; i < NumberOfWindNodes; i++)
        {
            Vector3 rand = Random.onUnitSphere;
            rand *= Random.Range(-WindNodePowerMax, WindNodePowerMax);
            Vector2 coord = new Vector2(Random.Range(0, (WindMapResolution * 3)), Random.Range(0, (WindMapResolution * 4)));
            texturepoints[((int)coord.x + ((int)coord.y * (WindMapResolution * 3)))] = vec3.toVec3(rand);
        }

        Tex = new Texture2D((WindMapResolution * 3), (WindMapResolution * 4));
        int XRes = WindMapResolution * 3;
        int YRes = WindMapResolution * 4;
        for (int i = 0; i < NumberOfWindIterations; i++)
        {
            for (int x = 0; x < XRes; x++)
            {
                for (int y = 0; y < YRes; y++)
                {

                    if (IsWithinMap(x,y) && IsNotOnEdge(x,y))
                    {

                        vec3 CurrentSample = texturepoints[GetCoordinate(x,y)];

                        vec3 a = vec3.addVec3(texturepoints[GetCoordinate(x - 1, y + 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x - 1, y + 1)] = a;

                        vec3 b = vec3.addVec3(texturepoints[GetCoordinate(x    , y + 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x    , y + 1)] = b;

                        vec3 c = vec3.addVec3(texturepoints[GetCoordinate(x + 1, y + 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x + 1, y + 1)] = c;

                        vec3 d = vec3.addVec3(texturepoints[GetCoordinate(x - 1, y    )], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x - 1, y    )] = d;

                        vec3 e = vec3.addVec3(texturepoints[GetCoordinate(x + 1, y    )], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x + 1, y    )] = e;

                        vec3 f = vec3.addVec3(texturepoints[GetCoordinate(x - 1, y - 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x - 1, y - 1)] = f;

                        vec3 g = vec3.addVec3(texturepoints[GetCoordinate(x    , y - 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x    , y - 1)] = g;

                        vec3 h = vec3.addVec3(texturepoints[GetCoordinate(x + 1, y - 1)], AddDeviation(CurrentSample));
                        texturepoints[GetCoordinate(x + 1, y - 1)] = h;
                    }
                    else if (IsWithinMap(x,y) && !IsNotOnEdge(x,y))
                    {

                    }
                    else
                    {
                        vec3 rtrn;
                        rtrn.x = 0;
                        rtrn.y = 1;
                        rtrn.z = 0;

                        Tex.SetPixel(x, y, new Color(0, 0, 0));
                    }
                }
            }
            for (int j = 0; j < texturepoints.Length; j++)
            {
                if (!IsZero(texturepoints[i]))
                {
                    vec3 rtrn;
                    rtrn.x = 1;
                    rtrn.y = 1;
                    rtrn.z = 1;

                    Tex.SetPixel(i % XRes, i / XRes, new Color(1, 1, 1));
                }
            }
            Tex.filterMode = FilterMode.Point;
            Tex.wrapMode = TextureWrapMode.Clamp;
            vec3[] texture = new vec3[(WindMapResolution * 3) * (WindMapResolution * 4)];

            Tex.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = Tex;
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;

    }

    int GetCoordinate(int _x, int _y)
    {
        return _x + (_y * (WindMapResolution * 3));
    }

    vec3 AddDeviation(vec3 _inp)
    {
        vec3 rtrn;
        int attribute = Random.Range(0,4);
        if (attribute < 1)
        {
            rtrn = vec3.toVec3(new Vector3(_inp.x + Random.Range(-MaxDeviation, MaxDeviation), _inp.y, _inp.z));
        }
        else if (attribute < 2)
        {
            rtrn = vec3.toVec3(new Vector3(_inp.x, _inp.y + Random.Range(-MaxDeviation, MaxDeviation), _inp.z));
        }
        else
        {
            rtrn = vec3.toVec3(new Vector3(_inp.x, _inp.y, _inp.z + Random.Range(-MaxDeviation, MaxDeviation)));
        }
        return rtrn;

    }

    bool IsWithinMap(int _x, int _y)
    {

        if (_x < WindMapResolution && _y < WindMapResolution) { return false; }
        if (_x > WindMapResolution * 2 && _y < WindMapResolution) { return false; }
        if (_x < WindMapResolution && _y > WindMapResolution * 2) { return false; }
        if (_x > WindMapResolution * 2 && _y > WindMapResolution * 2) { return false; }

        return true;

    }

    bool IsNotOnEdge(int _x, int _y)
    {
        if (_y == 0) { return false; }
        if (_x == WindMapResolution  && _y < WindMapResolution) { return false; }
        if (_x == (WindMapResolution * 2) && _y < WindMapResolution) { return false; }
        if ((_x < WindMapResolution || _x > 2 * WindMapResolution) && _y == WindMapResolution) { return false; }
        if (_x == 0 || _x == 3 * WindMapResolution - 1) { return false; }
        if (_y == WindMapResolution * 2 && (_x < WindMapResolution || _x > 2 * WindMapResolution)) { return false; }
        if (_x == WindMapResolution && _y > 2 * WindMapResolution) { return false; }
        if (_x == 2 * WindMapResolution && _y > 2 * WindMapResolution) { return false; }
        if (_y == 4 * WindMapResolution - 1) { return false; }
        return true;

    }

    bool IsZero(vec3 _inp)
    {
        if (_inp.x == 0 && _inp.y == 0 && _inp.z == 0) { return true; }
        return false;
    }
    float mapValues(float _value, float _fromA, float _ToA, float _fromB, float _ToB)
    {
        return (_value - _fromA) / (_ToA - _fromA) * (_ToB - _fromB) + _fromB;
    }
}
