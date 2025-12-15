using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DebugCoM : MonoBehaviour
{
    // 重心の描画サイズ
    [SerializeField] private float gizmoSize = 0.1f;
    // 重心の色
    [SerializeField] private Color gizmoColor = Color.red;

    void OnDrawGizmosSelected()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            Gizmos.color = gizmoColor;
            // worldCenterOfMass はワールド座標での重心位置です
            Gizmos.DrawSphere(rb.worldCenterOfMass, gizmoSize);
        }
    }
}