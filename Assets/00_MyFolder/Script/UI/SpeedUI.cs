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
    [SerializeField] private string speedFormat = "時速{0:F1}km"; // 表示フォーマット
    [SerializeField] private float updateInterval = 0.1f;         // 更新間隔（秒）

    private Rigidbody carRigidbody;
    private float lastUpdateTime;

    void Start()
    {
        // 車のRigidbodyを取得
        if (carObject != null)
        {
            carRigidbody = carObject.GetComponent<Rigidbody>();
            if (carRigidbody == null)
            {
                Debug.LogWarning("Car object doesn't have a Rigidbody component!");
            }
        }
        else
        {
            Debug.LogWarning("Car object is not assigned!");
        }

        // TMPテキストが設定されていない場合は警告
        if (speedText == null)
        {
            Debug.LogWarning("Speed text component is not assigned!");
        }

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
        if (carRigidbody != null && speedText != null)
        {
            // 車の速度を取得（m/s）
            float speedInMPS = carRigidbody.velocity.magnitude;

            // 時速（km/h）に変換（m/s × 3.6 = km/h）
            float speedInKMH = speedInMPS * 3.6f;

            // TMPテキストに表示
            speedText.text = string.Format(speedFormat, speedInKMH);
        }
    }

    // 車のオブジェクトを動的に設定するメソッド（必要に応じて）
    public void SetCarObject(GameObject car)
    {
        carObject = car;
        if (carObject != null)
        {
            carRigidbody = carObject.GetComponent<Rigidbody>();
        }
    }
}