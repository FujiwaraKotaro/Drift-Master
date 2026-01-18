using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GetItem : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    [SerializeField] private float initialForceStrength = 30000f; // 質量1500に適した力
    [SerializeField] private float sizeMultiplier = 1.2f; //アイテム取得時のサイズUP倍率

    public int number_getItem = 1;

    [Header("Camera Adjustment Settings")]
    [SerializeField] private CameraScript cameraScript; // カメラスクリプトへの参照
    [SerializeField] private float cameraAdjustmentMultiplier = 2.0f; // カメラ調整倍率（可変）
    private int itemsPerCameraAdjustment = 5; // 5つごとに調整

    [Header("Score UI Settings")]
    [SerializeField] private GameObject scoreUIPrefab;   // スコアUIのプレハブ
    [SerializeField] private Transform spawnParent;      // 生成する親（Canvasなど）
    [SerializeField] private float moveDistance = 100f; // 上に移動する距離
    [SerializeField] private float duration = 1.5f;     // アニメーション時間

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            // アイテム取得時も車の前方向に力を加える
            m_Rigidbody.AddForce(transform.forward * initialForceStrength, ForceMode.Impulse);
            number_getItem++;

            // 現在のサイズと位置を保存
            Vector3 currentScale = transform.localScale;
            Vector3 currentPosition = transform.position;

            // 車のサイズを更新
            transform.localScale = currentScale * sizeMultiplier;

            // サイズ増加分だけY座標を上に移動（地面に埋まらないように）
            float sizeIncrease = (sizeMultiplier - 1.0f) * currentScale.y;
            transform.position = new Vector3(currentPosition.x, currentPosition.y + sizeIncrease, currentPosition.z);

            SoundManager.Instance.PlaySE("SpeedUpSE");
            // スコアUI演出を開始
            ShowScoreUI();

            // 5個ごとにカメラのオフセットを調整
            if (number_getItem % itemsPerCameraAdjustment == 0)
            {
                if (cameraScript != null)
                {
                    cameraScript.UpdateCameraOffset(cameraAdjustmentMultiplier);
                }
            }

            // ぶつかったアイテムを1秒間非アクティブにする
            StartCoroutine(HideItemTemporarily(other.gameObject));
        }
    }

    // アイテムを一時的に非表示にするコルーチン
    private IEnumerator HideItemTemporarily(GameObject item)
    {
        item.SetActive(false);
        yield return new WaitForSeconds(1f);
        item.SetActive(true);
    }

    private void ShowScoreUI()
    {
        // プレハブを生成
        GameObject scoreUIInstance = Instantiate(scoreUIPrefab, spawnParent);

        // 子オブジェクトからコンポーネントを取得
        TMP_Text scoreText = scoreUIInstance.GetComponentInChildren<TMP_Text>();
        CanvasGroup canvasGroup = scoreUIInstance.GetComponent<CanvasGroup>();

        // TMPテキストにスコアを設定
        scoreText.text = "スピードUP\nサイズUP";

        // 初期状態を設定
        canvasGroup.alpha = 1f;

        // 開始位置を設定（必要に応じて調整）
        Vector3 startPosition = scoreUIInstance.transform.position;

        // アニメーション実行
        // 1. 上に移動
        scoreUIInstance.transform.DOMoveY(startPosition.y + moveDistance, duration);

        // 2. フェードアウト
        canvasGroup.DOFade(0f, duration);

        // 3. アニメーション完了後にデストロイ
        DOVirtual.DelayedCall(duration, () =>
        {
            if (scoreUIInstance != null)
            {
                Destroy(scoreUIInstance);
            }
        });
    }
}