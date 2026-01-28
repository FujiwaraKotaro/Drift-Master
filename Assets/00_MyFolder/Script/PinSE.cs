using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSE : MonoBehaviour
{
    // クールダウン時間（秒） - すべてのインスタンスで共有
    private static float cooldownTime = 3f;
    // 音を鳴らせるかどうかのフラグ - すべてのインスタンスで共有
    private static bool canPlaySound = true;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            // 1フレーム待機してからチェック（BigPinのkinematic設定を待つ）
            StartCoroutine(CheckAndPlaySE());
        }
    }

    // 1フレーム待機してからkinematicチェックと音の再生を行う
    private IEnumerator CheckAndPlaySE()
    {
        // 1フレーム待機
        yield return null;

        // このピンがkinematicの場合は音を鳴らさない
        if (rb != null && rb.isKinematic)
        {
            yield break;
        }

        // フラグがtrueの場合のみ音を鳴らす
        if (canPlaySound)
        {
            SoundManager.Instance.PlaySE("PinSE");
            StartCoroutine(CooldownCoroutine());
        }
    }

    // オブジェクトが非アクティブになる時の処理
    private void OnDisable()
    {
        // コルーチンが途中で停止する場合に備えてフラグをリセット
        canPlaySound = true;
    }

    // クールダウン用コルーチン
    private IEnumerator CooldownCoroutine()
    {
        canPlaySound = false;
        yield return new WaitForSeconds(cooldownTime);
        canPlaySound = true;
    }
}