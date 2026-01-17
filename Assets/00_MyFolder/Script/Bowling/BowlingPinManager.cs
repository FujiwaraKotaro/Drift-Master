using System.Collections.Generic;
using UnityEngine;

// ピンの物理管理を行うクラス
// ・倒れたピンの判定
// ・ピンの配置リセット（全復活 or 残りピンのみ維持）
public class BowlingPinManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pins;    // シーン上のピン（初期設定用）
    [SerializeField] private float pinDownAngle = 45f; // 倒れたとみなす角度
    [SerializeField] private float maxDistanceFromOriginal = 3.0f; // 初期位置からの最大許容距離

    private string pinTag = "Pin"; // ピンを識別するためのタグ

    // ⭐ 各フレームのピン最大値を記録するリスト
    private List<int> frameMaxPinCounts = new List<int>();

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
        InitializePins();
        // 初回フレームのピン数を記録
        RecordCurrentFramePinCount();
    }

    // ピンの初期化処理
    private void InitializePins()
    {
        initialPinTransforms.Clear();

        foreach (var pin in pins)
        {
            if (pin != null)
            {
                initialPinTransforms.Add(new PinTransform(pin));
            }
        }
    }

    // PinTagがついているアクティブなオブジェクトを検索してpins配列を更新
    private void FindAndSetCurrentPins()
    {
        GameObject[] taggedPins = GameObject.FindGameObjectsWithTag(pinTag);

        // ⭐ pins配列を新しく設定
        List<GameObject> newPinsList = new List<GameObject>();

        foreach (GameObject pin in taggedPins)
        {
            if (pin.activeInHierarchy)
            {
                newPinsList.Add(pin);
            }
        }

        // pins配列を更新
        pins = newPinsList.ToArray();

        // initialPinTransformsも更新
        initialPinTransforms.Clear();
        foreach (var pin in pins)
        {
            if (pin != null)
            {
                initialPinTransforms.Add(new PinTransform(pin));
            }
        }

        Debug.Log($"新しいステージで{pins.Length}本のピンを検出しました");

        // ⭐ 各フレームのピン最大値をリストに記録
        RecordCurrentFramePinCount();
    }

    // ⭐ 現在のフレームのピン数を記録
    private void RecordCurrentFramePinCount()
    {
        frameMaxPinCounts.Add(pins.Length);
        Debug.Log($"フレーム{frameMaxPinCounts.Count}: 最大ピン数={pins.Length}を記録しました");
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
            if (i < initialPinTransforms.Count)
            {
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
        // 現在アクティブなステージのPinTagがついているオブジェクトを検索
        FindAndSetCurrentPins();

        // 検出されたピンを初期位置にリセット
        foreach (var pinData in initialPinTransforms)
        {
            GameObject p = pinData.gameObject;
            if (p != null)
            {
                p.SetActive(true);

                // 物理挙動を完全に止めてから位置を戻す（重要）
                Rigidbody rb = p.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = false;
                    rb.Sleep(); // 一旦スリープさせると安定する
                }

                p.transform.position = pinData.position;
                p.transform.rotation = pinData.rotation;
            }
        }

        Debug.Log($"ステージのピンをリセットしました: {pins.Length}本");
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
                    rb.isKinematic = false;
                    // 倒れていないピンは、微妙に動いていても元の回転に戻すと不自然なので、
                    // 速度ゼロにするだけにとどめるか、少しだけ補正する
                    // ここではシンプルに速度ゼロ化のみ
                }
            }
        }
    }

    // 現在のピン数を取得するメソッド（外部から参照用）
    // フレーム開始時の初期ピン数を返す
    public int GetCurrentPinCount()
    {
        return initialPinTransforms.Count;
    }

    public int GetCurrentActivePinCount()
    {
        int activeCount = 0;
        foreach (var pin in pins)
        {
            if (pin != null && pin.activeSelf)
            {
                activeCount++;
            }
        }
        return activeCount;
    }

    // 現在のピンリストを取得するメソッド（外部から参照用）
    public List<GameObject> GetCurrentPins()
    {
        return new List<GameObject>(pins);
    }

    // ⭐ 指定フレームの最大ピン数を取得
    public int GetFrameMaxPinCount(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frameMaxPinCounts.Count)
        {
            return frameMaxPinCounts[frameIndex];
        }
        // デフォルト値として現在のピン数を返す
        return initialPinTransforms.Count; 
    }
    // ⭐ ゲームリセット時にピン数履歴もクリア
    public void ResetFramePinCounts()
    {
        frameMaxPinCounts.Clear();
    }

    // ⭐ フレーム数を取得
    public int GetFrameCount()
    {
        return frameMaxPinCounts.Count;
    }
}