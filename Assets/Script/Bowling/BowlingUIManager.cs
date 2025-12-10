using System.Collections.Generic;
using UnityEngine;
using TMPro;

// UI担当：データの表示のみを行う
// Managerから計算済みのデータをもらってテキストを整形する
public class BowlingUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text rollHistoryText; // 上段 (例: X  9 /)
    [SerializeField] private TMP_Text totalScoreText;  // 下段 (例: 20 38)

    // Managerから呼び出される更新処理
    public void UpdateScoreBoard(BowlingScoreManager scoreManager)
    {
        List<int> rolls = scoreManager.Rolls;
        int[] frameScores = scoreManager.GetCumulativeScores(); // 計算済みのスコアを受け取る

        string historyStr = "";
        string scoreStr = "";
        int rollIndex = 0;

        // 1～10フレームの表示用ループ
        for (int f = 1; f <= 10; f++)
        {
            if (rollIndex >= rolls.Count)
            {
                // まだデータがない未来のフレームは空欄
                historyStr += "     | ";
                scoreStr += "     | ";
                continue;
            }

            // --- 上段 (History) の文字列生成 ---
            string frameHist = "";
            int advance = 0;

            if (f < 10) // 1~9フレーム
            {
                int first = rolls[rollIndex];
                if (first == 10) // Strike
                {
                    frameHist = " X   ";
                    advance = 1;
                }
                else
                {
                    frameHist += first + " ";
                    if (rollIndex + 1 < rolls.Count)
                    {
                        int second = rolls[rollIndex + 1];
                        if (first + second == 10) frameHist += "/";
                        else frameHist += second;
                        advance = 2;
                    }
                    else
                    {
                        advance = 1; // 1投目だけ投げた状態
                    }
                }
            }
            else // 10フレーム
            {
                // 10フレは最大3回分を表示
                // ここは簡易的に残りのrollをすべて表示する
                int remaining = rolls.Count - rollIndex;
                for (int i = 0; i < remaining && i < 3; i++)
                {
                    int pin = rolls[rollIndex + i];
                    if (pin == 10) frameHist += "X ";
                    else if (i > 0 && rolls[rollIndex + i - 1] + pin == 10 && rolls[rollIndex + i - 1] != 10) frameHist += "/ ";
                    else frameHist += pin + " ";
                }
                advance = remaining;
            }

            // --- 下段 (Score) の文字列生成 ---
            int score = frameScores[f - 1]; // 0始まりのインデックスなので-1
            string frameScoreStr = (score != -1) ? score.ToString() : "";

            // 文字列連結とパディング
            historyStr += $"{frameHist.PadRight(5)}| ";
            scoreStr += $"{frameScoreStr.PadRight(5)}| ";

            rollIndex += advance;
        }

        // テキスト反映
        if (rollHistoryText != null) rollHistoryText.text = historyStr;
        if (totalScoreText != null) totalScoreText.text = scoreStr;
    }
}