using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    [SerializeField] private float initialForceStrength = 30000f; // 質量1500に適した力

    private bool flag = false;

    public int number_getItem = 1;

    void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

        if (m_Rigidbody.isKinematic == false && !flag)
        {
            // 初速を与える（車オブジェクトの前方向に）
            m_Rigidbody.AddForce(transform.forward * initialForceStrength, ForceMode.Impulse);
            flag = true;
        }

        // 時速 (km/h) に変換
        //Debug.Log("Speed: " + (m_Rigidbody.velocity.magnitude * 3.6f).ToString("F1") + " km/h");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            // アイテム取得時も車の前方向に力を加える
            m_Rigidbody.AddForce(transform.forward * initialForceStrength, ForceMode.Impulse);
            number_getItem++;
        }
    }
}