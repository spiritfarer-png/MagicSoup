using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LootPool", menuName = "ScriptableObject/掉落池")]
public class LootPool : ScriptableObject
{
    [Header("素材池")]
    [SerializeField] private CardMaterialData[] materialLoots;
    [Header("遗物池")]
    [SerializeField] private CardMaterialData[] relicLoots;

    private List<CardMaterialData> materialLootPool;
    private List<CardMaterialData> relicLootPool;

    public void Initialize()
    {
        materialLootPool = new List<CardMaterialData>(materialLoots);
        relicLootPool = new List<CardMaterialData>(relicLoots);
    }

    public CardMaterialData PopMaterial()
    {
        if (materialLootPool == null || materialLootPool.Count == 0)
        {
            materialLootPool = new List<CardMaterialData>(materialLoots);
        }
        int index = Random.Range(0, materialLootPool.Count);
        var pop = materialLootPool[index];
        materialLootPool.RemoveAt(index);
        return pop;
    }

    public CardMaterialData PopRelic()
    {
        if (relicLootPool == null || relicLootPool.Count == 0)
        {
            relicLootPool = new List<CardMaterialData>(relicLoots);
        }
        int index = Random.Range(0, relicLootPool.Count);
        var pop = relicLootPool[index];
        relicLootPool.RemoveAt(index);
        return pop;
    }
}
