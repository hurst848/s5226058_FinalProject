using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public float LocalSpaceBoundingSphere = 1000;

    public DVec3 GalacticPosition;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.magnitude > LocalSpaceBoundingSphere)
        {
            transform.position = -transform.position;
        }
    }




    public void Move(Vector3 _move)
    {
        GalacticPosition += _move;
    }
}
