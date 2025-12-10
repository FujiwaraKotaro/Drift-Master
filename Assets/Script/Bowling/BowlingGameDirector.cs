using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲーム全体の進行管理クラス (旧 GameManager)
// 車、スコア管理、ピン管理を統括する「監督」の役割
public class BowlingGameDirector : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private BowlingScoreManager scoreManager; // スコア計算担当
    [SerializeField] private BowlingPinManager pinManager;     // ピン管理担当

    [Header("Game Objects")]
    [SerializeField] private Transform car;       // プレイヤーの車
    [SerializeField] private Rigidbody carRb;     // 車の物理ボディ

    [Header("Settings")]
    [SerializeField] private float finishLineZ = 50f;     // 判定ライン
    [SerializeField] private float waitTimeSeconds = 3f;  // 衝突後の待ち時間

    // 車の初期位置記憶用
    private Vector3 carStartPos;
    private Quaternion carStartRot;

    private bool isJudging = false; // 判定中フラグ

    void Start()
    {
        // 車のスタート位置を覚えておく
        carStartPos = car.position;
        carStartRot = car.rotation;
    }

    void Update()
    {
        // ゲーム終了時ならRキーで完全リセット（シーンリロード）
        if (scoreManager.IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // 判定中でなく、車がラインを超えたら判定プロセス開始
        if (!isJudging && car.position.z >= finishLineZ)
        {
            StartCoroutine(ProcessThrowResult());
        }
    }

    // 投球結果の処理フロー
    private IEnumerator ProcessThrowResult()
    {
        isJudging = true;

        // 1. ピンが倒れきるのを待つ
        yield return new WaitForSeconds(waitTimeSeconds);

        // 2. ピンマネージャーに倒れたピンを報告させる
        List<GameObject> fallenPins = pinManager.CheckFallenPins();
        int fallenCount = fallenPins.Count;

        Debug.Log($"倒れたピン: {fallenCount}本");

        // 3. スコアマネージャーに点数を記録
        scoreManager.RecordThrow(fallenCount);

        // 4. 次のターンの準備状況を確認（1投目か2投目か、ストライクかなど）
        int previousFrame = scoreManager.CurrentFrame;
        int previousThrow = scoreManager.CurrentThrow;

        // スコアマネージャーの状態を進める
        scoreManager.AdvanceTurn(fallenCount);

        if (scoreManager.IsGameOver)
        {
            Debug.Log("ゲーム終了！ Rキーでリトライ");
        }
        else
        {
            // まだゲームが続く場合、次の投球のセットアップを行う
            SetupNextThrow(fallenPins, fallenCount);
        }

        // 5. 車をスタート地点に戻す
        ResetCar();

        isJudging = false;
    }

    // 次の投球のための配置セットアップ
    private void SetupNextThrow(List<GameObject> fallenPins, int countLastThrow)
    {
        // ルール:
        // 現在が「1投目」になるなら -> 新しいフレームなので全ピン復活
        // 現在が「2投目」になるなら -> 倒れたピンだけ除去して、残りはそのまま

        if (scoreManager.CurrentThrow == 1)
        {
            // 次が1投目ということは、フレームが変わった（あるいは前の投球でストライクだった）
            // -> 全ピンをリセットして配置しなおす
            pinManager.ResetAllPins();
            Debug.Log("Next Frame: Reset All Pins");
        }
        else
        {
            // 次が2投目（または10フレーム目の3投目など）
            // -> さっき倒れたピンを除去する
            pinManager.RemovePins(fallenPins);

            // 残っているピンの揺れを止める
            pinManager.StabilizeStandingPins();

            Debug.Log("Next Throw: Remove Fallen Pins Only");
        }
    }

    // 車をリセットする処理
    private void ResetCar()
    {
        carRb.velocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;
        car.position = carStartPos;
        car.rotation = carStartRot;
    }
}