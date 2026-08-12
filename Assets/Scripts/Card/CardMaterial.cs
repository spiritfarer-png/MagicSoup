using System;
using UnityEngine;

[Serializable]
public class CardMaterialInfo
{
    [SerializeField] private CardMaterialData cardMaterialData;
    public CardMaterialData Data => cardMaterialData;
    public Intent[] Intents { get; private set; }
    public Color Color => cardMaterialData.color;
    public Sprite Icon => cardMaterialData.icon;
    public string MaterialName => cardMaterialData.materialName;
    public int IniHealth { get; private set; }
    public int IniAttack { get; private set; }
    public bool IsAscended { get; private set; }

    public CardMaterialInfo(CardMaterialData data) { cardMaterialData = data; }

    public void Initialize()
    {
        IsAscended = false;
        Intents = cardMaterialData.normalIntents;
        IniHealth = cardMaterialData.iniHealth;
        IniAttack = cardMaterialData.iniattack;
    }

    public void Ascend()
    {
        IsAscended = true;
        Intents = cardMaterialData.ascendedIntents;
        IniHealth = cardMaterialData.ascendedIniHealth;
        IniAttack = cardMaterialData.ascendedIniAttack;
    }
}
