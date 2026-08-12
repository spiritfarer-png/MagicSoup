using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardMaterialData", menuName = "ScriptableObject/素材数据", order = 0)]
public class CardMaterialData : ScriptableObject
{
    public string materialID;
    public string materialName;
    public Sprite icon;
    public Color color;

    public int iniattack;
    public int iniHealth;

    public int ascendedIniAttack;
    public int ascendedIniHealth;

    /// <summary>
    /// 意图列表
    /// </summary>
    public Intent[] normalIntents;

    /// <summary>
    /// 敲牌后的意图列表
    /// </summary>
    public Intent[] ascendedIntents;
}

[Serializable]
public struct Intent
{
    public IntentConditionType condition;
    public int conditionValue;
    public MaterialAction action;

}

public enum IntentConditionType
{
    // 无条件触发
    None,
    Equal,
    NotEqual,
    Less,
    Greater,
}

[Serializable]
public struct MaterialAction
{
    public enum ActionType
    {
        Attack,
        Defend,
        Heal,
    }

    public ActionType type;
    public int value;
}
