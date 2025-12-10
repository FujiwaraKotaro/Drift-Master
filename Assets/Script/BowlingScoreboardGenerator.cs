using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BowlingScoreboardGenerator : EditorWindow
{
    // 設定項目
    private float totalWidth = 1200f;
    private float totalHeight = 200f;
    private Color headerColor = new Color(0.0f, 0.3f, 0.7f, 1f); // 濃い青
    private Color bodyColor = new Color(0.9f, 0.95f, 1.0f, 1f); // 薄い水色
    private TMP_FontAsset fontAsset;

    [MenuItem("Tools/Bowling Scoreboard Generator")]
    public static void ShowWindow()
    {
        GetWindow<BowlingScoreboardGenerator>("Scoreboard Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("ボウリングスコア表 自動生成ツール", EditorStyles.boldLabel);

        totalWidth = EditorGUILayout.FloatField("全体の幅", totalWidth);
        totalHeight = EditorGUILayout.FloatField("全体の高さ", totalHeight);
        headerColor = EditorGUILayout.ColorField("ヘッダー色", headerColor);
        bodyColor = EditorGUILayout.ColorField("背景色", bodyColor);
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("フォント(TMP)", fontAsset, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("スコア表を生成 (Generate)"))
        {
            CreateScoreboard();
        }
    }

    private void CreateScoreboard()
    {
        // 親となるCanvasを探す、なければ作る
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // 1. メインの親パネル作成
        GameObject rootObj = CreateUIObject("Scoreboard_Root", canvas.transform);
        RectTransform rootRect = rootObj.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(totalWidth, totalHeight);

        // 横並びレイアウト設定
        HorizontalLayoutGroup rootLayout = rootObj.AddComponent<HorizontalLayoutGroup>();
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = true;
        rootLayout.spacing = 2; // フレーム間の隙間

        // 2. フレーム1〜9の作成
        for (int i = 1; i <= 9; i++)
        {
            CreateFrame(rootObj.transform, i, false);
        }

        // 3. フレーム10の作成
        CreateFrame(rootObj.transform, 10, true);

        // 4. TOTAL欄の作成
        CreateTotalFrame(rootObj.transform);

        Debug.Log("ボウリングスコア表の生成が完了しました！");
    }

    // 通常フレーム作成メソッド
    private void CreateFrame(Transform parent, int frameNum, bool isLastFrame)
    {
        GameObject frameObj = CreateUIObject($"Frame_{frameNum}", parent);

        // 背景とレイアウト
        Image bg = frameObj.AddComponent<Image>();
        bg.color = bodyColor;

        VerticalLayoutGroup vLayout = frameObj.AddComponent<VerticalLayoutGroup>();
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = false; // 上下の比率を固定しない

        // --- 上段：ヘッダー (数字) ---
        GameObject headerObj = CreateUIObject("Header", frameObj.transform);
        Image headerImg = headerObj.AddComponent<Image>();
        headerImg.color = headerColor;

        LayoutElement headerLE = headerObj.AddComponent<LayoutElement>();
        headerLE.flexibleHeight = 0.3f; // 高さの比率 30%
        headerLE.minHeight = 40;

        CreateText(headerObj.transform, frameNum.ToString(), 36, Color.white, true);

        // --- 中段：スコア (投球) ---
        GameObject scoresObj = CreateUIObject("Scores_Row", frameObj.transform);

        LayoutElement scoresLE = scoresObj.AddComponent<LayoutElement>();
        scoresLE.flexibleHeight = 0.3f; // 高さの比率 30%

        HorizontalLayoutGroup scoreHLayout = scoresObj.AddComponent<HorizontalLayoutGroup>();
        scoreHLayout.childControlWidth = true;
        scoreHLayout.childForceExpandWidth = true;
        scoreHLayout.childAlignment = TextAnchor.MiddleCenter;

        // 投球枠の作成 (通常は2つ、10フレーム目は3つ)
        int throwCount = isLastFrame ? 3 : 2;
        for (int t = 1; t <= throwCount; t++)
        {
            GameObject throwObj = CreateUIObject($"Throw_{t}", scoresObj.transform);
            // 枠線代わりの背景（必要なら）
            // Image throwBg = throwObj.AddComponent<Image>(); 
            // throwBg.color = new Color(0,0,0,0.1f); 

            // 三角形やXを表示するためのテキスト
            CreateText(throwObj.transform, "", 24, Color.black, false);

            // フレーム内の区切り線（右側）を追加（最後以外）
            if (t < throwCount)
            {
                // 簡易的な区切り線
                GameObject line = CreateUIObject("Div_Line", scoresObj.transform);
                LayoutElement lineLE = line.AddComponent<LayoutElement>();
                lineLE.preferredWidth = 2; // 線の太さ
                Image lineImg = line.AddComponent<Image>();
                lineImg.color = new Color(0, 0, 0, 0.2f);
            }
        }

        // --- 下段：小計 ---
        GameObject subTotalObj = CreateUIObject("SubTotal", frameObj.transform);
        LayoutElement subTotalLE = subTotalObj.AddComponent<LayoutElement>();
        subTotalLE.flexibleHeight = 0.4f; // 高さの比率 40%

        CreateText(subTotalObj.transform, "", 40, Color.black, true); // 初期値は空
    }

    // 合計(TOTAL)フレーム作成メソッド
    private void CreateTotalFrame(Transform parent)
    {
        GameObject totalObj = CreateUIObject("Total_Column", parent);

        Image bg = totalObj.AddComponent<Image>();
        bg.color = headerColor; // ここだけ全体が青っぽいデザインにする場合

        VerticalLayoutGroup vLayout = totalObj.AddComponent<VerticalLayoutGroup>();
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;

        // 幅を少し広めにする
        LayoutElement totalLE = totalObj.AddComponent<LayoutElement>();
        totalLE.preferredWidth = 140;

        // ヘッダー
        GameObject headerObj = CreateUIObject("Header", totalObj.transform);
        LayoutElement headerLE = headerObj.AddComponent<LayoutElement>();
        headerLE.flexibleHeight = 0.3f;

        CreateText(headerObj.transform, "TOTAL", 24, Color.white, true);

        // 合計数字表示エリア
        GameObject valueObj = CreateUIObject("Value", totalObj.transform);
        LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
        valueLE.flexibleHeight = 0.7f;

        CreateText(valueObj.transform, "0", 60, Color.white, true);
    }

    // 汎用：UIオブジェクト作成ヘルパー
    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        Undo.RegisterCreatedObjectUndo(obj, "Create UI Element");
        return obj;
    }

    // 汎用：テキスト作成ヘルパー
    private void CreateText(Transform parent, string content, float fontSize, Color color, bool bold)
    {
        GameObject textObj = CreateUIObject("Text", parent);

        // 親いっぱいに広げる
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10;
        tmp.fontSizeMax = fontSize;

        if (bold) tmp.fontStyle = FontStyles.Bold;
        if (fontAsset != null) tmp.font = fontAsset;
    }
}