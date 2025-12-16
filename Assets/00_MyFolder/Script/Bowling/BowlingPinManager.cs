using System.Collections.Generic;
using UnityEngine;

// ピンの物理管理を行うクラス
// ・倒れたピンの判定
// ・ピンの配置リセット（全復活 or 残りピンのみ維持）
public class BowlingPinManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pins;    // シーン上のピン全10本
    [SerializeField] private float pinDownAngle = 45f; // 倒れたとみなす角度
    [SerializeField] private float maxDistanceFromOriginal = 3.0f; // 初期位置からの最大許容距離

    // ピンの初期位置と回転を記憶するための構造体
    private struct PinTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public GameObject gameObject;

        public PinTransform(GameObject obj)
        {
            gameObject = obj;
            position = obj.transform.position;
            rotation = obj.transform.rotation;
        }
    }

    private List<PinTransform> initialPinTransforms = new List<PinTransform>();

    void Start()
    {
        // ゲーム開始時に全ピンの初期位置を記憶する
        foreach (var pin in pins)
        {
            initialPinTransforms.Add(new PinTransform(pin));
        }
    }

    // 倒れたピンの数を数え、倒れたピンのリストを返す
    public List<GameObject> CheckFallenPins()
    {
        List<GameObject> fallenPins = new List<GameObject>();

        for (int i = 0; i < pins.Length; i++)
        {
            var pin = pins[i];

            // 非アクティブ（すでに除去された）ピンは無視
            if (pin == null || !pin.activeSelf) continue;

            float angle = Vector3.Angle(pin.transform.up, Vector3.up);

            // 初期位置からの距離を計算
            Vector3 originalPosition = initialPinTransforms[i].position;
            float distanceFromOriginal = Vector3.Distance(pin.transform.position, originalPosition);

            // 倒れた判定の条件
            // 1. 傾きが大きい
            // 2. コース外（Y座標が低い）
            // 3. 初期位置から一定距離以上離れた
            if (angle > pinDownAngle ||
                pin.transform.position.y < -0.5f ||
                distanceFromOriginal > maxDistanceFromOriginal)
            {
                fallenPins.Add(pin);
            }
        }
        return fallenPins;
    }

    // 指定されたリストのピンを非表示にする（1投目の後の処理など）
    public void RemovePins(List<GameObject> pinsToRemove)
    {
        foreach (var pin in pinsToRemove)
        {
            pin.SetActive(false);
        }
    }

    // 全てのピンを初期位置に戻して復活させる（新しいフレームの開始時）
    public void ResetAllPins()
    {
        foreach (var pinData in initialPinTransforms)
        {
            GameObject p = pinData.gameObject;
            p.SetActive(true);

            // 物理挙動を完全に止めてから位置を戻す（重要）
            Rigidbody rb = p.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep(); // 一旦スリープさせると安定する


            p.transform.position = pinData.position;
            p.transform.rotation = pinData.rotation;
        }
    }

    // 現在残っているピンの物理挙動だけリセット（位置はずらさない）
    // 2投目の前に、揺れているピンを静止させるために使用
    public void StabilizeStandingPins()
    {
        foreach (var pin in pins)
        {
            if (pin != null && pin.activeSelf)
            {
                Rigidbody rb = pin.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    // 倒れていないピンは、微妙に動いていても元の回転に戻すと不自然なので、
                    // 速度ゼロにするだけにとどめるか、少しだけ補正する
                    // ここではシンプルに速度ゼロ化のみ
                }
            }
        }
    }
}