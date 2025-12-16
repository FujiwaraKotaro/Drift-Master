using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("車の設定")]
    [Tooltip("最高速度")]
    public float maxSpeed = 30f;
    [Tooltip("加速力")]
    public float acceleration = 50f;
    [Tooltip("旋回性能（高いほど急に曲がる）")]
    public float turnSpeed = 100f;

    [Header("重心調整")]
    [Tooltip("転倒防止のため重心を下げるオフセット値")]
    public float centerOfMassOffset = -1.0f;

    public LayerMask groundLayer;
    public float rayLength = 1.2f;
    private bool isGrounded;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 重心を強制的に下げて転倒しにくくする
        rb.centerOfMass = new Vector3(0, centerOfMassOffset, 0);
    }

    void Update()
    {
        // 入力の取得（自動進行なのでアクセルは常に1、ブレーキなし）
        // 必要なら Input.GetAxis("Vertical") を足してください
        moveInput = 1.0f;
        turnInput = Input.GetAxis("Horizontal"); // A/D または 矢印左右
    }

    void FixedUpdate()
    {
        if (CheckGround()) // 【追加】もし地面についていたら...
        {
            Move();      // 走る
            Turn();      // 曲がる
            ApplyGrip(); // グリップする
        }
    }

    // 1. 自動前進
    void Move()
    {
        // 最高速度以下なら加速力を加える
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
        }
    }

    // 2. 旋回（物理演算ではなく回転を直接操作）
    void Turn()
    {
        // 停止中は回らないようにする（少し動いていれば回れる）
        if (rb.velocity.magnitude > 0.1f)
        {
            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    // 3. 完全グリップ処理（横滑りを完全に消去）
    void ApplyGrip()
    {
        // 現在の速度ベクトルを「進行方向成分」と「横方向成分」に分解
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        // 横方向の速度（横滑り）を完全に消去
        localVelocity.x = 0f;

        // 補正した速度をワールド座標に戻して適用
        rb.velocity = transform.TransformDirection(localVelocity);
    }

    // 見えないレーザーを下に飛ばして地面を探す機能
    private bool CheckGround()
    {
        // 自分の位置から、下方向(-transform.up)に、rayLengthの長さだけ線を飛ばす
        return  Physics.Raycast(transform.position, -transform.up, rayLength, groundLayer);
    }

    // デバッグ用：シーンビューで接地判定のレーザーを可視化
    void OnDrawGizmos()
    {
        // 接地していれば赤、浮いていれば緑の線を表示
        Gizmos.color = isGrounded ? Color.red : Color.green;
        // 車の中心から下へ線を引く
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * rayLength));
    }
}