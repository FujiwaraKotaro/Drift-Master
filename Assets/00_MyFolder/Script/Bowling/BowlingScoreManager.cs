using System.Collections.Generic;
using UnityEngine;

// ロジック担当：スコア計算とゲーム状態の管理
// 「現在何フレーム目か？」「次はピンをリセットすべきか？」をすべて履歴(rolls)から都度計算する
public class BowlingScoreManager : MonoBehaviour
{
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
        uiManager.UpdateScoreBoard(this); // データ更新したら即UI反映
    }

    // --- 計算ロジック ---

    // 現在の履歴から「ゲームが終わっているか」「次はどうすべきか」を算出する
    public GameStatus CheckGameStatus()
    {
        int rollIndex = 0;
        int frame = 1;

        // 1〜9フレームのシミュレーション
        for (; frame < 10; frame++)
        {
            if (rollIndex >= rolls.Count)
            {
                // データ切れ＝ここが「今のフレーム」の開始地点
                // 新しいフレームの1投目なので、ピンは全リセット
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }

            int first = rolls[rollIndex];

            if (first == 10) // Strike
            {
                rollIndex++;
                // 次のデータがなければ、次は「新しいフレームの1投目」
                if (rollIndex >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }
            else // Open or Spare
            {
                // 1投目だけ投げた状態か？
                if (rollIndex + 1 >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };

                rollIndex += 2; // 2投完了
            }
        }

        // 10フレーム目の処理
        if (frame == 10)
        {
            // まだ10フレに到達していない（9フレまででデータが終わっている）場合
            if (rollIndex >= rolls.Count)
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

            int throwsIn10th = rolls.Count - rollIndex;

            // 1投目を投げた直後
            if (throwsIn10th == 1)
            {
                int first = rolls[rollIndex];
                // ストライクならリセット、それ以外なら除去
                return new GameStatus
                {
                    IsGameOver = false,
                    NextAction = (first == 10) ? NextPinAction.ResetAll : NextPinAction.RemoveFallen
                };
            }
            // 2投目を投げた直後
            else if (throwsIn10th == 2)
            {
                int first = rolls[rollIndex];
                int second = rolls[rollIndex + 1];

                // 終了判定: オープンフレームなら終了
                if (first + second < 10 && first != 10)
                    return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };

                // 3投目がある場合
                // スペアならリセット
                if (first + second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                // ストライク→ストライクならリセット
                if (first == 10 && second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                // ストライク→非ストライク（例: X, 5）なら除去
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };
            }
            // 3投目を投げた直後
            else if (throwsIn10th == 3)
            {
                return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };
            }
        }

        return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };
    }

    // UI用のスコア計算（既存ロジックを微修正して維持）
    public int[] GetCumulativeScores()
    {
        int[] frameScores = new int[10];
        for (int i = 0; i < 10; i++) frameScores[i] = -1;

        int runningTotal = 0;
        int rollIndex = 0;

        for (int f = 0; f < 10; f++)
        {
            if (rollIndex >= rolls.Count) break;

            int currentFrameScore = -1;
            int advance = 0;

            if (f == 9) // 10フレーム
            {
                int sum = 0;
                int throws = 0;
                // 残りの投球を合計
                for (int i = 0; rollIndex + i < rolls.Count && i < 3; i++)
                {
                    sum += rolls[rollIndex + i];
                    throws++;
                }

                // 終了条件を満たしたかチェック
                bool isFrameFinished = false;
                if (throws == 3) isFrameFinished = true;
                else if (throws == 2 && sum < 10 && rolls[rollIndex] != 10) isFrameFinished = true; // オープン

                if (isFrameFinished) currentFrameScore = sum;
                advance = throws;
            }
            else // 1-9フレーム
            {
                if (rolls[rollIndex] == 10) // Strike
                {
                    if (rollIndex + 2 < rolls.Count)
                        currentFrameScore = 10 + rolls[rollIndex + 1] + rolls[rollIndex + 2];
                    advance = 1;
                }
                else if (rollIndex + 1 < rolls.Count) // Spare or Open
                {
                    if (rolls[rollIndex] + rolls[rollIndex + 1] == 10) // Spare
                    {
                        if (rollIndex + 2 < rolls.Count)
                            currentFrameScore = 10 + rolls[rollIndex + 2];
                    }
                    else // Open
                    {
                        currentFrameScore = rolls[rollIndex] + rolls[rollIndex + 1];
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