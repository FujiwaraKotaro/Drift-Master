using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BowlingUIGenerator : EditorWindow
{
    private const int BORDER_THICKNESS = 2;

    [MenuItem("Tools/Create Bowling UI")]
    public static void CreateScoreBoard()
    {
        // 既存削除
        GameObject existingUI = GameObject.Find("BowlingScoreBoard");
        if (existingUI != null) Undo.DestroyObjectImmediate(existingUI);

        // Canvas準備
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

        // 親パネル作成
        GameObject scoreBoardRoot = new GameObject("BowlingScoreBoard");
        scoreBoardRoot.transform.SetParent(canvas.transform, false);
        Image rootBg = scoreBoardRoot.AddComponent<Image>();
        rootBg.color = Color.black;

        RectTransform rootRT = scoreBoardRoot.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(1200, 180);

        HorizontalLayoutGroup rootLayout = scoreBoardRoot.AddComponent<HorizontalLayoutGroup>();
        rootLayout.padding = new RectOffset(BORDER_THICKNESS, BORDER_THICKNESS, BORDER_THICKNESS, BORDER_THICKNESS);
        rootLayout.spacing = BORDER_THICKNESS;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;

        Undo.RegisterCreatedObjectUndo(scoreBoardRoot, "Create Bowling UI");

        // Managerに渡すリスト
        List<TMP_Text> boxTexts = new List<TMP_Text>(); // Roll履歴用 (Boxごと)
        List<TMP_Text> scoreTexts = new List<TMP_Text>(); // 合計スコア用

        // 1～10フレーム作成
        for (int i = 1; i <= 10; i++)
        {
            CreateFrame(i, scoreBoardRoot.transform, boxTexts, scoreTexts);
        }

        // Managerへ割り当て
        AssignToManager(boxTexts, scoreTexts);

        Debug.Log($"ボウリングUI生成完了。BoxText数: {boxTexts.Count} (期待値:21)");
    }

    private static void CreateFrame(int frameNum, Transform parent, List<TMP_Text> boxList, List<TMP_Text> scoreList)
    {
        // --- フレーム外枠 ---
        GameObject frameObj = new GameObject($"Frame_{frameNum}");
        frameObj.transform.SetParent(parent, false);

        Image frameBg = frameObj.AddComponent<Image>();
        frameBg.color = Color.black;

        VerticalLayoutGroup vLayout = frameObj.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = BORDER_THICKNESS;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = false;

        LayoutElement layoutElem = frameObj.AddComponent<LayoutElement>();
        layoutElem.flexibleWidth = (frameNum == 10) ? 1.6f : 1.0f;

        // 1. ヘッダー
        GameObject headerObj = CreatePanel("Header", frameObj.transform, Color.white);
        CreateText(headerObj, frameNum.ToString(), 20, true); // ヘッダー用テキストはリスト管理しない
        LayoutElement headerLE = headerObj.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 35;

        // 2. 中段 (Boxエリア)
        GameObject historyContainer = new GameObject("HistoryContainer");
        historyContainer.transform.SetParent(frameObj.transform, false);
        Image histBg = historyContainer.AddComponent<Image>();
        histBg.color = Color.black; // グリッド線用

        HorizontalLayoutGroup hLayout = historyContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = BORDER_THICKNESS;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;

        LayoutElement historyLE = historyContainer.AddComponent<LayoutElement>();
        historyLE.flexibleHeight = 1.5f;

        // ★Boxの生成ループ (ここを変更)
        int boxCount = (frameNum == 10) ? 3 : 2;
        for (int b = 0; b < boxCount; b++)
        {
            // 白いBoxを作る
            GameObject boxObj = CreatePanel($"Box_{b + 1}", historyContainer.transform, Color.white);

            // その中にテキストを作る
            TMP_Text boxText = CreateText(boxObj, "", 28, false);

            // リストに追加 (Manager用)
            boxList.Add(boxText);
        }

        // 3. 下段 (合計スコア)
        GameObject scoreObj = CreatePanel("ScoreRow", frameObj.transform, Color.white);
        TMP_Text scoreTextComponent = CreateText(scoreObj, "0", 32, true);
        scoreList.Add(scoreTextComponent);

        LayoutElement scoreLE = scoreObj.AddComponent<LayoutElement>();
        scoreLE.flexibleHeight = 1.0f;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    private static TMP_Text CreateText(GameObject parent, string content, float fontSize, bool isBold)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;

        return tmp;
    }

    private static void AssignToManager(List<TMP_Text> boxList, List<TMP_Text> scoreList)
    {
        BowlingUIManager manager = FindObjectOfType<BowlingUIManager>();
        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);

            // プロパティ名を修正したものに対応 (rollHistoryTexts -> rollBoxTexts)
            // もしスクリプト側で変数名を変えた場合はここも合わせる必要があります。
            // 今回は BowlingUIManager.cs で rollBoxTexts に変えています。
            SerializedProperty propBox = so.FindProperty("rollBoxTexts");
            SerializedProperty propScore = so.FindProperty("totalScoreTexts");

            if (propBox != null && propScore != null)
            {
                propBox.ClearArray();
                propScore.ClearArray();

                // BoxTextは21個あるはず
                for (int i = 0; i < boxList.Count; i++)
                {
                    propBox.InsertArrayElementAtIndex(i);
                    propBox.GetArrayElementAtIndex(i).objectReferenceValue = boxList[i];
                }

                // ScoreTextは10個
                for (int i = 0; i < scoreList.Count; i++)
                {
                    propScore.InsertArrayElementAtIndex(i);
                    propScore.GetArrayElementAtIndex(i).objectReferenceValue = scoreList[i];
                }

                so.ApplyModifiedProperties();
                Debug.Log($"Reference Assigned: {boxList.Count} boxes, {scoreList.Count} scores.");
            }
        }
    }
}