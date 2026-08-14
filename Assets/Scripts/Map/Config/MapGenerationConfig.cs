using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationConfig",menuName = "Map/Map Generation Config",order = 0)]
public sealed class MapGenerationConfig : ScriptableObject
{
    [Min(1)]
    public int TotalFloorCount = 9;

    /// <summary>
    /// 每一层的节点生成规则
    /// </summary>
    public List<MapFloorRule> FloorRules = new List<MapFloorRule>();

    /// <summary>
    /// 不同节点类型的入度权重
    /// </summary>
    public List<MapInDegreeRule> InDegreeRules = new List<MapInDegreeRule>();
}
