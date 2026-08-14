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
    public int iniHealth
    {
        get
        {
            int value = 0;
            if (cardMaterialInfoArray == null) return value;
            foreach (var material in cardMaterialInfoArray)
            {
                if (material != null) value += material.IniHealth;
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
        isAscended = true;
    }
}

