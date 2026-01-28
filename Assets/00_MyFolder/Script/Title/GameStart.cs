using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTweenの名前空間

public class GameStart : MonoBehaviour
{
    public static bool gameStarted = false;

    [SerializeField] private GameObject titleUI;
    [SerializeField] private GameObject MainUI;
    [SerializeField] private BowlingGameDirector gameDirector;

    public void StartGame()
    {
        gameStarted = true;

        titleUI.SetActive(!titleUI.activeSelf);
        MainUI.SetActive(!MainUI.activeSelf);

        // ゲーム開始時にSubCameraをオンにする
        gameDirector.ActivateCurrentSubCamera();
    }
}

