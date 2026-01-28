using UnityEngine;

public class PinSetter : MonoBehaviour
{
    [Tooltip("ピン生成用のPrefab")]
    public GameObject pinPrefab;

    [Tooltip("ピン同士の間隔（ルート上） 標準は12インチ＝0.3048m")]
    public float spacing = 0.3048f;

    [SerializeField] int rows = 4; // ボウリングなら4行

    [ContextMenu("Generate Pins")] // コンテキストメニューから実行可能にする
    public void GeneratePins()
    {
        // 既存の子オブジェクトを削除する（重複防止）
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        for (int row = 0; row < rows; row++)
        {
            // 各行のピンの数は (row + 1) 本
            for (int col = 0; col <= row; col++)
            {
                // Z座標: 行番号 × 正三角形の高さ
                float zPos = row * spacing * Mathf.Sqrt(3) / 2;

                // X座標: 列インデックス - (その行の総ピン数の半分) を掛けて中央揃え
                // 行の幅は row * spacing なので、中央は (row * 0.5f) ずらす
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