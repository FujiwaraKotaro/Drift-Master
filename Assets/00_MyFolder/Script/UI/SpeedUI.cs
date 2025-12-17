using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject carObject; // 車のGameObject
    [SerializeField] private TMP_Text speedText;   // 速度表示用のTMPテキスト

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f;         // 更新間隔（秒）

    private Rigidbody carRigidbody;
    private float lastUpdateTime;

    void Start()
    {
        carRigidbody = carObject.GetComponent<Rigidbody>();
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        // 指定された間隔で速度を更新
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateSpeedDisplay();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdateSpeedDisplay()
    {
        // 車の速度を取得（m/s）
        float speedInMPS = carRigidbody.velocity.magnitude;

        // 時速（km/h）に変換（m/s × 3.6 = km/h）
        float speedInKMH = speedInMPS * 3.6f;

        // TMPテキストに表示
        speedText.text = string.Format("時速{0:F1}km", speedInKMH);
    }
}