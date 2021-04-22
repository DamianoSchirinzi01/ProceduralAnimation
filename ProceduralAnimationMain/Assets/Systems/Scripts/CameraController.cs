using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private currentBugState currentState;
    [SerializeField] CinemachineVirtualCamera playerCam;
    [SerializeField] CinemachineVirtualCamera botCam;

    [SerializeField] Transform target;
    [SerializeField] float yOffset;

    float rotX;
    float rotY;
    float RotationSpeed = 1f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Update()
    {
        followTarget();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        cameraRotation();
    }

    public void switchModes(bool isOnAutoPilot)
    {
        if (isOnAutoPilot)
        {
            currentState = currentBugState.AI_Controlled;
            playerCam.m_Priority = 0;
            botCam.m_Priority = 20;
        }
        else
        {
            currentState = currentBugState.PlayerControlled;
            botCam.m_Priority = 0;
            playerCam.m_Priority = 20;
        }
    }

    private void followTarget()
    {
        transform.position = new Vector3(target.position.x, target.position.y + yOffset, target.position.z);
    }

    private void cameraRotation()
    {
        //now for the mouse rotation
        rotX += Input.GetAxis("Mouse X") * RotationSpeed;
        rotY += Input.GetAxis("Mouse Y") * RotationSpeed;

        rotY = Mathf.Clamp(rotY, -8f, 20f);

        //Camera rotation only allowed if game us not paused
        transform.rotation = Quaternion.Euler(rotY, rotX, 0f);
    }

    private enum currentBugState
    {
        PlayerControlled,
        AI_Controlled
    }
}
