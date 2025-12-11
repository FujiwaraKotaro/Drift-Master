using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    void Start()
    {
        // ブラウザのデバイスピクセル比（DPI）を取得して適用
        // これにより、高解像度モニターでもクッキリ表示されます
        QualitySettings.resolutionScalingFixedDPIFactor = 1.0f;
    }
}
