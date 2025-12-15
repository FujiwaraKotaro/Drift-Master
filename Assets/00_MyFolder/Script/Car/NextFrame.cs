using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextFrame : MonoBehaviour
{
    [SerializeField] BowlingGameDirector gameDirector;

    [Header("Speed Detection Settings")]
    [SerializeField] private float speedCheckInterval = 0.1f; // 速度をチェックする間隔（秒）
    [SerializeField] private float rapidDecelerationThreshold = 5f; // 急激な減速とみなす閾値（m/s²）
    [SerializeField] private float minimumSpeed = 1f; // 判定を開始する最低速度（m/s）

    private Rigidbody rb;
    private Vector3 previousVelocity;
    private float lastSpeedCheckTime;
    private bool isSpeedMonitoring = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody component not found on CollisionOcean object!");
        }
    }

    private void Update()
    {
        if (rb != null && !gameDirector.isJudging)
        {
            CheckSpeedDeceleration();
        }
    }

    private void CheckSpeedDeceleration()
    {
        float currentTime = Time.time;

        // 指定された間隔でチェック
        if (currentTime - lastSpeedCheckTime >= speedCheckInterval)
        {
            Vector3 currentVelocity = rb.velocity;
            float currentSpeed = currentVelocity.magnitude;

            // 最低速度以上の場合のみ監視開始
            if (currentSpeed >= minimumSpeed)
            {
                if (isSpeedMonitoring)
                {
                    // 前回の速度と比較して減速度を計算
                    float previousSpeed = previousVelocity.magnitude;
                    float speedDifference = previousSpeed - currentSpeed;
                    float deceleration = speedDifference / speedCheckInterval;

                    // 急激な減速を検知
                    if (deceleration >= rapidDecelerationThreshold)
                    {
                        Debug.Log($"急激な減速を検知: {deceleration:F2} m/s²");
                        StartCoroutine(gameDirector.ProcessThrowResult());
                        isSpeedMonitoring = false; // 一度判定したら監視を停止
                    }
                }
                else
                {
                    // 監視開始
                    isSpeedMonitoring = true;
                }
            }
            else if (isSpeedMonitoring && currentSpeed < minimumSpeed * 0.5f)
            {
                // 速度が非常に低くなった場合も判定
                Debug.Log("速度が極低速になったため判定実行");
                StartCoroutine(gameDirector.ProcessThrowResult());
                isSpeedMonitoring = false;
            }

            previousVelocity = currentVelocity;
            lastSpeedCheckTime = currentTime;
        }
    }

    // 監視をリセットする公開メソッド（ゲーム開始時などに使用）
    public void ResetSpeedMonitoring()
    {
        isSpeedMonitoring = false;
        previousVelocity = Vector3.zero;
        lastSpeedCheckTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameDirector.isJudging && other.gameObject.CompareTag("Ocean"))
        {
            StartCoroutine(gameDirector.ProcessThrowResult());
        }
    }
}