using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BowlingScoreManager : MonoBehaviour
{
    [SerializeField] private int totalFrames = 10;
    public int TotalFrames => totalFrames;

    [SerializeField] private BowlingUIManager uiManager;
    [SerializeField] private BowlingPinManager pinManager;

    // 内部の「投球」データ (Single Source of Truth)
    private List<int> rolls = new List<int>();
    public List<int> Rolls => rolls;

    // Directorに「次にピンをどう操作すべきか」を伝えるための列挙型
    public enum NextPinAction
    {
        None,           // 何もしない（ゲームオーバーなど）
        ResetAll,       // 全ピンを復活（フレーム開始、ストライクなど）
        RemoveFallen    // 倒れたピンを除去（2投目の前など）
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

        Debug.Log($"記録: ピン数={pinsDown}");

        uiManager.UpdateScoreBoard(this); // データ更新後すぐにUIを反映
    }

    // フレーム数を動的に設定するメソッド
    public void SetTotalFrames(int frames)
    {
        if (frames > 0)
        {
            totalFrames = frames;
            Debug.Log($"フレーム数を{frames}に変更しました");
        }
    }

    // --- 計算ロジック ---

    // 現在の状況から「ゲームオーバーかどうか」「次はどうすべきか」を算出
    public GameStatus CheckGameStatus()
    {
        int rollIndex = 0;
        int frame = 1;

        // 1～(最終フレーム-1)のシミュレーション
        for (; frame < totalFrames; frame++)
        {
            if (rollIndex >= rolls.Count)
            {
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }

            int first = rolls[rollIndex];

            if (first == pinManager.GetFrameMaxPinCount(frame-1)) // Strike
            {
                rollIndex++;
                if (rollIndex >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };
            }
            else // Open or Spare
            {
                if (rollIndex + 1 >= rolls.Count)
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };

                rollIndex += 2; // 2投
            }
        }

        // 最終フレームの処理
        if (frame == totalFrames)
        {   
            if (rollIndex >= rolls.Count)
                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

            int throwsInFinalFrame = rolls.Count - rollIndex;

            // 1投目を投げた
            if (throwsInFinalFrame == 1)
            {
                int first = rolls[rollIndex];
                return new GameStatus
                {
                    IsGameOver = false,
                    NextAction = (first == pinManager.GetFrameMaxPinCount(frame-1)) ? NextPinAction.ResetAll : NextPinAction.RemoveFallen
                };
            }
            // 2投目を投げた
            else if (throwsInFinalFrame == 2)
            {
                int first = rolls[rollIndex];
                int second = rolls[rollIndex + 1];

                // 終了条件: オープンフレームなら終了
                if (first + second < pinManager.GetFrameMaxPinCount(frame-1) && first != pinManager.GetFrameMaxPinCount(frame-1))
                    return new GameStatus { IsGameOver = true, NextAction = NextPinAction.None };

                // 3投目がある場合
                if (first + second == pinManager.GetFrameMaxPinCount(frame-1))
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                if (first == pinManager.GetFrameMaxPinCount(frame-1) && second == pinManager.GetFrameMaxPinCount(frame-1))
                    return new GameStatus { IsGameOver = false, NextAction = NextPinAction.ResetAll };

                return new GameStatus { IsGameOver = false, NextAction = NextPinAction.RemoveFallen };
            }
            // 3投目を投げた
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

                // 最終フレームは全ての投球の合計
                for (int i = 0; rollIndex + i < rolls.Count && i < 3; i++)
                {
                    sum += rolls[rollIndex + i];
                    throws++;
                }

                bool isFrameFinished = false;
                if (throws == 3) isFrameFinished = true;
                else if (throws == 2 && rolls[rollIndex] + (throws > 1 ? rolls[rollIndex + 1] : 0) < pinManager.GetFrameMaxPinCount(f) && rolls[rollIndex] != pinManager.GetFrameMaxPinCount(f))
                    isFrameFinished = true;

                if (isFrameFinished)
                {
                    currentFrameScore = sum;
                    Debug.Log($"フレーム{f + 1} 最終フレーム: 合計={sum}, 投球数={throws}");
                }
                advance = throws;
            }
            else // 通常フレーム
            {
                if (rolls[rollIndex] == pinManager.GetFrameMaxPinCount(f)) // Strike
                {
                    if (rollIndex + 2 < rolls.Count)
                    {
                        // ストライク: maxPins + 次の2投の合計
                        currentFrameScore = pinManager.GetFrameMaxPinCount(f) + rolls[rollIndex + 1] + rolls[rollIndex + 2];

                        Debug.Log($"フレーム{f + 1} ストライク: {pinManager.GetFrameMaxPinCount(f)}+{rolls[rollIndex + 1]}+{rolls[rollIndex + 2]}={currentFrameScore}");
                    }
                    advance = 1;
                }
                else if (rollIndex + 1 < rolls.Count) // Spare or Open
                {
                    int first = rolls[rollIndex];
                    int second = rolls[rollIndex + 1];

                    if (first + second == pinManager.GetFrameMaxPinCount(f)) // Spare
                    {
                        if (rollIndex + 2 < rolls.Count)
                        {
                            // スペア: 10 + 次の1投
                            currentFrameScore = pinManager.GetFrameMaxPinCount(f) + rolls[rollIndex + 2];

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