using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween‚Ì–¼‘O‹óŠÔ

public class GameStart : MonoBehaviour
{
    public static bool gameStarted = false;

    [SerializeField] private GameObject titleUI;
    [SerializeField] private GameObject scoreBoard;
    [SerializeField] private Camera mainCamera; // ƒJƒƒ‰QÆ

    [SerializeField] private Vector3 cameraTargetPosition = new Vector3(0, 10, -10); // ˆÚ“®æ
    [SerializeField] private Vector3 cameraTargetRotation = new Vector3(30, 0, 0);   // ‰ñ“]æiEulerŠpj

    public void StartGame()
    {
        gameStarted = true;

        titleUI.SetActive(!titleUI.activeSelf);
        scoreBoard.SetActive(!scoreBoard.activeSelf);

        if (mainCamera != null)
        {
            mainCamera.transform.DOMove(cameraTargetPosition, 1.0f);
            mainCamera.transform.DORotate(cameraTargetRotation, 1.0f); // 1•b‚©‚¯‚Ä‰ñ“]
        }
    }
}
