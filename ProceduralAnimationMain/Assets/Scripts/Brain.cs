using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Brain : MonoBehaviour
{
    [SerializeField] private IK_Type thisIKtype;
    [SerializeField] Transform target;

    [Header("IK")]
    [SerializeField] Transform IK_Holder; 
    [SerializeField] List<LegStepper> legs; //Each leg is stored here

    [Header("HeadTracking")]
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle; //Max turn angle of the head
    [SerializeField] float headTurnSpeed; //How fast the head should turn/track target

    [Header("EyeTracking")]
    [SerializeField] Transform rightEye;
    [SerializeField] Transform leftEye;

    [SerializeField] float rightEyeMinYRotation; //Minimun and maximum rotations each eye will allow
    [SerializeField] float rightEyeMaxYRotation;
    [SerializeField] float leftEyeMinYRotation;
    [SerializeField] float leftEyeMaxYRotation;
    [SerializeField] float eyeTurnSpeed; //How fast the eyes should turn/track target

    [Header("BodyOrientation")]
    [SerializeField] float maxBob; //How high should the idle Bobbing raise the IK
    [SerializeField] float bobSpeed; //How fast should bob move the IK
    [SerializeField] float rotationOrientationSpeed;    //How fast the body should move on the Yaxis to match the average height of the legs.
    [SerializeField] float HeightOrientationSpeed;
    [SerializeField] float orientationSmooth;
    [SerializeField] Transform rayOrigin;
    //[SerializeField] List<Transform> rays;
    [SerializeField] float rayDist; //Distance ray will go, multiplied by the direction the ray will go
    Vector3 curNormal = Vector3.up; // smoothed terrain normal

    [Header("Locomotion")]
    [SerializeField] Transform mainRoot;
    //Max turning and moving speeds
    [SerializeField] float turnSpeed;
    [SerializeField] float moveSpeed;
    //How fast the IK can reach its full speed
    [SerializeField] float turnAcceleration;
    [SerializeField] float moveAcceleration;
    //The minimum and maximum distance the transform should be from the target
    [SerializeField] float minDistanceToTarget;
    [SerializeField] float maxDistanceToTarget;
    //The max angle the target should be at in relation to this transforms forward vector
    [SerializeField] float maxAngToTarget;

    //Velocity in world space
    SmoothDamp.Vector3 currentVelocity;
    SmoothDamp.Float currentAngularVelocity;
    SmoothDamp.Vector3 currentVelocityBobbing;

    [SerializeField] private bool isRotating;
    [SerializeField] private LayerMask whatIsWalkable;

    void Awake()
    {
        //Release the IK!
        IK_Holder.parent = null;

        StartCoroutine(LegStepUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        LocomotionUpdate();
        alterBodyHeight();
        alterBodyOrientation();
    }

    void LateUpdate()
    {
        headTrackerUpdate();
        EyeTrackerUpdate();       
    }

    //!!!Moves headbone/Anchor to follow position of designated target!!!
    private void headTrackerUpdate()
    {
        //Storing current head rotation
        Quaternion currentLocalRotation = headBone.localRotation;
        //set rotation to 0 so we use the heads zero rotation when transforming the position
        headBone.localRotation = Quaternion.identity;
        //get direction in world space
        Vector3 targetWorldLookDir = target.position - headBone.position;
        //transforms the above world direciton into local space relative to the head of the IK
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);
        
        targetLocalLookDir = Vector3.RotateTowards(Vector3.forward, targetLocalLookDir, Mathf.Deg2Rad * headMaxTurnAngle, 0);
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
        //Adding 1 - Mathf.Exp(-headTurnSpeed * Time.deltaTime makes the rotation frame rate independent by using a damping function
        headBone.localRotation = Quaternion.Slerp(currentLocalRotation, targetLocalRotation, 1 - Mathf.Exp(-headTurnSpeed * Time.deltaTime));
    }

    //!!!Moves eyes to follow position of current target!!!
    private void EyeTrackerUpdate()
    {
        //Direction towards target in relation to the headBone
        Quaternion targetEyeRotation = Quaternion.LookRotation(target.position - headBone.position, transform.up);

        rightEye.rotation = Quaternion.Slerp(rightEye.rotation, targetEyeRotation, 1 - Mathf.Exp(-eyeTurnSpeed * Time.deltaTime));
        leftEye.rotation = Quaternion.Slerp(leftEye.rotation, targetEyeRotation, 1 - Mathf.Exp(-eyeTurnSpeed * Time.deltaTime));

        //Store current eye rotation values
        float rightEyeCurrentYrotation = rightEye.localEulerAngles.y;
        float leftEyeCurrentYrotation = leftEye.localEulerAngles.y;

        //If the eyes exceed 180f on the Y axis they should reset to -180f so they are not turning continously
        if (rightEyeCurrentYrotation > 180f)
        {
            rightEyeCurrentYrotation -= 360f;
        }
        if (leftEyeCurrentYrotation > 180f)
        {
            leftEyeCurrentYrotation -= 360f;
        }       
        //Clamp both yes Y axis rotation value to their respective mins/max
        float rightEyeYaxisClamped = Mathf.Clamp(rightEyeCurrentYrotation, rightEyeMinYRotation, rightEyeMaxYRotation);
        float leftEyeYaxisClamped = Mathf.Clamp(leftEyeCurrentYrotation, leftEyeMinYRotation, leftEyeMaxYRotation);

        //Apply clamped Y value only on the Y axis as we only want to move eye on its Y axis.
        rightEye.localEulerAngles = new Vector3(rightEye.localEulerAngles.x, rightEyeYaxisClamped, rightEye.localEulerAngles.z);
        leftEye.localEulerAngles = new Vector3(leftEye.localEulerAngles.x, leftEyeYaxisClamped, leftEye.localEulerAngles.z);

    }

    //!!!Calls the tryMove function on each leg and check which leg should step when. Legs will currently step diagonally!!!
    private IEnumerator LegStepUpdate()
    {
        if(thisIKtype == IK_Type.Quadruped)
        {
            //This will run constantly
            while (true)
            {
                //Move legs Diagonally
                do //do this <<<
                {
                    legs[0].TryMove();
                    legs[3].TryMove();

                    yield return null;

                } while (legs[0].isMoving || legs[3].isMoving); // <<< while this is true

                //and again
                do
                {
                    legs[1].TryMove();
                    legs[2].TryMove();

                    yield return null;

                } while (legs[1].isMoving || legs[2].isMoving);
            }
        }
        else
        {
            //This will run constantly
            while (true)
            {
                //Move legs Diagonally
                do //do this <<<
                {
                    legs[0].TryMove();

                    yield return null;

                } while (legs[0].isMoving); // <<< while this is true

                //and again
                do
                {
                    legs[1].TryMove();
                    yield return null;

                } while (legs[1].isMoving);
            }
        }
      
    }

    //!!!Moves transform toward or away from its target using min and max thresholds!!!
    void LocomotionUpdate()
    {
        //Direction from transform to the target
        Vector3 targetDir = target.position - transform.position;
        //Project vector toward the targetPos on the local XZ plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(targetDir, transform.up);

        //Get the angle from the forward vector of this transform to the direction of the target
        float angleToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);        
        //Use Lerp function to gradually increase/decrease the velocity
        float targetAngularVelocity = Mathf.Sign(angleToTarget)  * Mathf.InverseLerp(25f, 45f, Mathf.Abs(angleToTarget)) * turnSpeed;
        currentAngularVelocity.Step(targetAngularVelocity, turnAcceleration);

        Vector3 targetVelocity = Vector3.zero;            

        
        //We dont want to move towards the target if we are facing away, rotate first.
        if (Mathf.Abs(angleToTarget) < 90f)
        {
            float distanceToTarget = towardTargetProjected.magnitude;

            if(distanceToTarget > maxDistanceToTarget)
            {
                //Move toward target
                targetVelocity = moveSpeed * towardTargetProjected.normalized;

                float heightDifference = target.position.y - transform.position.y;

                if(heightDifference > 40f || heightDifference < -40f)
                {
                    Debug.Log("Target is too high to reach");
                    targetVelocity = Vector3.zero;
                }
            }
            else if(distanceToTarget < minDistanceToTarget)
            {
                //Move away from target
                targetVelocity = moveSpeed * -towardTargetProjected.normalized * 0.66f;
            }

            targetVelocity *= Mathf.InverseLerp(turnSpeed, turnSpeed * .2f, Mathf.Abs(currentAngularVelocity));
        }

        currentVelocity.Step(targetVelocity, moveAcceleration);
        transform.position += currentVelocity.currentValue * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * currentAngularVelocity, transform.up);
    } 

    private void alterBodyOrientation()
    {
        RaycastHit mainHit;
        //RaycastHit hit1; //rf
        //RaycastHit hit2; //lf
        //RaycastHit hit3; //rr
        //RaycastHit hit4; //lr

        if (Physics.Raycast(rayOrigin.transform.position, -rayOrigin.up * rayDist, out mainHit, whatIsWalkable))
        {
            #region orientation using crossPosition
            //Physics.Raycast(rays[0].transform.position, -rays[0].up * rayDist, out hit1, whatIsWalkable); //Castiing rays from four points of the IK
            //Physics.Raycast(rays[1].transform.position, -rays[1].up * rayDist, out hit2, whatIsWalkable);
            //Physics.Raycast(rays[2].transform.position, -rays[2].up * rayDist, out hit3, whatIsWalkable);
            //Physics.Raycast(rays[3].transform.position, -rays[3].up * rayDist, out hit4, whatIsWalkable);

            //Debug.DrawRay(rayOrigin.transform.position, -rayOrigin.up * rayDist); //Draw rays down from each point
            //Debug.DrawRay(rays[0].transform.position, -rays[0].up * rayDist);
            //Debug.DrawRay(rays[1].transform.position, -rays[1].up * rayDist);
            //Debug.DrawRay(rays[2].transform.position, -rays[2].up * rayDist);
            //Debug.DrawRay(rays[3].transform.position, -rays[3].up * rayDist);

            //Vector3 a = hit3.point - hit4.point; //Get the vectors that connect each point/legs
            //Vector3 b = hit1.point - hit3.point;
            //Vector3 c = hit2.point - hit1.point;
            //Vector3 d = hit3.point - hit1.point;

            //Vector3 crossBA = Vector3.Cross(b, a); //Get the cross product of each connection
            //Vector3 crossCB = Vector3.Cross(c, b);
            //Vector3 crossDC = Vector3.Cross(d, c);
            //Vector3 crossAD = Vector3.Cross(a, d);

            //Vector3 finalCross = (crossBA + crossCB + crossDC + crossAD).normalized;
            #endregion

            curNormal = Vector3.Lerp(curNormal, mainHit.normal, 4 * Time.deltaTime); //Control speed in which the current normal translates into the hit.normal
            Quaternion grndTilt = Quaternion.FromToRotation(Vector3.up, curNormal); //Creates a rotation that rotates using the upVector to the new/current normal
            //Smoothing controls how much the IK should react to the new normal the raycast found
            Quaternion newRot = Quaternion.Euler(grndTilt.x * orientationSmooth, 0, grndTilt.z * orientationSmooth); //excludes the Y rotation as locomotion handles the Y axis
            mainRoot.localRotation = Quaternion.Slerp(mainRoot.localRotation, newRot, rotationOrientationSpeed * Time.deltaTime); //applies to new rotational value to IK
        }
    }

    //!!!Alters height of body depedning on the average Yvalue position of each leg!!!
    private void alterBodyHeight()
    {
        float averageLegPosY = returnAverage();
        //smoothly Lerps the Y position of the mainRootBone which means this will not interfere with any movement translations in the locomotionUpdate function
        float bobbingAmount = Mathf.PingPong(Time.time * bobSpeed, maxBob); //Ping pong this float which is added to the legPos to give a bobbing effect
        mainRoot.localPosition = new Vector3(mainRoot.localPosition.x, Mathf.Lerp(mainRoot.localPosition.y, averageLegPosY + bobbingAmount, HeightOrientationSpeed * Time.deltaTime), mainRoot.localPosition.z);
    }

    //!!!Returns average from a list!!!
    private float returnAverage()
    {
        //Created new list of float values
        var legYValues = new List<float>();

        //Add each legs,foot y position value to the list
        foreach (LegStepper leg in legs)
        {
            legYValues.Add(leg.transform.position.y);
        }

        //Adds all values within the list together
        float totalOfAllLegs = legYValues.Sum();
        float average = totalOfAllLegs / legYValues.Count; //Gets the average by dividing totalOfAllLegs by the length on the list
        //returns the average
        return average;
    }

    private enum IK_Type
    {
        Quadruped,
        Biped
    }

}
