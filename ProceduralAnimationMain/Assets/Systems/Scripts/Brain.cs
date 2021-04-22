using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [SerializeField] CameraController cameraManager;
    [SerializeField] Transform target;
    [SerializeField] Transform IkTargetsParent;
    [SerializeField] List<LegStep> allLegs;

    #region Individual Legs
    [SerializeField] LegStep frontRightLegStep;
    [SerializeField] LegStep frontLeftLegStep;
    [SerializeField] LegStep MidRightLegStep;
    [SerializeField] LegStep MidLeftLegStep;
    [SerializeField] LegStep backRightLegStep;
    [SerializeField] LegStep backLeftLegStep;
    #endregion

    public float heightAdjustmentDamping;
    public float yOffset;
    public float bodyRotationSpeed;

    public Vector3 inputVelocity;
    public Vector3 worldVelocity;
    public Vector3 currentMoveDir;
    public float currentSpeed;
    public float maxMoveSpeed;
    public float accelerationSpeed;

    public bool autoPilot;

    private void Awake()
    {
        forceUpdateAllLegs();
        fillLegList();
        IkTargetsParent.parent = null;
    }

    private void fillLegList()
    {
        allLegs.Add(frontRightLegStep);
        allLegs.Add(frontLeftLegStep);
        allLegs.Add(MidRightLegStep);
        allLegs.Add(MidLeftLegStep);
        allLegs.Add(backRightLegStep);
        allLegs.Add(backLeftLegStep);
    }

    private void Start()
    {
        StartCoroutine(legUpdate());
    }

    private void Update()
    {
        GetInput();
        offsetBody();

        if (autoPilot == false)
        {
            Move(currentMoveDir);
        }
        else
        {
            moveToTargetPoint(target.position);
        }
    }

    private void GetInput()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        currentMoveDir = new Vector3(horizontal, 0, vertical);
        currentMoveDir.Normalize();       

        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxMoveSpeed = 15f;
        }
        else
        {
            maxMoveSpeed = 10f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(transform.up * bodyRotationSpeed * Time.deltaTime);
            forceUpdateAllLegs();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(-transform.up * bodyRotationSpeed * Time.deltaTime);
            forceUpdateAllLegs();

        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (autoPilot)
            {
                autoPilot = false;
                cameraManager.switchModes(autoPilot);
            }
            else
            {
                autoPilot = true;
                cameraManager.switchModes(autoPilot);
            }
        }
    }

    private void Move(Vector3 direction)
    {
        if(direction != Vector3.zero)
        {
            if(currentSpeed < maxMoveSpeed)
            {
                currentSpeed += accelerationSpeed * Time.deltaTime;
            }
        }
        else
        {
            if(currentSpeed > 0)
            {
                currentSpeed -= accelerationSpeed * Time.deltaTime;
            }
        }

        Vector3 localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(currentMoveDir.x, 0f, currentMoveDir.z)), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * currentSpeed);
        worldVelocity = inputVelocity * currentSpeed;

        transform.position += (worldVelocity * Time.deltaTime);
    }

    private void moveToTargetPoint(Vector3 targetPosition)
    {
        float step = maxMoveSpeed * Time.deltaTime;   
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        transform.LookAt(targetPosition);
    }

    private IEnumerator legUpdate()
    {
        while (true)
        {
            do
            {
                frontLeftLegStep.TryMove();
                backRightLegStep.TryMove();
                MidLeftLegStep.TryMove();

                if (frontRightLegStep.requiresForceStep) 
                {
                    forceUpdateLeg(frontRightLegStep);
                }

                if (backLeftLegStep.requiresForceStep)
                {
                    forceUpdateLeg(backLeftLegStep);
                }

                if (MidRightLegStep.requiresForceStep)
                {
                    forceUpdateLeg(MidRightLegStep);
                }

                yield return null;
            } while (backRightLegStep.isMoving || frontLeftLegStep.isMoving || MidLeftLegStep.isMoving);

            do
            {
                frontRightLegStep.TryMove();
                backLeftLegStep.TryMove();
                MidRightLegStep.TryMove();

                if (frontLeftLegStep.requiresForceStep)
                {
                    forceUpdateLeg(frontLeftLegStep);
                }

                if (backRightLegStep.requiresForceStep)
                {
                    forceUpdateLeg(backRightLegStep);
                }

                if (MidLeftLegStep.requiresForceStep)
                {
                    forceUpdateLeg(MidLeftLegStep);
                }
                yield return null;
            } while (backLeftLegStep.isMoving || frontRightLegStep.isMoving || MidRightLegStep.isMoving);
        }
    }

    private void offsetBody()
    {       
        float yOffsetBobbing = yOffset;
        if(currentMoveDir.magnitude == 0)
        {
            yOffsetBobbing = Mathf.Lerp(yOffset - 1f, yOffset + 1f, Mathf.PingPong(Time.time, 1));
        }

        float heightOffset = getAverageHeight().y + yOffsetBobbing;
        Vector3 newNormal = getAverageNormals();

        transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, heightOffset, heightAdjustmentDamping * Time.deltaTime), transform.position.z);
        if(autoPilot == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, newNormal), newNormal), 20 * Time.deltaTime);
        }
    }
    private Vector3 getAverageHeight()
    {
        Vector3 average = Vector3.zero;

        for (int i = 0; i < allLegs.Count; i++)
        {
            average += allLegs[i].transform.position;
        }

        return (average / allLegs.Count);
    }
    private Vector3 getAverageNormals()
    {
        Vector3 average = Vector3.zero;

        for (int i = 0; i < allLegs.Count; i++)
        {
            average += allLegs[i].stepNormal;
        }

        return (average / allLegs.Count);
    }
    private void forceUpdateAllLegs()
    {
        frontLeftLegStep.TryMove();
        backRightLegStep.TryMove();
        frontRightLegStep.TryMove();
        backLeftLegStep.TryMove();
        MidRightLegStep.TryMove();
        MidLeftLegStep.TryMove();

    }
    private void shareCurrentVelocity()
    {
        bool movingForwards;
        if(currentMoveDir.z > .1f) { movingForwards = true; }
        else { movingForwards = false;}

        foreach (LegStep leg in allLegs)
        {
            leg.movingForwards = movingForwards;
        }
    }

    private void forceUpdateLeg(LegStep thisLeg)
    {
        thisLeg.TryMove();
    }

}
