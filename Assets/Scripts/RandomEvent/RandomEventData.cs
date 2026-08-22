using System;
using System.Collections.Generic;

public enum RandomEventType
{
    UpgradeCardEvent,
    MaterialEvent,
    RelicEvent,
    PotionEvent
}

/// <summary>
/// 一个对话节点
/// </summary>
[Serializable]
public class RandomEventDialogueData
{
    public string dialogueText;
    public List<RandomEventOptionData> options = new List<RandomEventOptionData>();
}

/// <summary>
/// 一个对话选项
/// </summary>
[Serializable]
public class RandomEventOptionData
{
    public string optionText;
    public bool isFinalOption;
    public bool isGetReward;
    public int nextDialogueIndex;
}
