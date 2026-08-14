using UnityEngine;

/// <summary>
/// 地图中两个节点之间的一条 UI 连线。
/// </summary>
public sealed class MapEdgeView : MonoBehaviour
{
    [SerializeField]
    private RectTransform lineRect;

    public void Initialize(Vector2 fromPosition, Vector2 toPosition, float thickness)
    {
        Vector2 direction = toPosition - fromPosition;
        float distance = direction.magnitude;
        lineRect.anchoredPosition = (fromPosition + toPosition) * 0.5f;
        lineRect.sizeDelta = new Vector2(distance, thickness);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}