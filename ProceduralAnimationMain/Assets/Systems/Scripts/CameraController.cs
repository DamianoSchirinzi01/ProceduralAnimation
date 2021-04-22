using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float rotX;
    float rotY;
    float RotationSpeed = 1f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //now for the mouse rotation
        rotX += Input.GetAxis("Mouse X") * RotationSpeed;
        rotY += Input.GetAxis("Mouse Y") * RotationSpeed;

        rotY = Mathf.Clamp(rotY, -8f, 20f);

        //Camera rotation only allowed if game us not paused
        transform.rotation = Quaternion.Euler(rotY, rotX, 0f);

    }
}
