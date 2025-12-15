using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] GameObject player;

    [Header("Tracking Settings")]
    public bool trackX = true; // X軸(左右)を追従するか
    // ボウリングの場合、trackYはオフ(false)がおすすめです
    public bool trackY = false; // Y軸(上下)を追従するか 
    public bool trackZ = true; // Z軸(奥行)を追従するか

    [Header("Position Offset")]
    public float distance = 6.0f;     // プレイヤーの後ろ何メートルか
    public float height = 3.0f;       // プレイヤーの上何メートルか
    public float smoothSpeed = 5.0f;  // 追従の滑らかさ

    [Header("Screen Position Adjustment")]
    [Tooltip("値を大きくするほど、車が画面の下の方に表示されます")]
    public float lookAtOffsetHeight = 2.0f; // 視点を車の中心からどれだけ上ずらすか

    void FixedUpdate()
    {
        if (player == null || !GameStart.gameStarted) return;

        // --- 1. 車の向きに基づいてカメラの位置を計算 ---
        // 車の後ろ方向 = 車の前方向の逆
        Vector3 carBackDirection = -player.transform.forward;

        // 車の位置から後ろ方向にdistance分離れた位置
        Vector3 targetPosition = player.transform.position + carBackDirection * distance + Vector3.up * height;

        Vector3 currentPos = transform.position;

        // フラグがONの軸だけ更新
        Vector3 nextPos = new Vector3(
            trackX ? targetPosition.x : currentPos.x,
            trackY ? targetPosition.y : currentPos.y,
            trackZ ? targetPosition.z : currentPos.z
        );

        // 滑らかに移動
        transform.position = Vector3.Lerp(transform.position, nextPos, smoothSpeed * Time.deltaTime);

        // --- 2. カメラの向き計算（車の方向を見る） ---
        // プレイヤーの現在位置から、少し上の点をターゲットにする
        Vector3 lookTarget = player.transform.position + Vector3.up * lookAtOffsetHeight;

        // そのターゲットの方向を向く
        transform.LookAt(lookTarget);
    }
}