using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public float LocalSpaceBoundingSphere = 1000;
   
    public DVec3 GalacticPosition;

    Hurst_PlanetGenerator[] planets;

    // Start is called before the first frame update
    void Start()
    {
        GalacticPosition = new DVec3(transform.position);
        planets = GameObject.FindObjectsOfType<Hurst_PlanetGenerator>();
        Debug.Log(planets.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.magnitude > LocalSpaceBoundingSphere)
        {
            Vector3 _initial = transform.position;
            transform.position = -transform.position;
            Vector3 _new = transform.position;

            Vector3 _moveDirection = _new - _initial;

            if (planets.Length > 0)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planets[i].MovePlanet(_moveDirection);
                } 
            }
        }
    }




    public void Move(Vector3 _move)
    {
        GalacticPosition += _move;
    }
}

public struct CameraViewFructrum
{
    public CameraViewFructrum(Camera c)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
        Left = planes[0].normal;    Right = planes[1].normal;
        Up = planes[3].normal;      Down = planes[2].normal;
    }

    public Vector3 Left, Right, Up, Down;

}
