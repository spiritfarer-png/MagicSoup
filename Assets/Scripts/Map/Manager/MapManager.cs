using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public sealed class MapManager : MonoBehaviour
{

    public static MapManager Instance { get; private set; }
    [SerializeField]
    private MapGenerationConfig generationConfig;
    public MapData CurrentMap { get; private set; }
    private readonly Dictionary<int, MapNodeData> nodeById = new Dictionary<int, MapNodeData>();

    private void Awake()
    {

        Instance = this;
    }

    private void Start()
    {
        StartNewMap((int)(Time.realtimeSinceStartup * 1000));
        UIManager.instance.Open<InventoryPanelUI>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public MapData GenerateNewMap(int seed)
    {
        MapGenerator generator = new MapGenerator();
        CurrentMap = generator.Generate(generationConfig, seed);
        BuildNodeIndex();
        return CurrentMap;
    }

    /// <summary>
    /// 获取指定节点
    /// </summary>
    public MapNodeData GetNode(int nodeId)
    {
        nodeById.TryGetValue(nodeId, out MapNodeData node);
        return node;
    }

    /// <summary>
    /// 玩家选择一个当前可用节点
    /// </summary>
    public bool SelectNode(int nodeId)
    {
        if (CurrentMap == null || CurrentMap.CurrentNodeId >= 0)
        {
            return false;
        }
        MapNodeData node = GetNode(nodeId);
        if (node == null || node.State != MapNodeState.Available)
        {
            return false;
        }
        node.State = MapNodeState.Current;
        CurrentMap.CurrentNodeId = node.Id;

        return true;
    }

    /// <summary>
    /// 完成当前节点，并解锁它连接的下一层节点
    /// </summary>
    public void CompleteCurrentNode()
    {
        if (CurrentMap == null || CurrentMap.CurrentNodeId < 0)
        {
            return;
        }

        MapNodeData current = GetNode(CurrentMap.CurrentNodeId);

        // 当前层其他路线不能再选择。
        foreach (MapFloorData floor in CurrentMap.Floors)
        {
            foreach (MapNodeData node in floor.Nodes)
            {
                if (node.State == MapNodeState.Available)
                {
                    node.State = MapNodeState.Locked;
                }
            }
        }
        current.State = MapNodeState.Completed;
        foreach (MapEdgeData edge in CurrentMap.Edges)
        {
            if (edge.FromNodeId == current.Id)
            {
                GetNode(edge.ToNodeId).State = MapNodeState.Available;
            }
        }
        CurrentMap.CurrentNodeId = -1;
    }

    /// <summary>
    /// 取消当前节点的选择，将其状态恢复为可选状态，并将当前节点 ID 重置为 -1。
    /// </summary>
    public void CancelCurrentNode()
    {
        if (CurrentMap == null || CurrentMap.CurrentNodeId < 0)
        {
            return;
        }

        MapNodeData current = GetNode(CurrentMap.CurrentNodeId);

        current.State = MapNodeState.Available;
        CurrentMap.CurrentNodeId = -1;
    }


    /// <summary>
    /// 构建节点索引
    /// </summary>
    private void BuildNodeIndex()
    {
        nodeById.Clear();
        foreach (MapFloorData floor in CurrentMap.Floors)
        {
            foreach (MapNodeData node in floor.Nodes)
            {
                nodeById.Add(node.Id, node);
            }
        }
    }

    public void OpenMap()
    {
        if (CurrentMap == null)
        {
            return;
        }
        UIManager.instance.Open<MapView>(CurrentMap);
    }

    public void StartNewMap(int seed)
    {
        GenerateNewMap(seed);
        OpenMap();
    }

    public void OnTreasureRoomFinished()
    {
        if (CurrentMap == null || CurrentMap.CurrentNodeId < 0)
        {
            return;
        }

        MapNodeData node = GetNode(CurrentMap.CurrentNodeId);

        if (node == null || node.NodeType != MapNodeType.Treasure)
        {
            Debug.LogError("当前地图节点不是 Treasure 节点。");
            return;
        }
        CompleteCurrentNode();
        UIManager.instance.Close<TreasureView>();
        OpenMap();
    }

    public void OnRandomEventFinished()
    {
        if (CurrentMap == null || CurrentMap.CurrentNodeId < 0)
        {
            return;
        }

        MapNodeData currentNode = GetNode(CurrentMap.CurrentNodeId);

        if (currentNode == null || currentNode.NodeType != MapNodeType.RandomEvent)
        {
            Debug.LogError("当前地图节点不是 RandomEvent 节点。");

            return;
        }
        CompleteCurrentNode();
        UIManager.instance.Close<RandomEventView>();
        OpenMap();
    }

    // public void OnNodeClicked(int nodeId)
    // {
    //     if (!SelectNode(nodeId))
    //     {
    //         return;
    //     }

    //     MapNodeData node = GetNode(nodeId);
    //     Debug.Log($"进入地图节点：{node.Id}，" + $"节点类型：{node.NodeType}");

    //     switch (node.NodeType)
    //     {
    //         case MapNodeType.NormalBattle:
    //         case MapNodeType.EliteBattle:
    //         case MapNodeType.Boss:
    //             UIManager.instance.Open<BattleView>(node);
    //             break;

    //         case MapNodeType.RandomEvent:
    //             UIManager.instance.Open<EventView>(node);
    //             break;

    //         case MapNodeType.Treasure:
    //             UIManager.instance.Open<TreasureView>(node);
    //             break;
    //     }
    // }

    // 测试代码
    public void OnNodeClicked(int nodeId)
    {
        if (!SelectNode(nodeId))
        {
            return;
        }

        MapNodeData node = GetNode(nodeId);
        Debug.Log($"进入地图节点：{node.Id}，" + $"节点类型：{node.NodeType}");
        UIManager.instance.Close<MapView>();
        switch (node.NodeType)
        {
            case MapNodeType.Treasure:
                UIManager.instance.Open<TreasureView>(node);
                break;
            case MapNodeType.RandomEvent:
                UIManager.instance.Open<RandomEventView>(node);
                break;
            case MapNodeType.NormalBattle:
                UIManager.instance.Close<MapView>();
                BattleManager.instance.InitializeBattle();
                break;
            case MapNodeType.EliteBattle:
                UIManager.instance.Close<MapView>();
                BattleManager.instance.InitializeBattle(BattleType.Elite);
                break;
            case MapNodeType.Boss:
                UIManager.instance.Close<MapView>();
                BattleManager.instance.InitializeBattle(BattleType.Boss);
                break;
            default:
                UIManager.instance.Close<MapView>();
                BattleManager.instance.InitializeBattle();
                break;
        }
    }

}
