using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] GameObject player;

    [Header("Tracking Settings")]
    public bool trackX = true; // X軸(左右)を追従するか
    // ボウリングの場合、trackYはオフ(false)がおすすめです
    public bool trackY = false; // Y軸(上下)を追従するか 
    public bool trackZ = true; // Z軸(前後)を追従するか

    [Header("Position Offset")]
    public float distance = 6.0f;     // プレイヤーの後ろ何メートル
    public float height = 3.0f;       // プレイヤーの上何メートル
    public float smoothSpeed = 5.0f;  // 追従の滑らかさ

    [Header("Screen Position Adjustment")]
    [Tooltip("値が大きいほど、車が画面の下の方に表示される")]
    public float lookAtOffsetHeight = 2.0f; // 注視点の中心をどれだけ上げるか

    // カメラオフセット管理用の変数
    private float initialDistance;
    private float initialHeight;
    private float cumulativeScaleMultiplier = 1.0f;

    void Start()
    {
        // 初期値を保存
        initialDistance = distance;
        initialHeight = height;
    }

    void FixedUpdate()
    {
        if (GameStart.gameStarted == false) return;
        if (player == null || !GameStart.gameStarted) return;

        // --- 1. 車の向きに基づいてカメラの位置を計算 ---
        // 車の後ろ = 車の前方の逆
        Vector3 carBackDirection = -player.transform.forward;

        // 車の位置からdistance離れた位置
        Vector3 targetPosition = player.transform.position + carBackDirection * distance + Vector3.up * height;

        Vector3 currentPos = transform.position;

        // フラグがONの軸のみ更新
        Vector3 nextPos = new Vector3(
            trackX ? targetPosition.x : currentPos.x,
            trackY ? targetPosition.y : currentPos.y,
            trackZ ? targetPosition.z : currentPos.z
        );

        // 滑らかに移動
        transform.position = Vector3.Lerp(transform.position, nextPos, smoothSpeed * Time.deltaTime);

        // --- 2. カメラの向きを計算（車の方を向く） ---
        // プレイヤーの現在位置を、注視点のターゲットにする
        Vector3 lookTarget = player.transform.position + Vector3.up * lookAtOffsetHeight;

        // その方向のターゲットを向く
        transform.LookAt(lookTarget);
    }

    /// <summary>
    /// カメラオフセットを更新（アイテムを5つ取得するごとに呼ばれる）
    /// </summary>
    /// <param name="multiplier">カメラ倍率</param>
    public void UpdateCameraOffset(float multiplier)
    {
        cumulativeScaleMultiplier *= multiplier;
        distance = initialDistance * cumulativeScaleMultiplier;
        height = initialHeight * cumulativeScaleMultiplier;
    }

    /// <summary>
    /// カメラオフセットを初期値にリセット（ゲームリセット時に呼ばれる）
    /// </summary>
    public void ResetCameraOffset()
    {
        cumulativeScaleMultiplier = 1.0f;
        distance = initialDistance;
        height = initialHeight;
    }
}