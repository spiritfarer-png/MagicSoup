using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 未知事件数据
/// </summary>
[CreateAssetMenu(fileName = "EventData", menuName = "ScriptableObject/事件数据", order = 0)]
public class UnknowEventData : ScriptableObject
{
    public int UnknowEventID; //事件id
    public string UnknowEventName; //事件名称
    [TextArea(3,8)]
    public string UnknowEventContent; //事件具体内容
    public Sprite UnknowEventIcon; //事件图片
    public List<UnknowEventOptionData> UnknowEventOptions; //事件选项按钮
}

/// <summary>
/// 未知事件单个选项
/// </summary>
[System.Serializable]
public class UnknowEventOptionData
{
    public int OptionID;
    public string optionName; //选项文字

    public UnknowEventActionType actionType;  //选项点击执行逻辑类型
    public int actionValue; // 逻辑数值
    public string actionText; // 道具名/音效名..

    public UnknowEventData nextEventData; //下一个事件数据
}

/// <summary>
/// 选项触发的逻辑类型
/// </summary>
[System.Serializable]
public enum UnknowEventActionType
{
    None = 0,
    //待添加

}