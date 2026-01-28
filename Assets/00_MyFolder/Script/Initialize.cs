using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    void Start()
    {
        // ブラウザのデバイスピクセル比（DPI）を1に固定する設定を適用
        // これにより、高解像度モニターでクッキリと表示される
        QualitySettings.resolutionScalingFixedDPIFactor = 1.0f;
    }
}