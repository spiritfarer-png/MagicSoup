using System;
using System.Collections.Generic;

[Serializable]
public sealed class MapData
{
    public int Seed;
    /// <summary>
    /// 当前正在处理的节点 Id
    /// </summary>
    public int CurrentNodeId = -1;
    public List<MapFloorData> Floors = new List<MapFloorData>();
    public List<MapEdgeData> Edges = new List<MapEdgeData>();
}
