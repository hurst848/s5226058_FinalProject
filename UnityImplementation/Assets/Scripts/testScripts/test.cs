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
    private bool running = false;
    Vector2[] texturepoints;

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
        if (Input.GetKey(KeyCode.Return) && !running)
        {
            running = true;
            StartCoroutine(CPUVersion());
        }
    }
    IEnumerator CPUVersion()
    {
        Tex = new Texture2D((WindMapResolution * 3), (WindMapResolution * 4));
       
        int XRes = WindMapResolution * 3;
        int YRes = WindMapResolution * 4;

        for (int x = 0; x < XRes; x++) { for (int y = 0; y < YRes; y++) { Tex.SetPixel(x, y, Color.black); } }
        Tex.Apply();

        texturepoints = new Vector2[(WindMapResolution * 3) * (WindMapResolution * 4)];
        for (int i = 0; i < texturepoints.Length; i++)
        { 
            texturepoints[i] = Vector2.zero;
        }
        for (int i = 0; i < NumberOfWindNodes; i++)
        {
            Vector3 rand = Random.onUnitSphere;
            rand *= Random.Range(-WindNodePowerMax, WindNodePowerMax);
            Vector2 coord = new Vector2(Random.Range(0, (WindMapResolution * 3)), Random.Range(0, (WindMapResolution * 4)));
            if (IsWithinMap((int)coord.x, (int)coord.y))
            {
                texturepoints[((int)coord.x + ((int)coord.y * (WindMapResolution * 3)))] = rand;
            }
            else { i--; }
            
        }

        for (int i = 0; i < NumberOfWindIterations; i++)
        {
            Texture2D StoredCalculated = new Texture2D(XRes,YRes);
            for (int x = 0; x < XRes; x++)
            {
                for (int y = 0; y < YRes; y++)
                {
                    if (texturepoints[GetCoordinate(x,y)] != Vector2.zero && StoredCalculated.GetPixel(x , y ) != Color.cyan)
                    {
                        if (IsWithinMap(x, y)  && IsNotOnEdge(x, y))
                        {                       
                            Vector2 CurrentSample = texturepoints[GetCoordinate(x, y)];
                            StoredCalculated.SetPixel(x, y, Color.cyan);
                            StoredCalculated.Apply();
                            if (StoredCalculated.GetPixel(x - 1, y + 1) != Color.cyan)//North West
                            {
                                Vector2 a = texturepoints[GetCoordinate(x - 1, y + 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x - 1, y + 1)] = a;
                                StoredCalculated.SetPixel(x - 1, y + 1, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x, y + 1) != Color.cyan)//North
                            {
                                Vector2 b = texturepoints[GetCoordinate(x, y + 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x, y + 1)] = b;
                                StoredCalculated.SetPixel(x, y + 1, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x + 1, y + 1) != Color.cyan)//North East
                            {
                                Vector2 c = texturepoints[GetCoordinate(x + 1, y + 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x + 1, y + 1)] = c;
                                StoredCalculated.SetPixel(x + 1, y + 1, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x - 1, y) != Color.cyan)//West
                            {
                                Vector2 d = texturepoints[GetCoordinate(x - 1, y)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x - 1, y)] = d;
                                StoredCalculated.SetPixel(x - 1, y, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x + 1, y) != Color.cyan)//East
                            {
                                Vector2 e = texturepoints[GetCoordinate(x + 1, y)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x + 1, y)] = e;
                                StoredCalculated.SetPixel(x + 1, y, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x - 1, y - 1) != Color.cyan)//South West
                            {
                                Vector2 f = texturepoints[GetCoordinate(x - 1, y - 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x - 1, y - 1)] = f;
                                StoredCalculated.SetPixel(x - 1, y - 1, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x, y - 1) != Color.cyan)//South
                            {
                                Vector2 g = texturepoints[GetCoordinate(x, y - 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x, y - 1)] = g;
                                StoredCalculated.SetPixel(x, y - 1, Color.cyan);
                            }
                            if (StoredCalculated.GetPixel(x + 1, y - 1) != Color.cyan)//South East
                            {
                                Vector2 h = texturepoints[GetCoordinate(x + 1, y - 1)] + AddDeviation(CurrentSample);
                                texturepoints[GetCoordinate(x + 1, y - 1)] = h;
                                StoredCalculated.SetPixel(x + 1, y - 1, Color.cyan);
                            }
                        }
                        else if (IsWithinMap(x, y) && !IsNotOnEdge(x, y))
                        {

                        }
                        else
                        {
                            Tex.SetPixel(x, y, new Color(0, 0, 0));
                        }
                        StoredCalculated.Apply();
                    }
                    
                }
            }

            StoredCalculated.filterMode = FilterMode.Point;
            StoredCalculated.wrapMode = TextureWrapMode.Clamp;
            StoredCalculated.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = StoredCalculated;
            yield return new WaitForSeconds(0.25f);


            for (int j = 0; j < texturepoints.Length; j++)
            {
                if (texturepoints[j] != Vector2.zero)
                {
                    vec3 rtrn;
                    rtrn.x = 1;
                    rtrn.y = 1;
                    rtrn.z = 1;

                    Tex.SetPixel(j % XRes, j / XRes, new Color(1, 1, 1));
                }
            }
            Tex.filterMode = FilterMode.Point;
            Tex.wrapMode = TextureWrapMode.Clamp;
            //vec3[] texture = new vec3[(WindMapResolution * 3) * (WindMapResolution * 4)];

            Tex.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = Tex;
            yield return new WaitForSeconds(0.1f);
        }
        running = false;
        yield return null;

    }


    void SpreadWind(Vector2 _currentSample, int _x, int _y, Texture2D _storedCalculated)
    {
        Vector2 normalizedSample = _currentSample.normalized;

        float degrees = Vector2.Angle(UnitCircle[0], _currentSample);
        if (degrees < 22.5f) // Facing North
        {
            if (_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2 a = texturepoints[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2 b = texturepoints[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2 c = texturepoints[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
            }
        }
        else if (_currentSample.x > 0 && degrees >= 22.5 && degrees < 67.5) // Facing North East
        {
            if (_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2 b = texturepoints[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2 c = texturepoints[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2 e = texturepoints[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
            }
            
        }
        else if (_currentSample.x > 0 && degrees >= 67.5 && degrees < 112.5) // Facing East
        {
            if (_storedCalculated.GetPixel(_x + 1, _y + 1) != Color.cyan)//North East
            {
                Vector2 c = texturepoints[GetCoordinate(_x + 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y + 1)] = c;
                _storedCalculated.SetPixel(_x + 1, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2 e = texturepoints[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2 h = texturepoints[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
            }
        }
        else if (_currentSample.x > 0 && degrees >= 112.5 && degrees < 157.5) // Facing South East
        {
            if (_storedCalculated.GetPixel(_x + 1, _y) != Color.cyan)//East
            {
                Vector2 e = texturepoints[GetCoordinate(_x + 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y)] = e;
                _storedCalculated.SetPixel(_x + 1, _y, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2 h = texturepoints[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2 g = texturepoints[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
            }
        }
        else if (degrees >= 157.5) // Facing South
        {
            if (_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2 f = texturepoints[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2 g = texturepoints[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x + 1, _y - 1) != Color.cyan)//South East
            {
                Vector2 h = texturepoints[GetCoordinate(_x + 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x + 1, _y - 1)] = h;
                _storedCalculated.SetPixel(_x + 1, _y - 1, Color.cyan);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 112.5 && degrees < 157.5) // Facing South West
        {
            if (_storedCalculated.GetPixel(_x, _y - 1) != Color.cyan)//South
            {
                Vector2 g = texturepoints[GetCoordinate(_x, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y - 1)] = g;
                _storedCalculated.SetPixel(_x, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2 f = texturepoints[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2 d = texturepoints[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 67.5 && degrees < 112.5) // Facing West
        {
            if (_storedCalculated.GetPixel(_x - 1, _y - 1) != Color.cyan)//South West
            {
                Vector2 f = texturepoints[GetCoordinate(_x - 1, _y - 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y - 1)] = f;
                _storedCalculated.SetPixel(_x - 1, _y - 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2 d = texturepoints[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2 a = texturepoints[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
            }
        }
        else if (_currentSample.x <= 0 && degrees >= 22.5 && degrees < 67.5) // Facing North West
        {
            if (_storedCalculated.GetPixel(_x - 1, _y) != Color.cyan)//West
            {
                Vector2 d = texturepoints[GetCoordinate(_x - 1, _y)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y)] = d;
                _storedCalculated.SetPixel(_x - 1, _y, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x - 1, _y + 1) != Color.cyan)//North West
            {
                Vector2 a = texturepoints[GetCoordinate(_x - 1, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x - 1, _y + 1)] = a;
                _storedCalculated.SetPixel(_x - 1, _y + 1, Color.cyan);
            }
            if (_storedCalculated.GetPixel(_x, _y + 1) != Color.cyan)//North
            {
                Vector2 b = texturepoints[GetCoordinate(_x, _y + 1)] + AddDeviation(_currentSample);
                texturepoints[GetCoordinate(_x, _y + 1)] = b;
                _storedCalculated.SetPixel(_x, _y + 1, Color.cyan);
            }
        }

    }

    int GetCoordinate(int _x, int _y)
    {
        return _x + (_y * (WindMapResolution * 3));
    }

    Vector2 AddDeviation(Vector3 _inp)
    {
        Vector2 rtrn = new Vector3();
        int attribute = Random.Range(0,3);
        if (attribute < 1)
        {
            rtrn = new Vector2(_inp.x + Random.Range(-MaxDeviation, MaxDeviation), _inp.y);
        }
        else if (attribute < 2)
        {
            rtrn = new Vector2(_inp.x, _inp.y + Random.Range(-MaxDeviation, MaxDeviation));
        }
        float newMag = rtrn.magnitude * 0.8f;
        rtrn = rtrn.normalized * newMag;
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
