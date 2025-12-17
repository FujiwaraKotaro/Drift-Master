using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween‚Ì–¼‘O‹óŠÔ

public class GameStart : MonoBehaviour
{
    public static bool gameStarted = false;

    [SerializeField] private GameObject titleUI;
    [SerializeField] private GameObject MainUI;
    [SerializeField] private GameObject subCamera;

    public void StartGame()
    {
        gameStarted = true;

        titleUI.SetActive(!titleUI.activeSelf);
        MainUI.SetActive(!MainUI.activeSelf);
        subCamera.SetActive(true);

    }
}
