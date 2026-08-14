using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 未知事件数据
/// </summary>
public class UnknowEventData
{
    public Text UnknowEventName; //事件名称
    public Text UnknowEventIntroduct; //事件介绍
    public Image UnknowEventIcon; //事件图片
    public List<UnknowEventOptionData> UnknowEventOptions; //事件选项按钮
}

/// <summary>
/// 未知事件单个选项
/// </summary>
public class UnknowEventOptionData
{
    public string optionName; //选项文字
    public Action optionClicked;  //选项点击事件
}