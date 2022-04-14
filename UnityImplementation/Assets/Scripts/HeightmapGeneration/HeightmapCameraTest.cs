using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightmapCameraTest : MonoBehaviour
{
    // Start is called before the first frame update
    public float cameraSpeed = 10.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = transform.forward.normalized;

        if (Input.GetKey(KeyCode.W))
        {
            dir *= cameraSpeed * Time.deltaTime;
            transform.position += dir;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dir *= -cameraSpeed * Time.deltaTime;
            transform.position += dir;
        }
        
    }
}
