using System;
using System.Collections.Generic;

/// <summary>
/// 根据配置生成地图节点以及相邻层之间的有向连接
/// </summary>
public sealed class MapGenerator
{
    public MapData Generate(MapGenerationConfig config, int seed)
    {
        MapConfigValidator.Validate(config);
        MapRandom random = new MapRandom(seed);

        MapData map = new MapData
        {
            Seed = seed,
            CurrentNodeId = -1
        };

        for (int floorIndex = 0; floorIndex < config.TotalFloorCount; floorIndex++)
        {
            MapFloorRule rule = config.FloorRules[floorIndex];
            int nodeCount = random.NextIntInclusive(rule.MinNodeCount, rule.MaxNodeCount);
            List<MapNodeType> nodeTypes = GenerateNodeTypes(rule, nodeCount, random);

            random.Shuffle(nodeTypes);

            MapFloorData floor = new MapFloorData
            {
                FloorIndex = floorIndex
            };

            for (int nodeIndex = 0; nodeIndex < nodeTypes.Count; nodeIndex++)
            {
                MapNodeType nodeType = nodeTypes[nodeIndex];

                MapNodeData node = new MapNodeData
                {
                    Id = floorIndex * 100 + nodeIndex,
                    FloorIndex = floorIndex,
                    IndexInFloor = nodeIndex,
                    NodeType = nodeType,

                    State = floorIndex == 0 ? MapNodeState.Available : MapNodeState.Locked,

                    DesiredInDegree = GetDesiredInDegree(nodeType, floorIndex, map, config, random)
                };

                floor.Nodes.Add(node);
            }

            map.Floors.Add(floor);
        }
        GenerateEdges(map, random);
        return map;
    }

    /// <summary>
    /// 生成一层的所有节点类型。
    /// </summary>
    private List<MapNodeType> GenerateNodeTypes(MapFloorRule rule, int nodeCount, MapRandom random)
    {
        List<MapNodeType> result = new List<MapNodeType>();

        foreach (MapNodeQuota quota in rule.FixedNodes)
        {
            for (int i = 0; i < quota.Count; i++)
            {
                result.Add(quota.NodeType);
            }
        }

        List<float> weights = new List<float>();

        foreach (MapNodeWeight nodeWeight in rule.RandomWeights)
        {
            weights.Add(nodeWeight.Weight);
        }

        // 按权重填满剩余位置。
        while (result.Count < nodeCount)
        {
            int selectedIndex = random.WeightedIndex(weights);

            result.Add(rule.RandomWeights[selectedIndex].NodeType);
        }

        return result;
    }

    /// <summary>
    /// 根据节点类型生成期望入度。
    /// </summary>
    private int GetDesiredInDegree(MapNodeType nodeType, int floorIndex, MapData map, MapGenerationConfig config, MapRandom random)
    {
        // 第一层没有入边。
        if (floorIndex == 0)
        {
            return 0;
        }

        int previousNodeCount = map.Floors[floorIndex - 1].Nodes.Count;

        // 宝箱和 Boss 汇聚上一层全部路线。
        if (nodeType == MapNodeType.Treasure || nodeType == MapNodeType.Boss)
        {
            return previousNodeCount;
        }

        MapInDegreeRule rule = FindInDegreeRule(config, nodeType);

        float[] weights =
        {
            rule.Degree1Weight,
            rule.Degree2Weight,
            rule.Degree3Weight
        };

        int degree = random.WeightedIndex(weights) + 1;

        return Math.Min(degree, previousNodeCount);
    }

    /// <summary>
    /// 获取对应节点类型的入度配置
    /// </summary>
    private MapInDegreeRule FindInDegreeRule(MapGenerationConfig config, MapNodeType nodeType)
    {
        foreach (MapInDegreeRule rule in config.InDegreeRules)
        {
            if (rule.NodeType == nodeType)
            {
                return rule;
            }
        }

        return null;
    }

    private void GenerateEdges(MapData map, MapRandom random)
    {
        for (int floorIndex = 0; floorIndex < map.Floors.Count - 1; floorIndex++)
        {
            List<MapNodeData> fromNodes = map.Floors[floorIndex].Nodes;
            List<MapNodeData> toNodes = map.Floors[floorIndex + 1].Nodes;

            // 每一项表示一条尚未分配来源的入边，其中保存目标节点的下标
            List<int> targetSlots = new List<int>();
            for (int targetIndex = 0; targetIndex < toNodes.Count; targetIndex++)
            {
                int degree = Math.Max(1, Math.Min(toNodes[targetIndex].DesiredInDegree, fromNodes.Count));

                for (int i = 0; i < degree; i++)
                {
                    targetSlots.Add(targetIndex);
                }
            }

            // 如果目标入度总数少于上一层节点数，增加入度，保证上一层所有节点都有出口。
            while (targetSlots.Count < fromNodes.Count)
            {
                targetSlots.Add(random.NextIntInclusive(0, toNodes.Count - 1));
            }
            random.Shuffle(targetSlots);
            HashSet<long> edgeKeys = new HashSet<long>();

            // 先让上一层每个节点各占用一个目标入度名额
            for (int sourceIndex = 0; sourceIndex < fromNodes.Count; sourceIndex++)
            {
                int targetIndex = targetSlots[sourceIndex];
                AddEdge(fromNodes[sourceIndex], toNodes[targetIndex], map.Edges, edgeKeys);
            }

            // 处理剩余的目标入度名额。
            for (int slotIndex = fromNodes.Count; slotIndex < targetSlots.Count; slotIndex++)
            {
                int targetIndex = targetSlots[slotIndex];
                MapNodeData target = toNodes[targetIndex];

                List<int> candidates = new List<int>();

                // 找出尚未连接到当前目标节点的来源节点。
                for (int sourceIndex = 0; sourceIndex < fromNodes.Count; sourceIndex++)
                {
                    long key = CreateEdgeKey(fromNodes[sourceIndex].Id, target.Id);
                    if (!edgeKeys.Contains(key))
                    {
                        candidates.Add(sourceIndex);
                    }
                }
                int selectedSourceIndex = candidates[random.NextIntInclusive(0, candidates.Count - 1)];
                AddEdge(fromNodes[selectedSourceIndex], target, map.Edges, edgeKeys);
            }
        }
    }
    private void AddEdge(MapNodeData source, MapNodeData target, List<MapEdgeData> edges, HashSet<long> edgeKeys)
    {
        long key = CreateEdgeKey(source.Id, target.Id);
        edgeKeys.Add(key);
        edges.Add(new MapEdgeData(source.Id, target.Id));
    }

    private long CreateEdgeKey(int fromNodeId, int toNodeId)
    {
        return ((long)fromNodeId << 32) | (uint)toNodeId;
    }
}