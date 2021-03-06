using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] Image flyVcamButton;
    [SerializeField] Image followVcamButton;
    [SerializeField] TextMeshProUGUI cameraID_text;
    [SerializeField] int cameraID;

    [Header("Changeables")]
    [SerializeField] CinemachineVirtualCamera flyVcam;
    [SerializeField] CinemachineFreeLook followVcam;
    [SerializeField] List<GameObject> currentCameraList;
    [SerializeField] private GameObject currentFixedCam;

    private bool flyCamActive;
    private bool followCamActive;

    // Start is called before the first frame update
    void Awake()
    {
        init();
    }

    void Start()
    {
        flyVcam.gameObject.SetActive(false);

        flyCamActive = false;
        followCamActive = false;
        toggleCameras();

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void init()
    {
        currentFixedCam = currentCameraList[0];
    }
   
    #region FixedCameraControls
    public void IncrementCamera()
    {
        if (cameraID == currentCameraList.Count - 1)
        {
            cameraID = 0;
        }
        else
        {
            cameraID++;
        }

        currentFixedCam = currentCameraList[cameraID];
        currentFixedCam.SetActive(true);

        toggleCameras();
    }

    public void DecrementCamera()
    {
        if (cameraID == 0)
        {
            cameraID = currentCameraList.Count - 1;
        }
        else
        {
            cameraID--;
        }

        currentFixedCam = currentCameraList[cameraID];
        currentFixedCam.SetActive(true);

        toggleCameras();
    }

    private void toggleCameras()
    {
        cameraID_text.text = cameraID.ToString();

        if (followCamActive)
        {
            toggleFollowCam();
        }

        if (flyCamActive)
        {
            toggleFlyCam();
        }

        foreach (GameObject cam in currentCameraList)
        {
            if (cam != currentFixedCam)
            {
                cam.SetActive(false);
            }
        }
    }
    #endregion

    public void toggleFollowCam()
    {
        cameraID = 0;
        if (!followCamActive)
        {
            flyVcamButton.color = Color.white;
            flyVcam.gameObject.SetActive(false);
            flyCamActive = false;

            followVcam.m_Priority = 20;
            followVcamButton.color = Color.green;
            followCamActive = true;
        }
        else if (followCamActive)
        {
            followVcam.m_Priority = 8;
            followVcamButton.color = Color.white;
            followCamActive = false;
        }

        cameraID_text.text = cameraID.ToString();
    }

    public void toggleFlyCam()
    {
        cameraID = 0;
        if (!flyCamActive)
        {
            followVcamButton.color = Color.white;
            followCamActive = false;

            flyVcam.gameObject.SetActive(true);
            flyVcam.m_Priority = 20;
            flyVcamButton.color = Color.green;
            flyCamActive = true;
        }
        else if (flyCamActive)
        {
            flyVcam.m_Priority = 8;
            flyVcamButton.color = Color.white;
            flyVcam.gameObject.SetActive(false);
            flyCamActive = false;
        }

        cameraID_text.text = cameraID.ToString();
    }
}
