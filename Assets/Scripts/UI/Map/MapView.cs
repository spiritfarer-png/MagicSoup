using UnityEngine;

/// <summary>
/// 地图界面。
/// 当前只接收地图数据，具体节点和连线显示后续实现。
/// </summary>
public sealed class MapView : UIView
{
    [SerializeField] private RectTransform nodeRoot;
    [SerializeField] private MapNodeView nodePrefab;

    [SerializeField] private float horizontalSpacing = 150f;
    [SerializeField] private float verticalSpacing = 65f;
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
        ClearNodes();
        float mapHeight = (mapData.Floors.Count - 1) * verticalSpacing;
        foreach (MapFloorData floor in mapData.Floors)
        {
            int nodeCount = floor.Nodes.Count;
            float floorWidth = (nodeCount - 1) * horizontalSpacing;
            for (int i = 0; i < nodeCount; i++)
            {
                MapNodeData node = floor.Nodes[i];
                MapNodeView nodeView = Instantiate(nodePrefab, nodeRoot);
                RectTransform rect = nodeView.GetComponent<RectTransform>();
                float x = i * horizontalSpacing - floorWidth * 0.5f;
                float y = floor.FloorIndex * verticalSpacing - mapHeight * 0.5f;
                rect.anchoredPosition = new Vector2(x, y);
                nodeView.Initialize(node, this);
            }
        }

        Debug.Log($"地图节点显示完成，层数：{mapData.Floors.Count}");
    }

    private void ClearNodes()
    {
        for (int i = nodeRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(nodeRoot.GetChild(i).gameObject);
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