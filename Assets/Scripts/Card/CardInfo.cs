using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public CardMaterialInfo[] materialInfoArray { get => cardMaterialInfoArray; }
    public string CardName { get; private set; }
    public Sprite SoupIconOverride => soupIconOverride;
    [SerializeField] private CardMaterialInfo[] cardMaterialInfoArray;
    [SerializeField] private Sprite soupIconOverride;
    [SerializeField] private string cardNameOverride;
    public Intent[] intents
    {
        get 
        {
            List<Intent> intents = new List<Intent>();
            foreach(var materialInfo in materialInfoArray)
            {
                if (materialInfo != null)
                {
                    intents.AddRange(materialInfo.Intents);
                }
            }
            return intents.ToArray();
        } 
    }
    public bool isAscended { get; private set; } = false;
    public int MaxHealth
    {
        get
        {
            int value = 0;
            if (cardMaterialInfoArray == null) return value;
            foreach (var material in cardMaterialInfoArray)
            {
                if (material != null) value += material.MaxHealth;
            }
            return value;
        }
    }


    public CardInfo(CardMaterialInfo[] materials) { cardMaterialInfoArray = materials; }

    public void Initialize()
    {
        if (cardMaterialInfoArray == null) cardMaterialInfoArray = Array.Empty<CardMaterialInfo>();
        foreach (var material in cardMaterialInfoArray)
            if(material != null)
                material.Initialize();
        ApplyCardModifiers();

        StringBuilder sb = new StringBuilder();
        foreach (var material in cardMaterialInfoArray)
        {
            if (material != null) sb.Append(material.MaterialName);
        }

        sb.Append("瓦罐汤");
        if (string.IsNullOrEmpty(cardNameOverride))
            CardName = sb.ToString();
        else CardName = cardNameOverride;
    }

    public void Ascend()
    {
        foreach(var material in cardMaterialInfoArray)
        {
            if(material != null) material.Ascend();
        }
        ApplyCardModifiers();
        isAscended = true;
    }

    private void ApplyCardModifiers()
    {
        int allBonus = 0;
        int attackBonus = 0;
        int defenceBonus = 0;
        foreach (var material in cardMaterialInfoArray)
        {
            if (material?.Data == null) continue;
            switch (material.Data.materialID)
            {
                case "19": allBonus = 1; break;
                case "34": defenceBonus += material.IsAscended ? 2 : 1; break;
                case "35": attackBonus += material.IsAscended ? 2 : 1; break;
            }
        }
        if (allBonus == 0 && attackBonus == 0 && defenceBonus == 0) return;
        foreach (var material in cardMaterialInfoArray)
        {
            if (material == null) continue;
            for (int i = 0; i < material.Intents.Length; i++)
            {
                Intent intent = material.Intents[i];
                intent.action.value += allBonus;
                if (intent.action.type == MaterialAction.ActionType.Attack || intent.action.type == MaterialAction.ActionType.AttackAll) intent.action.value += attackBonus;
                else if (intent.action.type == MaterialAction.ActionType.Defend) intent.action.value += defenceBonus;
                material.Intents[i] = intent;
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("<b>卡牌：");
        sb.Append(CardName).AppendLine("</b>");
        foreach (var intent in intents)
        {
            sb.AppendLine(intent.ToString());
        }
        return sb.ToString();
    }
}

