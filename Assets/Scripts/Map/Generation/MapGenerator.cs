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
        GenerateEdges(map);
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

    /// <summary>
    /// 生成层与层之间的连线，保证任意两条连线绘制时都不交叉。
    /// 原理：节点按 index 从左到右排布后，两条边 (a→b)、(c→d) 交叉当且仅当
    /// a 在 c 左侧但 b 在 d 右侧，即“来源顺序与目标顺序相反”。
    /// 因此只要构造单调分配（把边按 (来源, 目标) 排序后目标下标非递减）就必然无交叉。
    /// 几何约束：两层平行线之间的直线连线不交叉时，边构成森林，最多 n + m - 1 条
    /// （n、m 分别为上下层节点数）。期望入度超出时会裁剪入度，并优先保留
    /// 宝箱 / Boss “汇聚全部路线”的效果；同时保证上一层每个节点至少有一条出边。
    /// </summary>
    private void GenerateEdges(MapData map)
    {
        for (int floorIndex = 0; floorIndex < map.Floors.Count - 1; floorIndex++)
        {
            List<MapNodeData> fromNodes = map.Floors[floorIndex].Nodes;
            List<MapNodeData> toNodes = map.Floors[floorIndex + 1].Nodes;
            int sourceCount = fromNodes.Count;
            int targetCount = toNodes.Count;

            // 1. 目标节点期望入度：至少 1，至多 = 上一层节点数
            int[] inDegrees = new int[targetCount];
            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                inDegrees[targetIndex] = Math.Max(1, Math.Min(toNodes[targetIndex].DesiredInDegree, sourceCount));
            }

            // 2. 可行性裁剪：总边数不能超过 n + m - 1（无交叉直线连线的最大边数）。
            //    优先削减普通节点，尽量保留宝箱 / Boss 汇聚全部路线的效果。
            int total = 0;
            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                total += inDegrees[targetIndex];
            }

            int maxTotal = sourceCount + targetCount - 1;
            while (total > maxTotal)
            {
                int targetIndex = FindLargestDegradable(inDegrees, toNodes, preferNonPriority: true);
                if (targetIndex < 0)
                {
                    targetIndex = FindLargestDegradable(inDegrees, toNodes, preferNonPriority: false);
                }
                if (targetIndex < 0)
                {
                    break;
                }
                inDegrees[targetIndex]--;
                total--;
            }

            // 3. 保证上一层每个节点都有出口：总边数至少 n
            while (total < sourceCount)
            {
                int targetIndex = FindSmallestBoostable(inDegrees, sourceCount);
                if (targetIndex < 0)
                {
                    break;
                }
                inDegrees[targetIndex]++;
                total++;
            }

            // 4. 构造“目标端口序列”：目标 j 重复 inDegrees[j] 次，整体有序。
            //    之后把序列切成 n 个连续块，第 i 个块 = 第 i 个来源节点连接的全部目标，
            //    由于端口序列有序，块与块之间天然满足单调性，连线不会交叉。
            List<int> ports = new List<int>(total);
            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                for (int i = 0; i < inDegrees[targetIndex]; i++)
                {
                    ports.Add(targetIndex);
                }
            }

            // 5. 确定切分点：需要 n - 1 个。
            //    相邻相等端口之间必须切开（否则同一来源会连到同一目标两次，产生重复边）；
            //    剩余切分点均匀散布在其余位置，让各来源的出边数尽量均衡。
            int mandatoryCutCount = total - targetCount;
            int extraCutCount = (sourceCount - 1) - mandatoryCutCount;

            HashSet<int> cutPositions = new HashSet<int>();
            for (int position = 1; position < total; position++)
            {
                if (ports[position - 1] == ports[position])
                {
                    cutPositions.Add(position);
                }
            }

            List<int> optionalPositions = new List<int>();
            for (int position = 1; position < total; position++)
            {
                if (!cutPositions.Contains(position))
                {
                    optionalPositions.Add(position);
                }
            }
            for (int i = 0; i < extraCutCount; i++)
            {
                int optionalIndex = (i * optionalPositions.Count) / Math.Max(1, extraCutCount);
                cutPositions.Add(optionalPositions[optionalIndex]);
            }

            List<int> cutList = new List<int>(cutPositions);
            cutList.Sort();

            // 6. 第 i 个块 = 第 i 个来源节点，连接块内出现的全部目标
            HashSet<long> edgeKeys = new HashSet<long>();
            int blockStart = 0;
            for (int sourceIndex = 0; sourceIndex < sourceCount; sourceIndex++)
            {
                int blockEnd = sourceIndex < cutList.Count ? cutList[sourceIndex] : total;
                for (int position = blockStart; position < blockEnd; position++)
                {
                    AddEdge(fromNodes[sourceIndex], toNodes[ports[position]], map.Edges, edgeKeys);
                }
                blockStart = blockEnd;
            }
        }
    }

    /// <summary>
    /// 找出入度可削减（> 1）的目标节点，取入度最大者优先削减。
    /// preferNonPriority 为 true 时跳过宝箱 / Boss（汇聚节点），尽量保留其“汇聚全部路线”的效果。
    /// 没有可削减的节点时返回 -1。
    /// </summary>
    private int FindLargestDegradable(int[] inDegrees, List<MapNodeData> toNodes, bool preferNonPriority)
    {
        int best = -1;
        for (int targetIndex = 0; targetIndex < inDegrees.Length; targetIndex++)
        {
            if (inDegrees[targetIndex] <= 1)
            {
                continue;
            }
            if (preferNonPriority && IsConvergenceNode(toNodes[targetIndex].NodeType))
            {
                continue;
            }
            if (best < 0 || inDegrees[targetIndex] > inDegrees[best])
            {
                best = targetIndex;
            }
        }
        return best;
    }

    /// <summary>
    /// 找出入度可增加（&lt; sourceCount）的目标节点，取当前入度最小者，让多余出边均匀分担。
    /// 没有可增加的节点时返回 -1。
    /// </summary>
    private int FindSmallestBoostable(int[] inDegrees, int sourceCount)
    {
        int best = -1;
        for (int targetIndex = 0; targetIndex < inDegrees.Length; targetIndex++)
        {
            if (inDegrees[targetIndex] >= sourceCount)
            {
                continue;
            }
            if (best < 0 || inDegrees[targetIndex] < inDegrees[best])
            {
                best = targetIndex;
            }
        }
        return best;
    }

    /// <summary>
    /// 宝箱 / Boss 属于“汇聚全部路线”的节点。
    /// </summary>
    private bool IsConvergenceNode(MapNodeType nodeType)
    {
        return nodeType == MapNodeType.Treasure || nodeType == MapNodeType.Boss;
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