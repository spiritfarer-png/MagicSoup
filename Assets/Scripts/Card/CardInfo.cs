using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public CardMaterialInfo[] materialInfoArray { get => cardMaterialInfoArray; }
    public string CardName { get; private set; }
    [SerializeField] private CardMaterialInfo[] cardMaterialInfoArray;
    [SerializeField] private Sprite soupIconOverride;
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
        CardName = sb.ToString();
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
        bool hasSalt = false;
        foreach (var material in cardMaterialInfoArray)
        {
            if (material?.Data?.materialID == "19") hasSalt = true;
        }
        if (!hasSalt) return;
        foreach (var material in cardMaterialInfoArray)
        {
            if (material == null) continue;
            for (int i = 0; i < material.Intents.Length; i++)
            {
                Intent intent = material.Intents[i];
                intent.action.value++;
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

