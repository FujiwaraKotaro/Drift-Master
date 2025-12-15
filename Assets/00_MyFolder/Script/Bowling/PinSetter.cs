using UnityEngine;

public class PinSetter : MonoBehaviour
{
    [Tooltip("ピンのPrefab")]
    public GameObject pinPrefab;

    [Tooltip("ピン同士の間隔（メートル） 公式は12インチ≒0.3048m")]
    public float spacing = 0.3048f;

    [ContextMenu("Generate Pins")] // コンテキストメニューから実行可能にする
    public void GeneratePins()
    {
        // 既存の子オブジェクトがあれば削除（重複防止）
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        int rows = 4; // ボウリングは通常4列

        for (int row = 0; row < rows; row++)
        {
            // 各行のピンの数は (row + 1) 個
            for (int col = 0; col <= row; col++)
            {
                // Z座標: 行数 × 正三角形の高さ
                float zPos = row * spacing * Mathf.Sqrt(3) / 2;

                // X座標: 列インデックス - (その行の幅の半分) でセンタリング
                // 行の幅は row * spacing なので、その半分 (row * 0.5f) を引く
                float xPos = (col - (row * 0.5f)) * spacing;

                Vector3 position = new Vector3(xPos, 0, zPos);

                // 生成 (親をこのオブジェクトにする)
                GameObject pin = Instantiate(pinPrefab, transform.position + position, Quaternion.identity, transform);
                pin.name = $"Pin_{row}_{col}";
            }
        }
        Debug.Log("Pins generated successfully.");
    }
}