using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class resultUIManager: MonoBehaviour
{
    [SerializeField] private Vector2 centerPosition = Vector2.zero; // 画面中央座標
    [SerializeField] private Vector2 enlargedSize = new Vector2(600, 400); // 拡大後のサイズ
    [SerializeField] private float duration = 2.0f; // アニメーション時間
    [SerializeField] private GameObject scoreBoard;
    [SerializeField] private GameObject resultUI;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = scoreBoard.GetComponent<RectTransform>();
    }

    // ゲームオーバー時に呼び出す
    public void ShowResultUI()
    {
        // 位置を中央に移動
        rectTransform.DOAnchorPos(centerPosition, duration).SetEase(Ease.OutCubic);
        // サイズを大きく
        rectTransform.DOSizeDelta(enlargedSize, duration).SetEase(Ease.OutCubic);
        resultUI.SetActive(true);

    }
}