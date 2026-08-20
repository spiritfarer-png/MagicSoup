using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图中两个节点之间的一条 UI 连线。
/// 继承自 MaskableGraphic，用一段二次贝塞尔曲线绘制平滑弧线：
/// 控制点取连线中点 + 垂直于连线方向的偏移（弧高 = 线段长度 × arcRatio，上限 maxArc）。
/// 偏移方向统一取“连线方向的左侧法线”，所有弧线同侧弯曲：
/// 共享端点的连线（从同一点扇出 / 汇聚到同一点）会保持左右顺序，不会互相交叉。
/// 弧高默认约线长的 15%、上限 18 像素，视觉上自然柔和，不会过于歪斜。
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class MapEdgeView : MaskableGraphic
{
    [Header("贝塞尔弧线参数")]
    [SerializeField, Range(0f, 0.5f)] private float arcRatio = 0.15f; // 控制点偏移 = 线段长度 × 该比例，0 = 直线
    [SerializeField, Range(0f, 60f)] private float maxArc = 18f;      // 控制点偏移上限（像素）
    [SerializeField, Range(4, 64)] private int segmentCount = 20;     // 贝塞尔曲线细分段数（越大越平滑）

    private Vector2 fromPosition;
    private Vector2 toPosition;
    private float thickness = 4f;
    private bool hasData;

    private Vector2[] points;
    private Vector2[] normals;
    private Vector2 curveCenter;
    private Vector2 curveSize;

    /// <summary>
    /// 由 MapView 在实例化后调用，传入两端节点坐标（与 nodeRoot 同一坐标系）。
    /// </summary>
    public void Initialize(Vector2 fromPosition, Vector2 toPosition, float thickness, int seed = 0)
    {
        this.fromPosition = fromPosition;
        this.toPosition = toPosition;
        this.thickness = thickness;
        hasData = true;
        // 在重建回调之外撑开 RectTransform（在 OnPopulateMesh 里改会触发
        // “graphic rebuild loop”报错）
        UpdateRectToFit();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!ComputePath())
        {
            return;
        }

        Color32 color32 = color;
        int count = points.Length;
        for (int i = 0; i < count; i++)
        {
            Vector2 p = points[i] - curveCenter;
            vh.AddVert(p + normals[i], color32, Vector2.zero);
            vh.AddVert(p - normals[i], color32, Vector2.zero);
        }
        for (int i = 0; i < count - 1; i++)
        {
            int baseIndex = i * 2;
            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex + 1, baseIndex + 3, baseIndex + 2);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (hasData)
        {
            UpdateRectToFit();
        }
        SetVerticesDirty();
    }
#endif

    /// <summary>
    /// 撑开 RectTransform 使其正好包住曲线，避免网格超出矩形被画布裁剪。
    /// 只能在非重建回调（Initialize / OnValidate）中调用。
    /// </summary>
    private void UpdateRectToFit()
    {
        if (!ComputePath())
        {
            return;
        }
        if (rectTransform.anchoredPosition != curveCenter)
        {
            rectTransform.anchoredPosition = curveCenter;
        }
        if (rectTransform.sizeDelta != curveSize)
        {
            rectTransform.sizeDelta = curveSize;
        }
    }

    /// <summary>
    /// 生成二次贝塞尔弧线路径：采样点 + 每点法线 + 包围盒中心/尺寸。
    /// 路径无效（无数据 / 两点重合 / 线宽为 0）时返回 false。
    /// </summary>
    private bool ComputePath()
    {
        if (!hasData || thickness <= 0f)
        {
            return false;
        }

        Vector2 delta = toPosition - fromPosition;
        float distance = delta.magnitude;
        if (distance < 0.01f)
        {
            return false;
        }

        Vector2 direction = delta / distance;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x); // 统一朝连线方向左侧弯曲

        // 控制点 = 中点 + 垂直偏移；弧高不超过 maxArc，避免过于歪斜
        float bulge = Mathf.Min(distance * arcRatio, maxArc);
        Vector2 control = (fromPosition + toPosition) * 0.5f + perpendicular * bulge;

        // 细分采样二次贝塞尔曲线：B(t) = (1-t)^2 P0 + 2(1-t)t C + t^2 P2
        int segments = Mathf.Max(4, segmentCount);
        if (points == null || points.Length != segments + 1)
        {
            points = new Vector2[segments + 1];
        }
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float inv = 1f - t;
            points[i] = inv * inv * fromPosition + 2f * inv * t * control + t * t * toPosition;
        }

        // 切线（中心差分）→ 法线；端点用单向差分，退化时回退到连线方向
        float halfThickness = thickness * 0.5f;
        if (normals == null || normals.Length != segments + 1)
        {
            normals = new Vector2[segments + 1];
        }
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i <= segments; i++)
        {
            Vector2 tangent;
            if (i == 0)
            {
                tangent = points[1] - points[0];
            }
            else if (i == segments)
            {
                tangent = points[segments] - points[segments - 1];
            }
            else
            {
                tangent = points[i + 1] - points[i - 1];
            }
            if (tangent.sqrMagnitude < 0.0001f)
            {
                tangent = direction;
            }
            normals[i] = new Vector2(-tangent.y, tangent.x).normalized * halfThickness;

            min = Vector2.Min(min, points[i] - normals[i]);
            max = Vector2.Max(max, points[i] + normals[i]);
        }

        curveCenter = (min + max) * 0.5f;
        curveSize = max - min;
        return true;
    }
}
