using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegStep : MonoBehaviour
{
    [SerializeField] Transform rayCastOrigin;
    [SerializeField] Transform homeTransform;

    public Vector3 stepNormal;
    private Vector3 resetPos;
    private Quaternion resetRot;

    [SerializeField] float sphereCastRadius;
    [SerializeField] float stepOverShootFraction;
    [SerializeField] float stepAtThreshold;
    [SerializeField] float moveDuration;
    [SerializeField] float liftAmount;
    [SerializeField] float rayRotationOffset;

    public bool movingForwards;
    public bool isMoving;
    public bool requiresForceStep;

    public LayerMask whatIsWalkable;

    private void Start()
    {
        storeRestValue();
    }

    private void Update()
    {
        requiresForceStep = checkDistanceToHome();
    }

    private void storeRestValue()
    {
        resetPos = transform.position;
        resetRot = transform.rotation;
    }     

    private Vector3 castRays()
    {        
        Vector3 direction = -transform.up + (transform.parent.right / rayRotationOffset);
        Vector3 newPosition = Vector3.zero;

        RaycastHit hit;

        if(Physics.SphereCast(rayCastOrigin.position, sphereCastRadius, direction, out hit, 15f, whatIsWalkable))
        {
            Debug.DrawLine(rayCastOrigin.position, hit.point, Color.green, 1f);
            Debug.Log(hit.transform.name);
            newPosition = hit.point;
            stepNormal = hit.normal;
        }
        else
        {
            newPosition = resetPos;
            stepNormal = Vector3.zero;
        }

        return newPosition;
    }

    public void TryMove()
    {
        if (isMoving)
        {
            Vector3 newHomePos = castRays();
            if (newHomePos == Vector3.zero)
            {
                Debug.Log("Vector is zero");
                homeTransform.position = resetPos;
                homeTransform.rotation = resetRot;
            }
            if (homeTransform.position != newHomePos)
            {
                homeTransform.position = newHomePos;
            }
            return;
        }

        float distanceFromHome = Vector3.Distance(transform.position, homeTransform.position);

        if (distanceFromHome > stepAtThreshold)
        {
            StartCoroutine(moveToHome());
        }
    }

    IEnumerator moveToHome()
    {
        isMoving = true;       

        Vector3 startPoint = transform.position;
        Quaternion startRot = transform.rotation;

        Quaternion endRot = homeTransform.rotation;

        Vector3 towardHome = (homeTransform.position - transform.position);

        float overShootDist = stepAtThreshold * stepOverShootFraction;
        Vector3 overshootVector = towardHome * overShootDist;

        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        Vector3 endPoint = homeTransform.position + overshootVector;

        Vector3 centerPoint = (startPoint + endPoint) / 2;
        centerPoint += homeTransform.up * liftAmount * Vector3.Distance(startPoint, endPoint) / 2f;

        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / moveDuration;
            normalizedTime = Easing.EaseInOutCubic(normalizedTime);

            transform.position = Vector3.Lerp(
                    Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                    Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                    normalizedTime); 
            transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            yield return null;
        }
        while (timeElapsed < moveDuration);

        isMoving = false;

    }

    private bool checkDistanceToHome()
    {
        float distToHome = Vector3.Distance(transform.position, homeTransform.position);

        if (distToHome > stepAtThreshold + 1f)
        {
            Debug.Log("Too far from threshold!");
            return true;
        }
        else
        {
            return false;
        }
    } 
}
