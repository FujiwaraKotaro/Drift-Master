using UnityEngine;
using UnityEngine.SceneManagement; // リトライ（シーン読み込み）に必要
using UnityEngine.UI;              // UI表示に必要
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("参照オブジェクト")]
    [SerializeField] private GameObject[] pins;    // 判定対象のピン全リスト
    [SerializeField] private Transform car;        // プレイヤーの車
    [SerializeField] private TextMeshProUGUI scoreText;       // 結果表示用のテキストUI

    [Header("ゲーム設定")]
    [SerializeField] private float finishLineZ = 50f;     // このZ座標を車が超えたら終了判定へ
    [SerializeField] private float waitTimeSeconds = 3f;  // ライン通過後、判定までの待ち時間
    [SerializeField] private float pinDownAngle = 45f;    // ピンが倒れたとみなす角度（度）

    private bool isGameFinished = false;

    // ★追加: static変数（シーンをリロードしても値が保持されます）
    private static int totalScore = 0;
    private static int throwCount = 0; // 何回投げたかもカウントしてみます

    void Start()
    {
        // ゲーム開始時に現在の累計スコアを少し表示（オプション）
        if (scoreText != null)
        {
            scoreText.text = $"Total: {totalScore}";
        }
    }

    void Update()
    {
        // 1. リトライ機能 (Rキー)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

        // ★追加: 完全リセット機能 (Spaceキー)
        // 累計スコアを0に戻してリロードします
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetTotalScore();
        }

        // ゲームが終わっていたらこれ以降の処理はしない
        if (isGameFinished) return;

        // 2. 終了判定トリガー (車が指定のラインを超えたら)
        if (car.position.z >= finishLineZ)
        {
            // ピンが散らばるのを少し待ってから判定を行う
            StartCoroutine(JudgeGameResult());
            isGameFinished = true;
        }
    }

    // 数秒待ってから判定するコルーチン
    private System.Collections.IEnumerator JudgeGameResult()
    {
        // ピンが物理演算で倒れきるのを待つ
        yield return new WaitForSeconds(waitTimeSeconds);

        int currentScore = CountFallenPins();
        ShowResult(currentScore);
    }

    // 倒れたピンを数えるロジック
    private int CountFallenPins()
    {
        int count = 0;
        foreach (GameObject pin in pins)
        {
            if (pin == null) continue; // 吹き飛んで消えた場合などの安全策

            // ピンの上方向(Y軸)と、ワールドの上方向の角度差を計算
            float angle = Vector3.Angle(pin.transform.up, Vector3.up);

            // 傾きが指定角度(45度)より大きければ「倒れた」とみなす
            // または、ピンがコース外に落ちてY座標が極端に低い場合も倒れたとみなす
            if (angle > pinDownAngle || pin.transform.position.y < -1f)
            {
                count++;
            }
        }
        return count;
    }

    // 結果表示
    private void ShowResult(int score)
    {
        // ★追加: 累計スコアに加算
        totalScore += score;
        throwCount++;

        string message = "";

        if (score == 10)
            message = "STRIKE!!";
        else if (score == 0)
            message = "Gutter...";
        else
            message = $"Score: {score}";

        // UIがあれば表示、なければログに出す
        if (scoreText != null)
        {
            // 今回のスコアと、累計スコアを両方表示します
            scoreText.text = $"{message}\nTotal: {totalScore} (Throw: {throwCount})\n\n[R] Retry  [Space] Reset All";
            scoreText.gameObject.SetActive(true);
        }

        Debug.Log($"ゲーム終了！ {message} (今回: {score} / 累計: {totalScore})");
    }

    // シーンのリロード
    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ★追加: 累計スコアをリセットしてリロード
    private void ResetTotalScore()
    {
        totalScore = 0;
        throwCount = 0;
        Debug.Log("スコアをリセットしました");
        ReloadScene();
    }
}