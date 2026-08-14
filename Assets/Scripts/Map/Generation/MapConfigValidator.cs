using System;
using System.Collections.Generic;

/// <summary>
/// 验证地图生成配置是否可以安全地交给生成器使用。
/// </summary>
public static class MapConfigValidator
{
    /// <summary>
    /// 验证配置。配置不合法时抛出带有明确原因的异常。
    /// </summary>
    public static void Validate(MapGenerationConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "地图生成配置不能为空。");
        }

        if (config.TotalFloorCount <= 0)
        {
            throw new InvalidOperationException("地图总层数必须大于 0。");
        }

        ValidateFloorRules(config);
        ValidateInDegreeRules(config);
    }

    private static void ValidateFloorRules(MapGenerationConfig config)
    {
        if (config.FloorRules == null)
        {
            throw new InvalidOperationException("FloorRules 不能为 null。");
        }

        if (config.FloorRules.Count != config.TotalFloorCount)
        {
            throw new InvalidOperationException($"FloorRules 数量必须等于地图总层数。" + $"当前规则数：{config.FloorRules.Count}，" + $"总层数：{config.TotalFloorCount}。");
        }

        HashSet<int> floorIndices = new HashSet<int>();

        foreach (MapFloorRule rule in config.FloorRules)
        {
            if (rule == null)
            {
                throw new InvalidOperationException("FloorRules 中存在空规则。");
            }

            if (rule.FloorIndex < 0 || rule.FloorIndex >= config.TotalFloorCount)
            {
                throw new InvalidOperationException($"层下标 {rule.FloorIndex} 超出合法范围。" + $"合法范围为 0～{config.TotalFloorCount - 1}。");
            }

            if (!floorIndices.Add(rule.FloorIndex))
            {
                throw new InvalidOperationException($"层下标 {rule.FloorIndex} 重复配置。");
            }

            ValidateFloorRule(rule);
        }

        for (int floorIndex = 0; floorIndex < config.TotalFloorCount; floorIndex++)
        {
            if (!floorIndices.Contains(floorIndex))
            {
                throw new InvalidOperationException($"缺少第 {floorIndex} 层的生成规则。");
            }
        }
    }

    private static void ValidateFloorRule(MapFloorRule rule)
    {
        if (rule.MinNodeCount <= 0)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层的最少节点数必须大于 0。");
        }

        if (rule.MaxNodeCount < rule.MinNodeCount)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层的最大节点数不能小于最少节点数。");
        }

        if (rule.FixedNodes == null)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层的 FixedNodes 不能为 null。");
        }

        if (rule.RandomWeights == null)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层的 RandomWeights 不能为 null。");
        }

        int fixedNodeCount = ValidateFixedNodes(rule);
        ValidateRandomWeights(rule, fixedNodeCount);
    }

    private static int ValidateFixedNodes(MapFloorRule rule)
    {
        int fixedNodeCount = 0;
        HashSet<MapNodeType> nodeTypes = new HashSet<MapNodeType>();

        foreach (MapNodeQuota quota in rule.FixedNodes)
        {
            if (quota == null)
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层的 FixedNodes 中存在空配置。");
            }

            if (!Enum.IsDefined(typeof(MapNodeType), quota.NodeType))
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层包含非法固定节点类型。");
            }

            if (!nodeTypes.Add(quota.NodeType))
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层重复配置固定节点类型 " + $"{quota.NodeType}。");
            }

            if (quota.Count < 0)
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层的固定节点数量不能小于 0。");
            }

            fixedNodeCount += quota.Count;
        }

        if (fixedNodeCount > rule.MinNodeCount)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层的固定节点总数 " + $"{fixedNodeCount} 超过最少节点数 {rule.MinNodeCount}。" + "这会导致随机到较小节点总数时无法生成。");
        }

        return fixedNodeCount;
    }

    private static void ValidateRandomWeights(MapFloorRule rule, int fixedNodeCount)
    {
        HashSet<MapNodeType> nodeTypes = new HashSet<MapNodeType>();

        float totalWeight = 0f;

        foreach (MapNodeWeight nodeWeight in rule.RandomWeights)
        {
            if (nodeWeight == null)
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层的 RandomWeights 中存在空配置。");
            }

            if (!Enum.IsDefined(typeof(MapNodeType), nodeWeight.NodeType))
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层包含非法随机节点类型。");
            }

            if (!nodeTypes.Add(nodeWeight.NodeType))
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层重复配置随机节点类型 " + $"{nodeWeight.NodeType}。");
            }

            if (nodeWeight.Weight < 0f)
            {
                throw new InvalidOperationException($"第 {rule.FloorIndex} 层的节点权重不能小于 0。");
            }

            totalWeight += nodeWeight.Weight;
        }

        bool needsRandomNodes = fixedNodeCount < rule.MaxNodeCount;

        if (needsRandomNodes && totalWeight <= 0f)
        {
            throw new InvalidOperationException($"第 {rule.FloorIndex} 层可能需要随机填充节点，" + "但随机权重总和不大于 0。");
        }
    }

    private static void ValidateInDegreeRules(MapGenerationConfig config)
    {
        if (config.InDegreeRules == null)
        {
            throw new InvalidOperationException("InDegreeRules 不能为 null。");
        }

        HashSet<MapNodeType> nodeTypes = new HashSet<MapNodeType>();

        foreach (MapInDegreeRule rule in config.InDegreeRules)
        {
            if (rule == null)
            {
                throw new InvalidOperationException("InDegreeRules 中存在空配置。");
            }

            if (!Enum.IsDefined(typeof(MapNodeType), rule.NodeType))
            {
                throw new InvalidOperationException("InDegreeRules 中包含非法节点类型。");
            }

            if (!nodeTypes.Add(rule.NodeType))
            {
                throw new InvalidOperationException($"节点类型 {rule.NodeType} 的入度规则重复配置。");
            }

            if (rule.Degree1Weight < 0f || rule.Degree2Weight < 0f || rule.Degree3Weight < 0f)
            {
                throw new InvalidOperationException($"节点类型 {rule.NodeType} 的入度权重不能小于 0。");
            }

            float totalWeight = rule.Degree1Weight + rule.Degree2Weight + rule.Degree3Weight;

            if (totalWeight <= 0f)
            {
                throw new InvalidOperationException($"节点类型 {rule.NodeType} 的入度权重总和必须大于 0。");
            }
        }

        RequireInDegreeRule(nodeTypes, MapNodeType.NormalBattle);
        RequireInDegreeRule(nodeTypes, MapNodeType.EliteBattle);
        RequireInDegreeRule(nodeTypes, MapNodeType.RandomEvent);
    }

    private static void RequireInDegreeRule(HashSet<MapNodeType> configuredTypes, MapNodeType requiredType)
    {
        if (!configuredTypes.Contains(requiredType))
        {
            throw new InvalidOperationException($"缺少节点类型 {requiredType} 的入度规则。");
        }
    }
}