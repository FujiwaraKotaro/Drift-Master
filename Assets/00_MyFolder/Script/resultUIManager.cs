using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class resultUIManager: MonoBehaviour
{
    [SerializeField] private Vector2 centerPosition = Vector2.zero; // 中央座標
    [SerializeField] private Vector2 enlargedSize = new Vector2(600, 400); // 拡大後のサイズ
    [SerializeField] private float duration = 2.0f; // アニメーション時間
    [SerializeField] private GameObject scoreBoard;
    [SerializeField] private GameObject resultUI;
    [SerializeField] private GameObject mainUI;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = scoreBoard.GetComponent<RectTransform>();
    }


    // ゲームオーバー時に呼び出す
    public void ShowResultUI()
    {
        // scoreBoardを1階層上に移動
        Transform currentParent = scoreBoard.transform.parent;
        if (currentParent != null && currentParent.parent != null)
        {
            scoreBoard.transform.SetParent(currentParent.parent, true); // worldPositionStayをtrueに設定
        }

        mainUI.SetActive(false);

        // 位置を中央に移動
        rectTransform.DOAnchorPos(centerPosition, duration).SetEase(Ease.OutCubic);
        // サイズを大きくする
        rectTransform.DOSizeDelta(enlargedSize, duration).SetEase(Ease.OutCubic);
        resultUI.SetActive(true);
    }
}