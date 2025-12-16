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

    // 投球ごとのアイテム倍率を記録（投球と同じ数だけ記録される）
    private List<int> rollMultipliers = new List<int>();
    public List<int> RollMultipliers => rollMultipliers;

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

        // 投球時のアイテム倍率を記録
        GetItem getItemScript = FindObjectOfType<GetItem>();
        int currentMultiplier = getItemScript.number_getItem;
        rollMultipliers.Add(currentMultiplier);

        Debug.Log($"投球記録: ピン={pinsDown}, 倍率=x{currentMultiplier}");

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

        // 10フレーム目の処理
        if (frame == 10)
        {
            if (rollIndex >= rolls.Count)
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

            int throwsIn10th = rolls.Count - rollIndex;

            // 1投目を投げた直後
            if (throwsIn10th == 1)
            {
                int first = rolls[rollIndex];
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
                if (first + second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                if (first == 10 && second == 10)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

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

    // UI用のスコア計算（投球ごとのアイテム倍率を適用）
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

                // 10フレームは投球ごとに倍率を適用
                for (int i = 0; rollIndex + i < rolls.Count && i < 3; i++)
                {
                    int pins = rolls[rollIndex + i];
                    int multiplier = (rollIndex + i < rollMultipliers.Count) ? rollMultipliers[rollIndex + i] : 1;
                    sum += pins * multiplier;
                    throws++;
                }

                bool isFrameFinished = false;
                if (throws == 3) isFrameFinished = true;
                else if (throws == 2 && rolls[rollIndex] + (throws > 1 ? rolls[rollIndex + 1] : 0) < 10 && rolls[rollIndex] != 10)
                    isFrameFinished = true;

                if (isFrameFinished) currentFrameScore = sum;
                advance = throws;
            }
            else // 1-9フレーム
            {
                if (rolls[rollIndex] == 10) // Strike
                {
                    if (rollIndex + 2 < rolls.Count)
                    {
                        // ストライク: 10×M1 + 次1投×M2 + 次2投×M3
                        int pins1 = 10;
                        int pins2 = rolls[rollIndex + 1];
                        int pins3 = rolls[rollIndex + 2];

                        int mult1 = (rollIndex < rollMultipliers.Count) ? rollMultipliers[rollIndex] : 1;
                        int mult2 = (rollIndex + 1 < rollMultipliers.Count) ? rollMultipliers[rollIndex + 1] : 1;
                        int mult3 = (rollIndex + 2 < rollMultipliers.Count) ? rollMultipliers[rollIndex + 2] : 1;

                        currentFrameScore = (pins1 * mult1) + (pins2 * mult2) + (pins3 * mult3);

                        Debug.Log($"フレーム{f + 1} ストライク: ({pins1}×{mult1})+({pins2}×{mult2})+({pins3}×{mult3})={currentFrameScore}");
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
                            // スペア: 1投目×M1 + 2投目×M2 + 次1投×M3
                            int mult1 = (rollIndex < rollMultipliers.Count) ? rollMultipliers[rollIndex] : 1;
                            int mult2 = (rollIndex + 1 < rollMultipliers.Count) ? rollMultipliers[rollIndex + 1] : 1;
                            int mult3 = (rollIndex + 2 < rollMultipliers.Count) ? rollMultipliers[rollIndex + 2] : 1;
                            int next = rolls[rollIndex + 2];

                            currentFrameScore = (first * mult1) + (second * mult2) + (next * mult3);

                            Debug.Log($"フレーム{f + 1} スペア: ({first}×{mult1})+({second}×{mult2})+({next}×{mult3})={currentFrameScore}");
                        }
                    }
                    else // Open
                    {
                        // オープン: 1投目×M1 + 2投目×M2
                        int mult1 = (rollIndex < rollMultipliers.Count) ? rollMultipliers[rollIndex] : 1;
                        int mult2 = (rollIndex + 1 < rollMultipliers.Count) ? rollMultipliers[rollIndex + 1] : 1;

                        currentFrameScore = (first * mult1) + (second * mult2);

                        Debug.Log($"フレーム{f + 1} オープン: ({first}×{mult1})+({second}×{mult2})={currentFrameScore}");
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