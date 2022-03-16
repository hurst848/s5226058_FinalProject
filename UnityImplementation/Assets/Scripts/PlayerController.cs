using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float mouseSensitivity = 1.0f;

    public float StandardSpeed = 10.0f;
    public float SprintSpeed = 20.0f;

    public CameraScript cs;

    void Start()
    {
        //cs = GetComponent<CameraScript>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        

        Vector3 rotation = new Vector3();


        rotation.y = Input.GetAxis("Mouse X") * mouseSensitivity;
        rotation.x = Input.GetAxis("Mouse Y") * -mouseSensitivity;

        transform.eulerAngles += rotation;

        Vector3 position = new Vector3();
        position += transform.forward;
        if (Input.GetKey(KeyCode.W)) { }
        if (Input.GetKey(KeyCode.S)) { position -= transform.forward; }
        if (Input.GetKey(KeyCode.D)) { position += transform.right; }
        if (Input.GetKey(KeyCode.A)) { position -= transform.right; }
        if (Input.GetKey(KeyCode.Space)) { position += transform.up; }
        if (Input.GetKey(KeyCode.LeftControl)) { position -= transform.up; }
        if (Input.GetKey(KeyCode.LeftShift)) { position *= SprintSpeed * Time.deltaTime; }
        else { position *= StandardSpeed * Time.deltaTime; }

        cs.Move(position);
        transform.position += position;
    }

}
