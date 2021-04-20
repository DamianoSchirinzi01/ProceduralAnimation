using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [SerializeField] List<LegStep> allLegs;

    [SerializeField] LegStep frontRightLegStep;
    [SerializeField] LegStep frontLeftLegStep;
    [SerializeField] LegStep MidRightLegStep;
    [SerializeField] LegStep MidLeftLegStep;
    [SerializeField] LegStep backRightLegStep;
    [SerializeField] LegStep backLeftLegStep;

    public float yOffset;
    public float bodyRotationSpeed;

    public Vector3 inputVelocity;
    public Vector3 worldVelocity;
    public Vector3 currentMoveDir;
    public float currentSpeed;
    public float maxMoveSpeed;
    public float accelerationSpeed;

    private void Awake()
    {
        StartCoroutine(legUpdate());
    }

    private void Update()
    {
        input();
        offsetBody();
    }

    private void input()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        currentMoveDir = new Vector3(horizontal, 0, vertical);
        currentMoveDir.Normalize();

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(transform.up * bodyRotationSpeed * Time.deltaTime);
            forceUpdateAll();
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(-transform.up * bodyRotationSpeed * Time.deltaTime);
            forceUpdateAll();

        }

        Move(currentMoveDir);
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
                    forceUpdate(frontRightLegStep);
                }

                if (backLeftLegStep.requiresForceStep)
                {
                    forceUpdate(backLeftLegStep);
                }

                if (MidRightLegStep.requiresForceStep)
                {
                    forceUpdate(MidRightLegStep);
                }

                yield return null;
            } while (backRightLegStep.isMoving || frontLeftLegStep.isMoving || MidRightLegStep.isMoving);

            do
            {
                frontRightLegStep.TryMove();
                backLeftLegStep.TryMove();
                MidRightLegStep.TryMove();

                if (frontLeftLegStep.requiresForceStep)
                {
                    forceUpdate(frontLeftLegStep);
                }

                if (backRightLegStep.requiresForceStep)
                {
                    forceUpdate(backRightLegStep);
                }

                if (MidLeftLegStep.requiresForceStep)
                {
                    forceUpdate(MidLeftLegStep);
                }
                yield return null;
            } while (backLeftLegStep.isMoving || frontRightLegStep.isMoving || MidRightLegStep.isMoving);
        }
    }

    private void offsetBody()
    {
        float heightOffset = getAverageHeight().y + yOffset;
        Vector3 newNormal = getAverageNormals();

        transform.position = new Vector3(transform.position.x, heightOffset, transform.position.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, newNormal), newNormal), 20 * Time.deltaTime);
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

    private void forceUpdateAll()
    {
        frontLeftLegStep.TryMove();
        backRightLegStep.TryMove();
        frontRightLegStep.TryMove();
        backLeftLegStep.TryMove();
        MidRightLegStep.TryMove();
        MidLeftLegStep.TryMove();

    }

    private void forceUpdate(LegStep thisLeg)
    {
        thisLeg.TryMove();
    }

}
