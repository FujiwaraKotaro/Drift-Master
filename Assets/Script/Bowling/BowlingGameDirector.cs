using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ゲーム全体の進行管理クラス
// Managerに状況判断を委譲し、返ってきた指示に従ってピンや車を操作する
public class BowlingGameDirector : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private BowlingScoreManager scoreManager;
    [SerializeField] private BowlingPinManager pinManager;

    [Header("Game Objects")]
    [SerializeField] private Transform car;
    [SerializeField] private Rigidbody carRb;

    [Header("Settings")]
    [SerializeField] private float finishLineZ = 50f;
    [SerializeField] private float waitTimeSeconds = 3f;

    private Vector3 carStartPos;
    private Quaternion carStartRot;
    private bool isJudging = false;

    void Start()
    {
        carStartPos = car.position;
        carStartRot = car.rotation;
    }

    void Update()
    {
        // ゲーム終了時リセット
        var status = scoreManager.CheckGameStatus();
        if (status.IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // 判定ライン通過チェック
        if (!isJudging && car.position.z >= finishLineZ)
        {
            StartCoroutine(ProcessThrowResult());
        }
    }

    private IEnumerator ProcessThrowResult()
    {
        isJudging = true;

        // 1. ピンが落ち着くのを待つ
        yield return new WaitForSeconds(waitTimeSeconds);

        // 2. 倒れたピンを集計して記録
        List<GameObject> fallenPins = pinManager.CheckFallenPins();
        int fallenCount = fallenPins.Count;

        Debug.Log($"倒れたピン: {fallenCount}本");
        scoreManager.RecordThrow(fallenCount); // 記録＆UI更新

        // 3. Managerに「次どうすればいい？」と聞く (ここが重要)
        var status = scoreManager.CheckGameStatus();

        if (status.IsGameOver)
        {
            Debug.Log("Game Over! Press R to Restart.");
        }
        else
        {
            // 次の投球に向けたセットアップ
            switch (status.NextAction)
            {
                case BowlingScoreManager.NextPinAction.ResetAll:
                    pinManager.ResetAllPins();
                    Debug.Log("Reset All Pins");
                    break;

                case BowlingScoreManager.NextPinAction.RemoveFallen:
                    pinManager.RemovePins(fallenPins);
                    pinManager.StabilizeStandingPins();
                    Debug.Log("Remove Fallen Pins");
                    break;
            }
        }

        // 4. 車をリセット
        ResetCar();

        isJudging = false;
    }

    private void ResetCar()
    {
        carRb.velocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;
        car.position = carStartPos;
        car.rotation = carStartRot;
    }
}