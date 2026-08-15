using System;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "SoupMaterialData", menuName = "ScriptableObject/素材数据", order = 0)]
public class SoupMaterialData : ScriptableObject
{
    public string materialID;
    public string materialName;
    public Sprite icon;
    public Color color;

    public int iniHealth;

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

    public bool Match(int time)
    {
        switch (condition)
        {
            case IntentConditionType.None: return true;
            case IntentConditionType.Less: return time < conditionValue;
            case IntentConditionType.Greater: return time > conditionValue;
            case IntentConditionType.Equal: return time == conditionValue;
            case IntentConditionType.NotEqual: return time != conditionValue;
            default: return false;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("意图: ");
        switch (condition)
        {
            case IntentConditionType.None: sb.Append("无条件"); break;
            case IntentConditionType.Less: sb.Append("计时器小于"); break;
            case IntentConditionType.Greater: sb.Append("计时器大于"); break;
            case IntentConditionType.NotEqual: sb.Append("计时器不等于"); break;
            case IntentConditionType.Equal: sb.Append("计时器等于"); break;
        }
        if(condition != IntentConditionType.None)
        {
            sb.Append(conditionValue);
            sb.Append("时 ");
        }

        switch (action.type)
        {
            case MaterialAction.ActionType.Heal: sb.Append(string.Format("造成{0}治疗",action.value)); break;
            case MaterialAction.ActionType.Defend: sb.Append(string.Format("造成{0}防御", action.value)); break;
            case MaterialAction.ActionType.Attack: sb.Append(string.Format("造成{0}伤害", action.value)); break;
        }

        return sb.ToString();
    }
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
