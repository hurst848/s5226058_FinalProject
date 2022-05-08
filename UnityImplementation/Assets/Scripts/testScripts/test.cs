using System.Linq;
using System.Collections;
using System.Collections.Generic;
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

    public ComputeShader windSolverShader;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] texturepoints = new Vector3[(WindMapResolution * 3) * (WindMapResolution * 4)];

        var texturePointDataBuffer = new ComputeBuffer(texturepoints.Length, 3 * sizeof(float));
        texturePointDataBuffer.SetData(texturepoints);
        windSolverShader.SetBuffer(0, "", texturePointDataBuffer);

        int ki = windSolverShader.FindKernel("CSMain");
        windSolverShader.Dispatch(ki, 8, 8, 1);
    }
}
