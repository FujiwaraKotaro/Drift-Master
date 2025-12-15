using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionOcean : MonoBehaviour
{
    [SerializeField] BowlingGameDirector gameDirector;
    private void OnTriggerEnter(Collider other)
    {
        if (!gameDirector.isJudging && other.gameObject.CompareTag("Ocean"))
        {
            StartCoroutine(gameDirector.ProcessThrowResult());
        }
    }
}
