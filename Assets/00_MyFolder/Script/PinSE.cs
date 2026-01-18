using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSE : MonoBehaviour
{
    // クールダウン時間（秒） - すべてのインスタンスで共有
    private static float cooldownTime = 3f;
    // 音を鳴らせるかどうかのフラグ - すべてのインスタンスで共有
    private static bool canPlaySound = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            // フラグがtrueの場合のみ音を鳴らす
            if (canPlaySound)
            {
                SoundManager.Instance.PlaySE("PinSE");
                StartCoroutine(CooldownCoroutine());
            }
        }
    }

    // オブジェクトが非アクティブになった時の処理
    private void OnDisable()
    {
        // コルーチンが途中で停止した場合に備えてフラグをリセット
        canPlaySound = true;
    }

    // クールダウンコルーチン
    private IEnumerator CooldownCoroutine()
    {
        canPlaySound = false;
        yield return new WaitForSeconds(cooldownTime);
        canPlaySound = true;
    }
}
