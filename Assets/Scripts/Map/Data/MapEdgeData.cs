using System;

[Serializable]
public struct MapEdgeData
{
    /// <summary>
    /// 起始节点 Id
    /// </summary>
    public int FromNodeId;
    /// <summary>
    /// 终点节点 Id
    /// </summary>
    public int ToNodeId;

    public MapEdgeData(int fromNodeId, int toNodeId)
    {
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
    }
}