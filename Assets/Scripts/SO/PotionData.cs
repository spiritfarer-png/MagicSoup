using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "potionMgr", menuName = "ScriptableObject/药水数据", order = 0)]
public class PotionData : ScriptableObject
{
    //药水唯一ID
    public string potionID;
    //药水名称
    public string potionName;
    //药水图标
    public Sprite icon;
    //药水持有量
    public int potionCount;
}
