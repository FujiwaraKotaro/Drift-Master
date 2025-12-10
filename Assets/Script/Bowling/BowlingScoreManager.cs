using System.Collections.Generic;
using UnityEngine;

// ロジック担当：スコア計算とゲーム進行管理
// UIへの直接の参照は持たず、UIManagerに更新を依頼する
public class BowlingScoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BowlingUIManager uiManager; // UI担当スクリプト

    // 投球ごとの倒した本数を記録するリスト（外部からの読み取り専用プロパティ付き）
    private List<int> rolls = new List<int>();
    public List<int> Rolls => rolls;

    // 現在のフレーム（1~10）
    public int CurrentFrame { get; private set; } = 1;

    // 現在の投球（1投目 or 2投目、10フレは3投目まで）
    public int CurrentThrow { get; private set; } = 1;

    // ゲームが終了したか
    public bool IsGameOver { get; private set; } = false;


    // --- 外部からの操作 ---

    public void RecordThrow(int pinsDown)
    {
        rolls.Add(pinsDown);
        RefreshUI();
    }

    public void AdvanceTurn(int pinsDown)
    {
        if (IsGameOver) return;

        // --- 10フレーム目の処理 ---
        if (CurrentFrame == 10)
        {
            if (CurrentThrow == 1)
            {
                CurrentThrow++;
            }
            else if (CurrentThrow == 2)
            {
                int firstThrow = rolls[rolls.Count - 2];
                int secondThrow = rolls[rolls.Count - 1];
                if (firstThrow + secondThrow >= 10) CurrentThrow++;
                else IsGameOver = true;
            }
            else
            {
                IsGameOver = true;
            }
        }
        // --- 1~9フレーム目の処理 ---
        else
        {
            if (CurrentThrow == 1)
            {
                if (pinsDown == 10) // Strike
                {
                    CurrentThrow = 1;
                    CurrentFrame++;
                }
                else
                {
                    CurrentThrow++;
                }
            }
            else
            {
                CurrentThrow = 1;
                CurrentFrame++;
            }
        }

        if (CurrentFrame > 10) IsGameOver = true;

        RefreshUI();
    }

    // --- 計算ロジック ---

    private void RefreshUI()
    {
        if (uiManager != null)
        {
            // 自分自身（Manager）を渡して、UIに描画してもらう
            uiManager.UpdateScoreBoard(this);
        }
    }

    // 各フレームごとの累計スコアを計算して配列で返す
    // 計算できない（まだ投げてない）フレームは -1 を入れる
    public int[] GetCumulativeScores()
    {
        int[] frameScores = new int[10];
        for (int i = 0; i < 10; i++) frameScores[i] = -1; // 初期化

        int runningTotal = 0;
        int rollIndex = 0;

        for (int f = 0; f < 10; f++)
        {
            if (rollIndex >= rolls.Count) break;

            int currentFrameScore = -1;
            int advance = 0; // ループをどれだけ進めるか

            // 10フレーム目
            if (f == 9)
            {
                int sum = 0;
                int throws = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (rollIndex + i < rolls.Count)
                    {
                        sum += rolls[rollIndex + i];
                        throws++;
                    }
                }
                // 終了条件を満たしていればスコア確定
                if (IsGameOver || (throws == 2 && sum < 10) || throws == 3)
                {
                    currentFrameScore = sum;
                }
                advance = throws;
            }
            // 通常フレーム (Strike)
            else if (rolls[rollIndex] == 10)
            {
                if (rollIndex + 2 < rolls.Count)
                    currentFrameScore = 10 + rolls[rollIndex + 1] + rolls[rollIndex + 2];
                advance = 1;
            }
            // 通常フレーム (Spare)
            else if (rollIndex + 1 < rolls.Count && (rolls[rollIndex] + rolls[rollIndex + 1] == 10))
            {
                if (rollIndex + 2 < rolls.Count)
                    currentFrameScore = 10 + rolls[rollIndex + 2];
                advance = 2;
            }
            // 通常フレーム (Open)
            else if (rollIndex + 1 < rolls.Count)
            {
                currentFrameScore = rolls[rollIndex] + rolls[rollIndex + 1];
                advance = 2;
            }
            else
            {
                // 途中
                advance = 1;
            }

            // スコア確定なら加算して記録
            if (currentFrameScore != -1)
            {
                runningTotal += currentFrameScore;
                frameScores[f] = runningTotal;
            }

            rollIndex += advance;
        }

        return frameScores;
    }
}