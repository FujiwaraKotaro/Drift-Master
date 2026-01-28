using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    [SerializeField] private GameStart gameStart;

    // シーンリロード後に呼ばれるフラグ
    private static bool shouldStartGame = false;

    void Start()
    {
        // シーンがリロードされたらゲーム開始
        if (shouldStartGame)
        {
            shouldStartGame = false;
            if (gameStart != null)
            {
                gameStart.StartGame();
            }
        }
    }

    void Update()
    {
        // Rキーでシーンをリロード
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    /// <summary>
    /// シーンをリロードし、ゲームを開始する
    /// </summary>
    public void RestartScene()
    {
        shouldStartGame = true;
        GameStart.gameStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

