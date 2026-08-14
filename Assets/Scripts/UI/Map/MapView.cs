using UnityEngine;
using System.Collections.Generic;

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

    [SerializeField] private float horizontalSpacing = 150f;
    [SerializeField] private float verticalSpacing = 65f;
    [SerializeField] private float edgeThickness = 4f;
    private MapData mapData;

    protected override void OnOpen(object param)
    {
        mapData = param as MapData;
        Refresh();
    }

    protected override void OnClose()
    {
        mapData = null;
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

        float mapHeight = (mapData.Floors.Count - 1) * verticalSpacing;

        // 计算位置并生成节点。
        foreach (MapFloorData floor in mapData.Floors)
        {
            int nodeCount = floor.Nodes.Count;
            float floorWidth = (nodeCount - 1) * horizontalSpacing;
            for (int i = 0; i < nodeCount; i++)
            {
                MapNodeData node = floor.Nodes[i];
                float x = i * horizontalSpacing - floorWidth * 0.5f;
                float y = floor.FloorIndex * verticalSpacing - mapHeight * 0.5f;
                Vector2 position = new Vector2(x, y);
                nodePositions.Add(node.Id, position);
                MapNodeView nodeView = Instantiate(nodePrefab, nodeRoot);
                RectTransform nodeRect = nodeView.GetComponent<RectTransform>();
                nodeRect.anchoredPosition = position;
                nodeView.Initialize(node, this);
            }
        }
        // 根据 MapData.Edges 生成连线。
        foreach (MapEdgeData edge in mapData.Edges)
        {
            Vector2 fromPosition = nodePositions[edge.FromNodeId];
            Vector2 toPosition = nodePositions[edge.ToNodeId];
            MapEdgeView edgeView = Instantiate(edgePrefab, edgeRoot);
            edgeView.Initialize(fromPosition, toPosition, edgeThickness);
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