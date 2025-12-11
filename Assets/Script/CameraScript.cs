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
    public float distance = 6.0f;     // プレイヤーの後ろ何メートルか（少し離した方が見やすいかも）
    public float height = 3.0f;       // プレイヤーの上何メートルか（少し高くした方が見やすいかも）
    public float smoothSpeed = 5.0f;  // 追従の滑らかさ

    [Header("Screen Position Adjustment")]
    [Tooltip("値を大きくするほど、車が画面の下の方に表示されます")]
    public float lookAtOffsetHeight = 2.0f; // ★ここを追加！視点を車の中心からどれだけ上ずらすか

    void FixedUpdate()
    {
        if (player == null　|| !GameStart.gameStarted) return;

        // --- 1. カメラの位置計算 (以前と同じ) ---
        float targetX = player.transform.position.x;
        float targetY = player.transform.position.y + height;
        float targetZ = player.transform.position.z - distance;

        Vector3 currentPos = transform.position;

        // フラグがONの軸だけ更新
        Vector3 nextPos = new Vector3(
            trackX ? targetX : currentPos.x,
            trackY ? targetY : currentPos.y,
            trackZ ? targetZ : currentPos.z
        );

        // 滑らかに移動
        transform.position = Vector3.Lerp(transform.position, nextPos, smoothSpeed * Time.deltaTime);


        // --- 2. カメラの向き計算 (★ここを変更) ---

        // プレイヤーの現在位置から、少し上の点をターゲットにする
        Vector3 lookTarget = player.transform.position + Vector3.up * lookAtOffsetHeight;

        // そのターゲットの方向を向く
        transform.LookAt(lookTarget);
    }
}