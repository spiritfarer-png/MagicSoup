using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当前层必须生成的固定类型节点及其数量
/// </summary>
[Serializable]
public sealed class MapNodeQuota
{
    public MapNodeType NodeType;
    [Min(0)]
    public int Count;
}

/// <summary>
/// 当前层剩余节点使用的类型权重，及其权重
/// </summary>
[Serializable]
public sealed class MapNodeWeight
{
    public MapNodeType NodeType;

    [Min(0f)]
    public float Weight = 1f;
}

[Serializable]
public sealed class MapFloorRule
{
    /// <summary>
    /// 层下标
    /// </summary>
    [Min(0)]
    public int FloorIndex;

    /// <summary>
    /// 当前层最少节点数
    /// </summary>
    [Min(1)]
    public int MinNodeCount = 3;

    /// <summary>
    /// 当前层最多节点数
    /// </summary>
    [Min(1)]
    public int MaxNodeCount = 4;
    /// <summary>
    /// 当前层必须生成的节点
    /// </summary>
    public List<MapNodeQuota> FixedNodes = new List<MapNodeQuota>();
    /// <summary>
    /// 填充剩余节点时使用的随机权重
    /// </summary>
    public List<MapNodeWeight> RandomWeights = new List<MapNodeWeight>();
}
