using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomEvent", menuName = "ScriptableObject/随机事件")]
public class RandomEventSO : ScriptableObject
{
    [Header("事件基础数据")]
    public RandomEventType randomEvent;
    public string eventTitle;
    public Sprite eventIcon;

    [Min(1)]
    public int upgradeCardCount = 2;
    [Min(1)]
    public int materialRewardCount = 2;

    [Header("事件对话")]
    public List<RandomEventDialogueData> dialogues = new List<RandomEventDialogueData>();
}