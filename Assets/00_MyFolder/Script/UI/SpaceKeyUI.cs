using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceKeyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject spaceKeyGuideUI; // SpaceKeyGuideUIオブジェクト

    [Header("References")]
    [SerializeField] private BowlingGameDirector gameDirector; // BowlingGameDirectorの参照

    private bool lastReadyState = false; // 前回のisReadyToThrow状態を記録

    void Start()
    {
        spaceKeyGuideUI.SetActive(false);
    }

    void Update()
    {
        if (gameDirector == null || spaceKeyGuideUI == null) return;

        // isReadyToThrowの状態変化を監視
        bool currentReadyState = GetIsReadyToThrow();

        // 状態が変化した時のみUIを更新
        if (currentReadyState != lastReadyState)
        {
            spaceKeyGuideUI.SetActive(currentReadyState);
            lastReadyState = currentReadyState;
        }
    }

    // isReadyToThrowの状態を取得
    private bool GetIsReadyToThrow()
    {
        // BowlingGameDirectorのisReadyToThrowがprivateなので、
        // 以下の条件で判定します：
        // 1. ゲームが開始されている
        // 2. 判定中でない
        // 3. 車のRigidbodyがkinematic状態（待機中）

        if (!GameStart.gameStarted || gameDirector.isJudging)
        {
            return false;
        }

        // 車のRigidbodyを取得して状態確認
        Transform car = GetCarTransform();
        if (car != null)
        {
            Rigidbody carRb = car.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                // kinematic状態かつ判定中でなければ発射待ち状態
                return carRb.isKinematic && !gameDirector.isJudging;
            }
        }

        return false;
    }

    // 車のTransformを取得（BowlingGameDirectorから）
    private Transform GetCarTransform()
    {
        // Reflectionを使って車の参照を取得
        var carField = typeof(BowlingGameDirector).GetField("car",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (carField != null)
        {
            return (Transform)carField.GetValue(gameDirector);
        }

        return null;
    }

    // 手動でUIの表示/非表示を切り替えるメソッド（デバッグ用）
    public void SetSpaceKeyGuideVisible(bool visible)
    {
        if (spaceKeyGuideUI != null)
        {
            spaceKeyGuideUI.SetActive(visible);
        }
    }
}