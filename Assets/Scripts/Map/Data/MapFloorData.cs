using System;
using System.Collections.Generic;

/// <summary>
/// 一层地图数据
/// </summary>
[Serializable]
public sealed class MapFloorData
{
    /// <summary>
    /// 层下标
    /// </summary>
    public int FloorIndex;
    /// <summary>
    /// 当前层的所有节点
    /// </summary>
    public List<MapNodeData> Nodes = new List<MapNodeData>();
}
