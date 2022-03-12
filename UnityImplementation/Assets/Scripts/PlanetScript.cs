using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetScript : MonoBehaviour
{

    // 2439700   Mercury radius

    public float Radius =100;

    public float MaximumTerrainHeight;

    public GameObject FaceTemplate;

    private GameObject[] Faces = new GameObject[6];

    // Start is called before the first frame update
    void Start()
    {
        GeneratePlanet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void GeneratePlanet()
    {
        for (int i = 0; i< Faces.Length; i++)
        {
            Faces[i] = Instantiate(FaceTemplate, transform);
        }

        Faces[0].GetComponent<FaceScript>().GenerateFace(new Vector3(0, 1, 0), Radius);
        Faces[1].GetComponent<FaceScript>().GenerateFace(new Vector3(0, -1, 0), Radius);
        Faces[2].GetComponent<FaceScript>().GenerateFace(new Vector3(1, 0, 0), Radius);
        Faces[3].GetComponent<FaceScript>().GenerateFace(new Vector3(-1, 0, 0), Radius);
        Faces[4].GetComponent<FaceScript>().GenerateFace(new Vector3(0, 0, 1), Radius);
        Faces[5].GetComponent<FaceScript>().GenerateFace(new Vector3(0, 0, -1), Radius);
    }
}
