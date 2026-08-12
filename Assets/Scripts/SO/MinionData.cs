using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinionData", menuName = "ScriptableObject/角色数据", order = 0)]
public class MinionData : ScriptableObject
{
    //随从ID
    public string minionId;
    //随从名字
    public string minionName;
    //随从星级
    public int starLevel;
    //随从背包中图标
    public Sprite portrait;
    // 战斗中的 2D 模型
    public GameObject prefab;              

    // 基础属性
    public int maxHp;
    public int attack;
    public int defense;

    // 行动条件配置
    //public ActionTriggerType triggerType;  // 触发类型 (如: 时钟指定时刻、整除周期、奇偶数等)
    
    // 允许行动的具体时钟数值 (例如 [2, 5, 8])
    public List<int> validClockTimes;      
    // 周期触发 (例如 每 3 步行动一次)
    public int clockModulo;                
}