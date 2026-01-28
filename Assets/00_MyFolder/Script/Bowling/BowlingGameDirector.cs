using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using unityroom.Api;

// ゲーム全体の進行管理クラス
// Managerに状況を判断させ、返ってくる指示に従ってピンを操作する
public class BowlingGameDirector : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private BowlingScoreManager scoreManager;
    [SerializeField] private BowlingPinManager pinManager;

    [Header("Game Objects")]
    [SerializeField] private Transform car;
    [SerializeField] private Rigidbody carRb;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CameraScript cameraScript; // カメラスクリプトへの参照
    [SerializeField] private float waitTimeSeconds = 3f;

    [Header("スペースキーガイドUI")]
    [SerializeField] private GameObject SpaceKeyGuideUI;

    private Vector3 carStartPos;
    private Quaternion carStartRot;

    // ステージ管理
    [SerializeField] private List<GameObject> stages = new List<GameObject>();
    // SubCamera管理（各ステージに対応するSubCamera）
    [SerializeField] private List<GameObject> subCameras = new List<GameObject>();
    private int currentStageIndex = 0;

    // 状態管理用のフラグ
    public bool isJudging = false;      // 判定中かどうか
    private bool isReadyToThrow = false; // 発射待ちかどうか

    void Start()
    {
        carStartPos = car.position;
        carStartRot = car.rotation;

        // ゲーム開始時のセットアップを行う（車を止めて発射待ちにする）
        ResetCar();
        SpaceKeyGuideUI.SetActive(true);

        // ゲーム開始前はすべてのSubCameraをオフにする（GameStart時にオンになる）
        DeactivateAllSubCameras();
    }

    /// <summary>
    /// SubCameraの初期化：最初のステージに対応するSubCameraのみアクティブにする
    /// </summary>
    private void InitializeSubCameras()
    {
        for (int i = 0; i < subCameras.Count; i++)
        {
            if (subCameras[i] != null)
            {
                subCameras[i].SetActive(i == currentStageIndex);
            }
        }
    }

    /// <summary>
    /// ゲームスタート時に呼ばれる：現在のSubCameraをアクティブにする
    /// </summary>
    public void ActivateCurrentSubCamera()
    {
        if (subCameras.Count > 0 && subCameras[currentStageIndex] != null)
        {
            subCameras[currentStageIndex].SetActive(true);
        }
    }

    /// <summary>
    /// すべてのSubCameraを非アクティブにする
    /// </summary>
    public void DeactivateAllSubCameras()
    {
        foreach (var cam in subCameras)
        {
            if (cam != null)
            {
                cam.SetActive(false);
            }
        }
    }

    void Update()
    {
        // ゲームオーバーチェック
        var status = scoreManager.CheckGameStatus();
        
        // --- 発射待ちの状態 ---
        if (isReadyToThrow)
        {
            // スペースキーが押されたら発射
            if (Input.GetKeyDown(KeyCode.Space) && GameStart.gameStarted)
            {
                ShootCar();
            }
            // 発射待ちの間は、以降の処理（ゴールなど）を行わせない
            return;
        }
    }

    private void ShootCar()
    {
        isReadyToThrow = false; // 待機状態解除
        carRb.isKinematic = false; // 物理演算を有効にする（車が動き出す/重力が効く）

        SpaceKeyGuideUI.SetActive(false);
    }

    public IEnumerator ProcessThrowResult()
    {
        isJudging = true;

        // 1. ピンの安定を待つ
        yield return new WaitForSeconds(waitTimeSeconds);

        // 2. 倒れたピンを集計して記録
        List<GameObject> fallenPins = pinManager.CheckFallenPins();
        int fallenCount = fallenPins.Count;

        Debug.Log($"倒れたピン数: {fallenCount}本");
        scoreManager.RecordThrow(fallenCount); // 記録とUI更新

        // 3. Managerに「どうすればいい？」と問い合わせる (重要)
        var status = scoreManager.CheckGameStatus();

        if (status.IsGameOver)
        {
            Debug.Log("Game Over! Press R to Restart.");
            // ゲームオーバー時はSubCameraをオフにする
            DeactivateAllSubCameras();

            // unityroomにスコアを送信
            int[] cumulativeScores = scoreManager.GetCumulativeScores();
            int totalScore = cumulativeScores[cumulativeScores.Length - 1];
            if (totalScore >= 0)
            {
                UnityroomApiClient.Instance.SendScore(1, totalScore, ScoreboardWriteMode.HighScoreDesc);
                Debug.Log($"unityroomにスコア送信: {totalScore}");
            }

            FindObjectOfType<resultUIManager>().ShowResultUI();
        }
        else
        {
            // 次の投球に向けたセットアップ
            switch (status.NextAction)
            {
                case BowlingScoreManager.NextPinAction.ResetAll:
                    // 次のステージに進める
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

            // カメラのオフセットをリセット
            if (cameraScript != null)
            {
                cameraScript.ResetCameraOffset();
            };

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
        // 最後のステージの場合は何もしない
        if(currentStageIndex == stages.Count - 1) return;
        // 現在のステージを非アクティブにする
        stages[currentStageIndex].SetActive(false);

        // 現在のSubCameraを非アクティブにする
        if (subCameras.Count > currentStageIndex && subCameras[currentStageIndex] != null)
        {
            subCameras[currentStageIndex].SetActive(false);
        }

        // 次のステージに進む
        currentStageIndex = currentStageIndex + 1;
        

        // 新しいステージをアクティブにする
        stages[currentStageIndex].SetActive(true);

        // 新しいSubCameraをアクティブにする
        if (subCameras.Count > currentStageIndex && subCameras[currentStageIndex] != null)
        {
            subCameras[currentStageIndex].SetActive(true);
        }

        Debug.Log($"ステージ変更: Stage{currentStageIndex + 1}がアクティブになりました");
    }

    private void ResetCar()
    {
        // 位置を戻す
        car.position = carStartPos;
        car.rotation = carStartRot;

        // 完全に止める
        carRb.velocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        // サイズを元に戻す
        car.transform.localScale = Vector3.one;

        // KinematicをONにして、物理的に「固定」状態にする
        carRb.isKinematic = true;

        // 発射待ちフラグを立てる
        isReadyToThrow = true;
    }
}