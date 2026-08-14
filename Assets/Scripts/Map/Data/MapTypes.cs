/// <summary>
/// 地图节点类型
/// NormalBattle: 普通战斗
/// EliteBattle: 精英战斗
/// RandomEvent: 随机事件
/// Treasure: 宝箱房
/// Boss: Boss 战斗
/// </summary>
public enum MapNodeType
{
    NormalBattle,
    EliteBattle,
    RandomEvent,
    Treasure,
    Boss
}

/// <summary>
/// 地图节点在当前游戏流程中的状态
/// Locked: 当前无法到达
/// Available: 当前可以到达
/// Current: 当前所在节点
/// Completed: 已完成节点
/// </summary>
public enum MapNodeState
{
    Locked,
    Available,
    Current,
    Completed
}