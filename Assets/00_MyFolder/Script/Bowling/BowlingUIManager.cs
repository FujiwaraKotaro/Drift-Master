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

                    if (first == 10) // Strike
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
                            text2.text = (first + second == 10) ? "/" : second.ToString();
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
            else // 最終フレーム (Boxは3つ)
            {
                TMP_Text text1 = rollBoxTexts[boxIndex];
                TMP_Text text2 = rollBoxTexts[boxIndex + 1];
                TMP_Text text3 = rollBoxTexts[boxIndex + 2];

                int remainingRolls = rolls.Count - rollIndex;

                // 1つ目のBox
                if (remainingRolls >= 1)
                {
                    int r1 = rolls[rollIndex];
                    text1.text = (r1 == 10) ? "X" : r1.ToString();
                }
                else text1.text = "";

                // 2つ目のBox
                if (remainingRolls >= 2)
                {
                    int r1 = rolls[rollIndex];
                    int r2 = rolls[rollIndex + 1];

                    if (r2 == 10)
                        text2.text = "X";
                    else if (r1 + r2 == 10 && r1 != 10)
                        text2.text = "/";
                    else
                        text2.text = r2.ToString();
                }
                else text2.text = "";

                // 3つ目のBox
                if (remainingRolls >= 3)
                {
                    int r2 = rolls[rollIndex + 1];
                    int r3 = rolls[rollIndex + 2];

                    if (r3 == 10)
                        text3.text = "X";
                    else if (r2 + r3 == 10 && r2 != 10)
                        text3.text = "/";
                    else
                        text3.text = r3.ToString();
                }
                else text3.text = "";
            }

            // --- 下段 (Total Score) の更新 ---
            if (f - 1 < totalScoreTexts.Count && f - 1 < frameScores.Length)
            {
                int score = frameScores[f - 1];
                totalScoreTexts[f - 1].text = (score != -1) ? score.ToString() : "";
            }
        }

        // 使用されていないUIテキストをクリア
        for (int i = totalFrames; i < totalScoreTexts.Count; i++)
        {
            totalScoreTexts[i].text = "";
        }
    }
}