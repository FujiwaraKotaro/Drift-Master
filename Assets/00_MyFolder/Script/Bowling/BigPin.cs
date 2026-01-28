using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPin : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float speedThreshold = 500f; // km/h

    private Rigidbody rb;
    private bool wasKinematic;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BigPin: Rigidbodyコンポーネントが見つかりません");
            return;
        }

        // 初期状態を記録
        wasKinematic = rb.isKinematic;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;
        if (!collision.gameObject.CompareTag("Car")) return;

        // 衝突してきたオブジェクトのRigidbodyを取得
        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null) return;

        // 衝突速度を計算 (m/s)
        float collisionSpeed = otherRb.velocity.magnitude;

        // km/hに変換 (m/s × 3.6 = km/h)
        float speedKmh = collisionSpeed * 3.6f;

        Debug.Log($"BigPin衝突検出: 衝突速度 = {speedKmh:F1} km/h (閾値: {speedThreshold} km/h)");

        // 閾値以下の速度ならkinematicにして動かないようにする
        if (speedKmh <= speedThreshold)
        {
            rb.isKinematic = true;
        }
    }

    // kinematic状態をリセットするメソッド
    public void ResetKinematic()
    {
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            Debug.Log("BigPin: kinematic状態をリセットしました");
        }
    }

    // 閾値を変更するメソッド
    public void SetSpeedThreshold(float newThreshold)
    {
        speedThreshold = newThreshold;
        Debug.Log($"BigPin: 速度閾値を {newThreshold} km/h に変更しました");
    }

    // 現在の速度を取得するメソッド
    public float GetCurrentSpeed()
    {
        if (rb != null)
        {
            return rb.velocity.magnitude * 3.6f; // km/h
        }
        return 0f;
    }

    // kinematic状態を確認するメソッド
    public bool IsKinematic()
    {
        return rb != null ? rb.isKinematic : true;
    }
}