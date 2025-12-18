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

    [Header("Wall衝突設定")]
    [Tooltip("Wall衝突時の回転速度")]
    public float wallAlignmentSpeed = 5f;

    public LayerMask groundLayer;
    public float rayLength = 1.2f;
    private bool isGrounded;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    private bool BGMflag = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 重心を強制的に下げて転倒しにくくする
        rb.centerOfMass = new Vector3(0, centerOfMassOffset, 0);
    }

    void Update()
    {
        if (CheckGround() && BGMflag)
        {
            SoundManager.Instance.PlayBGM("EnginBGM");
            BGMflag = false;
        }

        if (!CheckGround())
        {
            SoundManager.Instance.StopBGM();
            BGMflag = true;
        }
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
        return Physics.Raycast(transform.position, -transform.up, rayLength, groundLayer);
    }

    // Wall衝突時の処理
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AlignWithWall(other);
        }
    }

    private void AlignWithWall(Collider wallCollider)
    {
        // 現在の速度を保存
        Vector3 currentVelocity = rb.velocity;
        float currentSpeed = currentVelocity.magnitude;

        // 車からWallオブジェクトへの方向を計算
        Vector3 directionToWall = (wallCollider.transform.position - transform.position).normalized;

        // Wallオブジェクトの向きを基にした並行方向を計算
        // Wallオブジェクトの前方向またはright方向を使用
        Vector3 wallDirection = wallCollider.transform.forward;

        // 現在の進行方向と壁の方向の内積で、どちら向きが適切かを判定
        if (Vector3.Dot(currentVelocity.normalized, wallDirection) < 0)
        {
            wallDirection = -wallDirection;
        }

        // より適切な方向を選択するため、wallのright方向も考慮
        Vector3 wallRightDirection = wallCollider.transform.right;
        if (Vector3.Dot(currentVelocity.normalized, wallRightDirection) > Vector3.Dot(currentVelocity.normalized, wallDirection))
        {
            wallDirection = wallRightDirection;
        }
        else if (Vector3.Dot(currentVelocity.normalized, -wallRightDirection) > Vector3.Dot(currentVelocity.normalized, wallDirection))
        {
            wallDirection = -wallRightDirection;
        }

        // 壁と並行になる回転を計算
        Quaternion targetRotation = Quaternion.LookRotation(wallDirection);

        // 即座に回転を適用（滑らかにしたい場合はLerpを使用）
        transform.rotation = targetRotation;

        // 速度を壁と並行な方向に向け直し、速度は維持
        rb.velocity = wallDirection * currentSpeed;

        Debug.Log($"Wall trigger detected! Aligned with wall direction: {wallDirection}");
    }
}