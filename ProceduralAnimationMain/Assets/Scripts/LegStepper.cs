using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegStepper : MonoBehaviour
{
    //Position and Rotation we want to stay in range of.
    [SerializeField] Transform homeTransform;
    //Checks how far this transform is to the above home target
    [SerializeField] float distanceToHome;
    [SerializeField] float rayYoffset;
    //How far above the ground the legStepper should reset
    [SerializeField] float yOffset;
    //Amount to lift leg by when moving towards center of target
    [SerializeField] float liftAmount;
    //Fraction of the distance from home we want to overshoot by
    [SerializeField] float stepOvershootFraction;
    //How long before we should take a step?
    [SerializeField] float stepDistanceThreshold;
    float stickingThreshold; //If this limit has been exceeded, this leg is most likely stick and cannot find a position
    [SerializeField] float stepAngleThreshold;
    //How long will a step take.
    [SerializeField] float moveDuration;

    //What layer will the IK be allowed to walk across
    [SerializeField] LayerMask whatIsWalkable;

    public bool isMoving { get; private set; }
    [SerializeField] bool showRays;

    public float originalStepThreshold { get; private set; }

    void Awake()
    {
        init();
    }

    private void init()
    {
        TryMove();
        originalStepThreshold = stepDistanceThreshold;
    }
    
    public void TryMove()
    {
        //We dont need to move again right now
        if (isMoving) return;

        //Check distance and angle between two targets
        distanceToHome = Vector3.Distance(transform.position, homeTransform.position);
        float angleToHome = Quaternion.Angle(transform.rotation, homeTransform.rotation);

        if (distanceToHome > stepDistanceThreshold || angleToHome > stepAngleThreshold) //If distance or angle has been exceeded. Move.
        {
            //Check if a suitable target Pos has been found
            if(findGroundedPos(out Vector3 endNormal, out Vector3 endPos))
            {
                //Rotation facing the direction the the homePosition but aligned with the transfroms local XZ plane
                Quaternion endRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(homeTransform.forward, endNormal), endNormal);
                StartCoroutine(MoveHome(endPos, endRot, moveDuration));
            }
        }
    }

    //shoots a ray from above the homeTransfrom down to find anything that is walkable.
    //When a suitable position/rotation is found, it returns true
    bool findGroundedPos(out Vector3 normal, out Vector3 pos)
    {
        //get the direction towards the home pos from this transform. normalise the vector as we dont need the magnitude, only the direction
        Vector3 dirToHome = (homeTransform.position - transform.position).normalized;

        float overshootDistance = stepDistanceThreshold - stepOvershootFraction;
        Vector3 overshootVector = dirToHome * overshootDistance;

        //The start pos of the ray
        Vector3 rayOrigin = homeTransform.position + overshootVector + homeTransform.up * rayYoffset;
        if (showRays)
        {
            Debug.DrawRay(rayOrigin, -homeTransform.up * rayYoffset, Color.red, 3f);
        }

        if (Physics.Raycast(rayOrigin, -homeTransform.up, out RaycastHit hit, Mathf.Infinity, whatIsWalkable))
        {
            //Suitable point found
            pos = hit.point;
            normal = hit.normal;
            return true;
        }

        //Keep searching for a suitable point
        pos = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

    private IEnumerator MoveHome(Vector3 endPoint, Quaternion endRot, float moveTime)
    {
        isMoving = true;

        //store start Position and rotation
        Quaternion startRot = transform.rotation;
        Vector3 startPoint = transform.position;
    
        //Applying the vector created to the target position
        endPoint += homeTransform.up * yOffset;

        Vector3 centerPoint = (startPoint + endPoint) / 2f;
        centerPoint += (homeTransform.up * liftAmount) * Vector3.Distance(startPoint, endPoint) / 2f;

        //time since this step has started
        float timeElapsed = 0;

        do
        {
            //increment time
            timeElapsed += Time.deltaTime;

            //normalise the time value
            float normalisedTime = timeElapsed / moveDuration;
            normalisedTime = Easing.EaseInOutCubic(normalisedTime);

            //Move the transform towards the target position/rotation over the normalised time amount
            transform.position = Vector3.Lerp(Vector3.Lerp(startPoint, centerPoint, normalisedTime), Vector3.Lerp(startPoint, endPoint, normalisedTime), normalisedTime);
            transform.rotation = Quaternion.Slerp(startRot, endRot, normalisedTime);

            yield return null;
        } while (timeElapsed < moveDuration);

        //This leg is no longer moving
        isMoving = false;
    }

    void OnDrawGizmosSelected()
    {
        if (isMoving)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, homeTransform.position);
        Gizmos.DrawWireCube(homeTransform.position, Vector3.one * 0.1f);
    }
}
