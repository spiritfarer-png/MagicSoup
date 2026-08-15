using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LootPool", menuName = "ScriptableObject/掉落池")]
public class LootPool : ScriptableObject
{
    [Header("素材池")]
    [SerializeField] private SoupMaterialData[] materialLoots;
    [Header("遗物池")]
    [SerializeField] private SoupMaterialData[] relicLoots;
    [Header("药水池")]
    [SerializeField] private PotionData[] potionLoots;

    private List<SoupMaterialData> materialLootPool;
    private List<SoupMaterialData> relicLootPool;
    private List<PotionData> potionLootPool;

    public void Initialize()
    {
        materialLootPool = new List<SoupMaterialData>(materialLoots);
        relicLootPool = new List<SoupMaterialData>(relicLoots);
        potionLootPool = new List<PotionData>(potionLoots);
    }

    public SoupMaterialData PopMaterial()
    {
        if (materialLootPool == null || materialLootPool.Count == 0)
        {
            materialLootPool = new List<SoupMaterialData>(materialLoots);
        }
        int index = Random.Range(0, materialLootPool.Count);
        var pop = materialLootPool[index];
        materialLootPool.RemoveAt(index);
        return pop;
    }

    public SoupMaterialData PopRelic()
    {
        if (relicLootPool == null || relicLootPool.Count == 0)
        {
            relicLootPool = new List<SoupMaterialData>(relicLoots);
        }
        int index = Random.Range(0, relicLootPool.Count);
        var pop = relicLootPool[index];
        relicLootPool.RemoveAt(index);
        return pop;
    }

    public PotionData PopPotion()
    {
        if (potionLootPool == null || potionLootPool.Count == 0)
        {
            potionLootPool = new List<PotionData>(potionLoots);
        }
        int index = Random.Range(0, potionLootPool.Count);
        var pop = potionLootPool[index];
        potionLootPool.RemoveAt(index);
        return pop;
    }
}
