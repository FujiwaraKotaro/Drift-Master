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
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float waitTimeSeconds = 3f;

    [Header("スペースキーガイドUI")]
    [SerializeField] private GameObject SpaceKeyGuideUI;

    private Vector3 carStartPos;
    private Quaternion carStartRot;

    // ステージ管理
    [SerializeField] private List<GameObject> stages = new List<GameObject>();
    private int currentStageIndex = 0;

    // 状態管理用のフラグ
    public bool isJudging = false;      // 判定中かどうか
    private bool isReadyToThrow = false; // 発射待ちかどうか

    void Start()
    {
        carStartPos = car.position;
        carStartRot = car.rotation;

        // ゲーム開始時もセットアップを行う（物理を止めて発射待ちにする）
        ResetCar();
        SpaceKeyGuideUI.SetActive(true);
    }

    void Update()
    {
        // ゲーム終了時リセット
        var status = scoreManager.CheckGameStatus();
        if (status.IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameStart.gameStarted = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // --- 発射待ちの処理 ---
        if (isReadyToThrow)
        {
            // スペースキーが押されたら発射
            if (Input.GetKeyDown(KeyCode.Space) && GameStart.gameStarted)
            {
                ShootCar();
            }
            // 発射待ちの間は、これ以降の処理（ゴール判定など）を行わせない
            return;
        }
    }

    private void ShootCar()
    {
        isReadyToThrow = false; // 待機状態解除
        carRb.isKinematic = false; // 物理演算をオンにする（車が動き出す/重力が効く）

        SpaceKeyGuideUI.SetActive(false);
    }

    public IEnumerator ProcessThrowResult()
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
            FindObjectOfType<resultUIManager>().ShowResultUI();
        }
        else
        {
            // 次の投球に向けたセットアップ
            switch (status.NextAction)
            {
                case BowlingScoreManager.NextPinAction.ResetAll:
                    // ステージを次に進める
                    ChangeToNextStage();

                    pinManager.ResetAllPins();
                    Debug.Log("Reset All Pins");
                    break;

                case BowlingScoreManager.NextPinAction.RemoveFallen:
                    pinManager.RemovePins(fallenPins);
                    pinManager.StabilizeStandingPins();
                    Debug.Log("Remove Fallen Pins");
                    break;
            }

            // 4. 車をリセット
            ResetCar();

            // 5. 車の近くにカメラを移動
            mainCamera.position = new Vector3(-245f, 7.25f, -466f);
            mainCamera.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        FindObjectOfType<GetItem>().number_getItem = 1;
        isJudging = false;
    }

    private void ChangeToNextStage()
    {
        if (stages.Count == 0) return;

        // 現在のステージを非アクティブにする
        stages[currentStageIndex].SetActive(false);

        // 次のステージに進む（最後のステージの場合は最初に戻る）
        currentStageIndex = (currentStageIndex + 1) % stages.Count;

        // 新しいステージをアクティブにする
        stages[currentStageIndex].SetActive(true);

        Debug.Log($"ステージを変更: Stage{currentStageIndex + 1}がアクティブになりました");
    }

    private void ResetCar()
    {
        // 位置を戻す
        car.position = carStartPos;
        car.rotation = carStartRot;

        // 物理挙動を完全に止める
        carRb.velocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        //サイズをもとに戻す
        car.transform.localScale = Vector3.one;

        // KinematicをONにして、物理的に「固定」する ---
        carRb.isKinematic = true;

        // 発射待ちフラグを立てる
        isReadyToThrow = true;
    }
}