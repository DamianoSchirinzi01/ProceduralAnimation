using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLegPlacement : MonoBehaviour
{
    public bool legGrounded;

    public Transform ikTarget;
    public Transform ikPoleTarget;
    public Vector3 stepPoint;
    public Vector3 stepNormal;

    public Vector3 worldTarget = Vector3.zero;
    public Vector3 worldVelocity = Vector3.right;
    public Vector3 optimalRestingPostion = Vector3.forward;
    public Vector3 restingPosition
    {
        get
        {
            return transform.TransformPoint(optimalRestingPostion);
        }
    }
    public Vector3 desiredPosition
    {
        get
        {
            return restingPosition + worldVelocity + (Random.insideUnitSphere * placementRandomization);
        }
    }

    public float placementRandomization = 0.5f;
    public float stepRadius = 0.25f;
    public AnimationCurve stepHeightCurve;
    public float stepHeightMultiplier = 0.25f;
    public float stepCooldown = 1f;
    public float stepDuration = 0.5f;
    public float stepOffset;
    public float lastStep = 0;

    public LayerMask whatIsGround;
    public float percent
    {
        get
        {
            return Mathf.Clamp01((Time.time - lastStep) / stepDuration);
        }
    }

    private void Start()
    {
        worldVelocity = Vector3.zero;
        lastStep = Time.time + stepCooldown * stepOffset;
        ikTarget.position = restingPosition;
        Step();
    }

    private void Update()
    {
        UpdateIkTarget();
        if (Time.time > lastStep + stepCooldown)
        {
            Step();
        }
    }
    public void UpdateIkTarget()
    {
        stepPoint = adjustPos(worldTarget + worldVelocity);
        ikTarget.position = Vector3.Lerp(ikTarget.position, stepPoint, percent) + stepNormal * stepHeightCurve.Evaluate(percent) * stepHeightMultiplier;
    }
    public void Step()
    {
        stepPoint = worldTarget = adjustPos(desiredPosition);
        lastStep = Time.time;
    }

    public Vector3 adjustPos( Vector3 position)
    {
        Vector3 direction = position - ikPoleTarget.position;
        RaycastHit hit;
        if(Physics.SphereCast(ikPoleTarget.position, stepRadius, direction, out hit, direction.magnitude * 2f, whatIsGround))
        {
            Debug.Log("Hit: " + hit.transform.name + "pos:" + hit.transform.position);
            Debug.DrawLine(ikPoleTarget.position, hit.point, Color.green, 0);
            position = hit.point;
            stepNormal = hit.normal;
            legGrounded = true;
        }
        else
        {
            position = restingPosition;
            stepNormal = Vector3.zero;
            legGrounded = false;
        }
        return position;
        
    }

}
