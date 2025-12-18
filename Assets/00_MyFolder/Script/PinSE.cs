using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSE : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Pin"))
        {
            SoundManager.Instance.PlaySE("PinSE");
        }
    }
}
