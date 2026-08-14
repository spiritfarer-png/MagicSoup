using System;

/// <summary>
/// 一张地图中的单个节点数据
/// </summary>
[Serializable]
public sealed class MapNodeData
{
    /// <summary>
    /// 节点 Id，Id = FloorIndex * 100 + IndexInFloor
    /// </summary>
    public int Id;
    /// <summary>
    /// 楼层
    /// </summary>
    public int FloorIndex;
    /// <summary>
    /// 当前层中的下标
    /// </summary>
    public int IndexInFloor;
    public MapNodeType NodeType;
    public MapNodeState State;
    /// <summary>
    /// 生成连线时期望获得的入边数量
    /// </summary>
    public int DesiredInDegree;
}
