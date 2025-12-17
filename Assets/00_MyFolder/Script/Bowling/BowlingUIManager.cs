using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BowlingUIManager : MonoBehaviour
{
    [Header("UI References")]
    // 上段：投球ごとのテキストリスト
    // 要素数は21個である必要があります (Frame1-9: 2個ずつ, Frame10: 3個 = 18+3=21)
    [SerializeField] private List<TMP_Text> rollBoxTexts;

    // 下段：各フレームの累積スコア (サイズ10)
    [SerializeField] private List<TMP_Text> totalScoreTexts;

    public void UpdateScoreBoard(BowlingScoreManager scoreManager)
    {
        List<int> rolls = scoreManager.Rolls;
        List<int> multipliers = scoreManager.RollMultipliers;
        int[] frameScores = scoreManager.GetCumulativeScores();

        int rollIndex = 0; // データのインデックス
        int boxIndex = 0;  // UIテキスト(Box)のインデックス

        // 1～10フレームのループ
        for (int f = 1; f <= 10; f++)
        {
            // --- 上段 (History / Box) の更新 ---

            if (f < 10) // 1~9フレーム (Boxは2つ)
            {
                // テキスト参照を取得 (範囲外チェック含む)
                if (boxIndex + 1 >= rollBoxTexts.Count) break;
                TMP_Text text1 = rollBoxTexts[boxIndex];
                TMP_Text text2 = rollBoxTexts[boxIndex + 1];
                boxIndex += 2; // 次のフレーム用に進める

                // データがない場合
                if (rollIndex >= rolls.Count)
                {
                    text1.text = "";
                    text2.text = "";
                }
                else
                {
                    int first = rolls[rollIndex];
                    int multiplier1 = (rollIndex < multipliers.Count) ? multipliers[rollIndex] : 1;

                    if (first == 10) // Strike
                    {
                        // ストライク時のアイテム倍率表示
                        text1.text = multiplier1 > 1 ? $"X ×{multiplier1}" : "X";
                        text2.text = ""; // ストライク時は2マス目を空ける
                        rollIndex++;     // データは1つ消費
                    }
                    else // Open / Spare
                    {
                        // 1投目は必ず倍率を表示（1フレーム目に倒したピンの数×M1）
                        text1.text = $"{first}  ×{multiplier1}";

                        // 2投目があるか確認
                        if (rollIndex + 1 < rolls.Count)
                        {
                            int second = rolls[rollIndex + 1];
                            int multiplier2 = (rollIndex + 1 < multipliers.Count) ? multipliers[rollIndex + 1] : 1;

                            if (first + second == 10)
                            {
                                // スペア時のアイテム倍率表示
                                text2.text = multiplier2 > 1 ? $"/  ×{multiplier2}" : "/";
                            }
                            else
                            {
                                // 2投目も倍率を表示（2フレーム目に倒したピンの数×M2）
                                text2.text = $"{second}  ×{multiplier2}";
                            }

                            rollIndex += 2; // データは2つ消費
                        }
                        else
                        {
                            // まだ投げていない
                            text2.text = "";
                            rollIndex++; // 1投目だけ消費
                        }
                    }
                }
            }
            else // 10フレーム (Boxは3つ)
            {
                // テキスト参照を取得
                if (boxIndex + 2 >= rollBoxTexts.Count) break;
                TMP_Text text1 = rollBoxTexts[boxIndex];
                TMP_Text text2 = rollBoxTexts[boxIndex + 1];
                TMP_Text text3 = rollBoxTexts[boxIndex + 2];

                // 10フレのロジック: データがある分だけ前から埋めていく
                int remainingRolls = rolls.Count - rollIndex;

                // 1つ目のBox
                if (remainingRolls >= 1)
                {
                    int r1 = rolls[rollIndex];
                    int mult1 = (rollIndex < multipliers.Count) ? multipliers[rollIndex] : 1;

                    if (r1 == 10)
                    {
                        text1.text = mult1 > 1 ? $"X ×{mult1}" : "X";
                    }
                    else
                    {
                        text1.text = $"{r1}  ×{mult1}";
                    }
                }
                else text1.text = "";

                // 2つ目のBox
                if (remainingRolls >= 2)
                {
                    int r1 = rolls[rollIndex];
                    int r2 = rolls[rollIndex + 1];
                    int mult2 = (rollIndex + 1 < multipliers.Count) ? multipliers[rollIndex + 1] : 1;

                    if (r2 == 10)
                    {
                        // 10フレでストライク
                        text2.text = mult2 > 1 ? $"X ×{mult2}" : "X";
                    }
                    else if (r1 + r2 == 10 && r1 != 10)
                    {
                        // スペア判定
                        text2.text = mult2 > 1 ? $"/  ×{mult2}" : "/";
                    }
                    else
                    {
                        text2.text = $"{r2}  ×{mult2}";   
                    }
                }
                else text2.text = "";

                // 3つ目のBox
                if (remainingRolls >= 3)
                {
                    int r2 = rolls[rollIndex + 1];
                    int r3 = rolls[rollIndex + 2];
                    int mult3 = (rollIndex + 2 < multipliers.Count) ? multipliers[rollIndex + 2] : 1;

                    if (r3 == 10)
                    {
                        text3.text = mult3 > 1 ? $"X ×{mult3}" : "X";
                    }
                    else if (r2 + r3 == 10 && r2 != 10)
                    {
                        text3.text = mult3 > 1 ? $"/  ×{mult3}" : "/";
                    }
                    else
                    {
                        text3.text = $"{r3}  ×{mult3}";
                    }
                }
                else text3.text = "";
            }

            // --- 下段 (Total Score) の更新 ---
            if (f - 1 < totalScoreTexts.Count)
            {
                int score = frameScores[f - 1];
                totalScoreTexts[f - 1].text = (score != -1) ? score.ToString() : "";
            }
        }
    }
}