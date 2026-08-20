using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 地图界面。
/// 当前只接收地图数据，具体节点和连线显示后续实现。
/// </summary>
public sealed class MapView : UIView
{
    [SerializeField] private RectTransform nodeRoot;
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private RectTransform edgeRoot;
    [SerializeField] private MapEdgeView edgePrefab;

    // 布局已改为按画布尺寸自动适配（见 Refresh），以下字段仅作参考，不再参与计算
    [SerializeField] private float horizontalSpacing = 150f;
    [SerializeField] private float verticalSpacing = 65f;
    [SerializeField] private float edgeThickness = 4f;

    [SerializeField] private Button inventoryButton;

    private MapData mapData;

    protected override void OnOpen(object param)
    {
        AudioManager.Instance.PlayBGM("地图音乐");
        mapData = param as MapData;
        inventoryButton.onClick.AddListener(()=> {
            AudioManager.Instance.PlaySFX("点击音效");
            UIManager.instance.Open<InventoryPanelUI>();
            });
        Refresh();
        PlayOpenTween();
    }

    protected override void OnClose()
    {
        mapData = null;
        inventoryButton.onClick.RemoveAllListeners();
    }

    public void Refresh()
    {
        if (mapData == null)
        {
            return;
        }

        ClearChildren(nodeRoot);
        ClearChildren(edgeRoot);

        Dictionary<int, Vector2> nodePositions = new Dictionary<int, Vector2>();

        // —— 自动适配画布尺寸：地图铺满 nodeRoot 可用区域，不溢出、不缩成一团 ——
        // 横向左右各留 300（右侧为右下角背包按钮让位，任何分辨率下节点都不会与按钮重叠）；
        // 纵向上下各留 50（覆盖节点半尺寸 + 少量留白，保持高度铺满）
        const float marginX = 300f;
        const float marginY = 50f;
        float availWidth = Mathf.Max(1f, nodeRoot.rect.width - marginX * 2f);
        float availHeight = Mathf.Max(1f, nodeRoot.rect.height - marginY * 2f);

        int maxNodeCount = 1;
        foreach (MapFloorData floor in mapData.Floors)
        {
            maxNodeCount = Mathf.Max(maxNodeCount, floor.Nodes.Count);
        }

        // 横向按最宽层铺满，纵向铺满全部层；节点数少的层自动居中
        float spacingX = availWidth / Mathf.Max(1, maxNodeCount - 1);
        float spacingY = availHeight / Mathf.Max(1, mapData.Floors.Count - 1);

        float mapHeight = (mapData.Floors.Count - 1) * spacingY;

        // 计算位置并生成节点。
        foreach (MapFloorData floor in mapData.Floors)
        {
            int nodeCount = floor.Nodes.Count;
            float floorWidth = (nodeCount - 1) * spacingX;
            for (int i = 0; i < nodeCount; i++)
            {
                MapNodeData node = floor.Nodes[i];
                float x = i * spacingX - floorWidth * 0.5f;
                float y = floor.FloorIndex * spacingY - mapHeight * 0.5f;
                Vector2 position = new Vector2(x, y);
                nodePositions.Add(node.Id, position);
                MapNodeView nodeView = Instantiate(nodePrefab, nodeRoot);
                RectTransform nodeRect = nodeView.GetComponent<RectTransform>();
                nodeRect.anchoredPosition = position;
                nodeView.Initialize(node, this);
            }
        }
        // 根据 MapData.Edges 生成连线。
        for (int edgeIndex = 0; edgeIndex < mapData.Edges.Count; edgeIndex++)
        {
            MapEdgeData edge = mapData.Edges[edgeIndex];
            Vector2 fromPosition = nodePositions[edge.FromNodeId];
            Vector2 toPosition = nodePositions[edge.ToNodeId];
            MapEdgeView edgeView = Instantiate(edgePrefab, edgeRoot);
            edgeView.Initialize(fromPosition, toPosition, edgeThickness, edgeIndex);
        }

        Debug.Log($"地图显示完成，节点层数：{mapData.Floors.Count}，" + $"连线数：{mapData.Edges.Count}");
    }


    private void ClearChildren(RectTransform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 后续由地图节点按钮调用。
    /// </summary>
    public void OnNodeClicked(int nodeId)
    {
        MapManager.Instance.OnNodeClicked(nodeId);
    }
}