using System;
using UnityEngine;

[Serializable]
public class CardMaterialInfo
{
    // 运行时的卡牌数据
    [SerializeField] private SoupMaterialData cardMaterialData;
    public SoupMaterialData Data => cardMaterialData;
    public Intent[] Intents { get; private set; }
    public Color Color => cardMaterialData.color;
    public Sprite Icon => cardMaterialData.icon;
    public string MaterialName => cardMaterialData.materialName;
    public int MaxHealth { get; private set; }
    public bool IsAscended { get; private set; } = false;

    public CardMaterialInfo(SoupMaterialData data) { cardMaterialData = data; }

    public void Initialize()
    {
        if (IsAscended)
        {
            Intents = (Intent[])cardMaterialData.ascendedIntents.Clone();
            MaxHealth = cardMaterialData.ascendedMaxHealth;
        }
        else
        {
            Intents = (Intent[])cardMaterialData.normalIntents.Clone();
            MaxHealth = cardMaterialData.maxHealth;
        }
        
    }

    public void Ascend()
    {
        IsAscended = true;
        MaxHealth = cardMaterialData.ascendedMaxHealth;
    }
}
