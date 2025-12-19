using System.Collections.Generic;
using UnityEngine;

// ロジック担当：スコア計算とゲーム状態の管理
// 「現在何フレーム目か？」「次はピンをリセットすべきか？」をすべて履歴(rolls)から都度計算する
public class BowlingScoreManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int totalFrames = 10; // フレーム数を可変にする
    public int TotalFrames => totalFrames;

    [SerializeField] private BowlingUIManager uiManager;

    // 唯一の「正解」データ (Single Source of Truth)
    private List<int> rolls = new List<int>();
    public List<int> Rolls => rolls;

    // Directorに「次にピンをどう操作すべきか」を伝えるための列挙型
    public enum NextPinAction
    {
        None,           // 何もしない（ゲーム終了時など）
        ResetAll,       // 全ピンを復活させる（フレーム開始時、ストライク後など）
        RemoveFallen    // 倒れたピンだけ除く（2投目の前など）
    }

    // ゲームの状態を返す構造体
    public struct GameStatus
    {
        public bool IsGameOver;
        public NextPinAction NextAction;
    }

    // --- 外部からの操作 ---

    public void RecordThrow(int pinsDown)
    {
        rolls.Add(pinsDown);

        Debug.Log($"投球記録: ピン={pinsDown}");

        uiManager.UpdateScoreBoard(this); // データ更新したら即UI反映
    }

    // フレーム数を動的に設定するメソッド
    public void SetTotalFrames(int frames)
    {
        if (frames > 0)
        {
            totalFrames = frames;
            Debug.Log($"総フレーム数を{frames}に変更しました");
        }
    }

    // --- 計算ロジック ---

    // 現在の履歴から「ゲームが終わっているか」「次はどうすべきか」を算出する
    public GameStatus CheckGameStatus()
    {
        int rollIndex = 0;
        int frame = 1;

        // 1〜(最終フレーム-1)のシミュレーション
        for (; frame < totalFrames; frame++)
        {
            if (rollIndex >= rolls.Count)
            {
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }

            int first = rolls[rollIndex];

            if (first == 10) // Strike
            {
                rollIndex++;
                if (rollIndex >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }
            else // Open or Spare
            {
                if (rollIndex + 1 >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };

                rollIndex += 2; // 2投完了
            }
        }

        // 最終フレームの処理
        if (frame == totalFrames)
        {
            if (rollIndex >= rolls.Count)
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

            int throwsInFinalFrame = rolls.Count - rollIndex;

            // 1投目を投げた直後
            if (throwsInFinalFrame == 1)
            {
                int first = rolls[rollIndex];
                return new GameStatus
                {
                    IsGameOver = false,
                    NextAction = (first == 10) ? NextPinAction.ResetAll : NextPinAction.RemoveFallen
                };
            }
            // 2投目を投げた直後
            else if (throwsInFinalFrame == 2)
            {
                int first = rolls[rollIndex];
                int second = rolls[rollIndex + 1];

                // 終了判定: オープンフレームなら終了
                if (first + second < 10 && first != 10)
                    return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };

                // 3投目がある場合
                if (first + second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                if (first == 10 && second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };
            }
            // 3投目を投げた直後
            else if (throwsInFinalFrame == 3)
            {
                return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };
            }
        }

        return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };
    }

    // UI用のスコア計算（標準的なボウリングルールに従う）
    public int[] GetCumulativeScores()
    {
        int[] frameScores = new int[totalFrames];
        for (int i = 0; i < totalFrames; i++) frameScores[i] = -1;

        int runningTotal = 0;
        int rollIndex = 0;

        for (int f = 0; f < totalFrames; f++)
        {
            if (rollIndex >= rolls.Count) break;

            int currentFrameScore = -1;
            int advance = 0;

            if (f == totalFrames - 1) // 最終フレーム
            {
                int sum = 0;
                int throws = 0;

                // 最終フレームは全投球の合計
                for (int i = 0; rollIndex + i < rolls.Count && i < 3; i++)
                {
                    sum += rolls[rollIndex + i];
                    throws++;
                }

                bool isFrameFinished = false;
                if (throws == 3) isFrameFinished = true;
                else if (throws == 2 && rolls[rollIndex] + (throws > 1 ? rolls[rollIndex + 1] : 0) < 10 && rolls[rollIndex] != 10)
                    isFrameFinished = true;

                if (isFrameFinished) currentFrameScore = sum;
                advance = throws;
            }
            else // 通常フレーム
            {
                if (rolls[rollIndex] == 10) // Strike
                {
                    if (rollIndex + 2 < rolls.Count)
                    {
                        // ストライク: 10 + 次の2投の合計
                        currentFrameScore = 10 + rolls[rollIndex + 1] + rolls[rollIndex + 2];

                        Debug.Log($"フレーム{f + 1} ストライク: 10+{rolls[rollIndex + 1]}+{rolls[rollIndex + 2]}={currentFrameScore}");
                    }
                    advance = 1;
                }
                else if (rollIndex + 1 < rolls.Count) // Spare or Open
                {
                    int first = rolls[rollIndex];
                    int second = rolls[rollIndex + 1];

                    if (first + second == 10) // Spare
                    {
                        if (rollIndex + 2 < rolls.Count)
                        {
                            // スペア: 10 + 次の1投
                            currentFrameScore = 10 + rolls[rollIndex + 2];

                            Debug.Log($"フレーム{f + 1} スペア: {first}+{second}+{rolls[rollIndex + 2]}={currentFrameScore}");
                        }
                    }
                    else // Open
                    {
                        // オープン: 2投の合計
                        currentFrameScore = first + second;

                        Debug.Log($"フレーム{f + 1} オープン: {first}+{second}={currentFrameScore}");
                    }
                    advance = 2;
                }
                else
                {
                    advance = 1; // 途中
                }
            }

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