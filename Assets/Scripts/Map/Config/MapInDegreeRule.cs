using System;
using UnityEngine;

/// <summary>
/// 一种节点类型对不同入度数量的倾向
/// </summary>
[Serializable]
public sealed class MapInDegreeRule
{
    public MapNodeType NodeType;

    /// <summary>
    /// 抽到一个入边的权重
    /// </summary>
    [Min(0f)]
    public float Degree1Weight = 1f;

    /// <summary>
    /// 抽到两个入边的权重
    /// </summary>
    [Min(0f)]
    public float Degree2Weight = 1f;

    /// <summary>
    /// 抽到三个入边的权重
    /// </summary>
    [Min(0f)]
    public float Degree3Weight = 1f;
}