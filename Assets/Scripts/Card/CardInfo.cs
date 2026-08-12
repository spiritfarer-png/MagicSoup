using System;
using System.Text;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public CardMaterialInfo[] materialInfoArray { get => cardMaterialInfoArray; }
    public string CardName { get; private set; }
    [SerializeField] private CardMaterialInfo[] cardMaterialInfoArray;

    public bool isAscended { get; private set; } = false;
    public int iniHealth
    {
        get
        {
            int value = 0;
            foreach(var material in cardMaterialInfoArray)
            {
                value += material.IniHealth;
            }
            return value;
        }
    }

    public int iniAttack
    {
        get
        {
            int value = 0;
            foreach (var material in cardMaterialInfoArray)
            {
                value += material.IniAttack;
            }
            return value;
        }
    }

    public int currentHealth;
    public int currentAttack;

    public CardInfo(CardMaterialInfo[] materials) { cardMaterialInfoArray = materials; }

    public void Initialize()
    {
        if (cardMaterialInfoArray == null) cardMaterialInfoArray = Array.Empty<CardMaterialInfo>();
        foreach (var material in cardMaterialInfoArray)
            material.Initialize();

        currentAttack = iniAttack;
        currentHealth = iniHealth;

        StringBuilder sb = new StringBuilder();
        foreach (var material in cardMaterialInfoArray)
            sb.Append(material.MaterialName);

        sb.Append("瓦罐汤");
        CardName = sb.ToString();
    }

    public void Ascend()
    {
        foreach(var material in cardMaterialInfoArray)
        {
            material.Ascend();
        }
        isAscended = true;
    }
}

