using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BowlingUIManager : MonoBehaviour
{
    [Header("UI References")]
    // 上段：投球ごとのテキストリスト
    [SerializeField] private List<TMP_Text> rollBoxTexts;

    // 下段：各フレームの累積スコア
    [SerializeField] private List<TMP_Text> totalScoreTexts;

    [SerializeField] private BowlingPinManager pinManager;

    public void UpdateScoreBoard(BowlingScoreManager scoreManager)
    {
        List<int> rolls = scoreManager.Rolls;
        int[] frameScores = scoreManager.GetCumulativeScores();
        int totalFrames = scoreManager.TotalFrames;

        int rollIndex = 0; // データのインデックス
        int boxIndex = 0;  // UIテキスト(Box)のインデックス

        // 1〜totalFramesフレームのループ
        for (int f = 1; f <= totalFrames; f++)
        {
            // UIが不足している場合は処理を停止
            if (f <= totalFrames - 1) // 通常フレーム
            {
                if (boxIndex + 1 >= rollBoxTexts.Count) break;
            }
            else // 最終フレーム
            {
                if (boxIndex + 2 >= rollBoxTexts.Count) break;
            }

            // --- 上段 (History / Box) の更新 ---
            if (f < totalFrames) // 通常フレーム (Boxは2つ)
            {
                TMP_Text text1 = rollBoxTexts[boxIndex];
                TMP_Text text2 = rollBoxTexts[boxIndex + 1];
                boxIndex += 2;

                // データがない場合
                if (rollIndex >= rolls.Count)
                {
                    text1.text = "";
                    text2.text = "";
                }
                else
                {
                    int first = rolls[rollIndex];

                    if (first == pinManager.GetFrameMaxPinCount(f-1)) // Strike
                    {
                        text1.text = "X";
                        text2.text = "";
                        rollIndex++;
                    }
                    else // Open / Spare
                    {
                        text1.text = first.ToString();

                        if (rollIndex + 1 < rolls.Count)
                        {
                            int second = rolls[rollIndex + 1];
                            text2.text = (first + second == pinManager.GetFrameMaxPinCount(f-1)) ? "/" : second.ToString();
                            rollIndex += 2;
                        }
                        else
                        {
                            text2.text = "";
                            rollIndex++;
                        }
                    }
                }
            }
            else // 最終フレーム (Boxは末尾の3つを使用)
            {
                // rollBoxTextsの末尾3つを使用
                int lastBoxIndex = rollBoxTexts.Count - 1;
                TMP_Text text1 = rollBoxTexts[lastBoxIndex - 2];
                TMP_Text text2 = rollBoxTexts[lastBoxIndex - 1];
                TMP_Text text3 = rollBoxTexts[lastBoxIndex];

                // 最終フレームの投球数を計算（rollsの末尾から最大3つ）
                int finalFrameRollCount = Mathf.Min(rolls.Count - rollIndex, 3);

                // 1つ目のBox
                if (finalFrameRollCount >= 1)
                {
                    int r1 = rolls[rollIndex];
                    text1.text = (r1 == pinManager.GetFrameMaxPinCount(f - 1)) ? "X" : r1.ToString();
                }
                else text1.text = "";

                // 2つ目のBox
                if (finalFrameRollCount >= 2)
                {
                    int r1 = rolls[rollIndex];
                    int r2 = rolls[rollIndex + 1];

                    if (r2 == pinManager.GetFrameMaxPinCount(f - 1))
                        text2.text = "X";
                    else if (r1 + r2 == pinManager.GetFrameMaxPinCount(f - 1) && r1 != pinManager.GetFrameMaxPinCount(f - 1))
                        text2.text = "/";
                    else
                        text2.text = r2.ToString();
                }
                else text2.text = "";

                // 3つ目のBox
                if (finalFrameRollCount >= 3)
                {
                    int r2 = rolls[rollIndex + 1];
                    int r3 = rolls[rollIndex + 2];

                    if (r3 == pinManager.GetFrameMaxPinCount(f - 1))
                        text3.text = "X";
                    else if (r2 + r3 == pinManager.GetFrameMaxPinCount(f - 1) && r2 != pinManager.GetFrameMaxPinCount(f - 1))
                        text3.text = "/";
                    else
                        text3.text = r3.ToString();
                }
                else text3.text = "";
            }

            // --- 下段 (Total Score) の更新 ---
            if (f == totalFrames) // 最終フレームは末尾を参照
            {
                int lastIndex = totalScoreTexts.Count - 1;
                if (lastIndex >= 0 && f - 1 < frameScores.Length)
                {
                    int score = frameScores[f - 1];
                    totalScoreTexts[lastIndex].text = (score != -1) ? score.ToString() : "";
                }
            }
            else if (f - 1 < totalScoreTexts.Count && f - 1 < frameScores.Length)
            {
                int score = frameScores[f - 1];
                totalScoreTexts[f - 1].text = (score != -1) ? score.ToString() : "";
            }
        }
        /*
        // 使用されていないUIテキストをクリア
        for (int i = totalFrames; i < totalScoreTexts.Count; i++)
        {
            totalScoreTexts[i].text = "";
        }
        */
    }
}