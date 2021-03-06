using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour
{
    [SerializeField] private Vector3 targetCurrentMovePoint;
    [SerializeField] private float timeBetweenPointSwap;
    [SerializeField] private float targetMoveSpeed;
    [SerializeField] private List<Transform> currentMovePoints;

    // Start is called before the first frame update
    void Start()
    {
        targetCurrentMovePoint = currentMovePoints[0].position;
        StartCoroutine(cycleMovePoints());

    }

    void Update()
    {
        moveTarget();
    }

    IEnumerator cycleMovePoints()
    {
        while (true)
        {
            foreach (Transform movePoint in currentMovePoints)
            {
                targetCurrentMovePoint = movePoint.position;
                yield return new WaitForSeconds(timeBetweenPointSwap);
            }
        }
    }

    private void moveTarget()
    {
        transform.position = Vector3.Lerp(transform.position, targetCurrentMovePoint, 1 - Mathf.Exp(-targetMoveSpeed * Time.deltaTime));
    }

}
