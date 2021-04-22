using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour
{
    public List<Transform> positions;
    public float MoveSpeed = 8;
    Coroutine MoveIE;

    void Start()
    {
        StartCoroutine(moveObject());

    }

    IEnumerator moveObject()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            MoveIE = StartCoroutine(Moving(i));
            yield return MoveIE;
        }
    }

    IEnumerator Moving(int currentPosition)
    {
        while (transform.position != positions[currentPosition].position)
        {
            transform.position = Vector3.MoveTowards(transform.position, positions[currentPosition].position, MoveSpeed * Time.deltaTime);
            yield return null;
        }

    }

}
