using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public enum MaterialArea { Inventory, Crafting,Potion }
    public enum CardArea { Inventory, Deployed }
    public static InventoryManager Instance { get; private set; }
    private const int MaterialSlotCapacity = 20;
    private const int CraftingSlotCapacity = 3;
    private const int PotionSlotCapacity = 5;
    private const int CardSlotCapacity = 10;
    private const int DeployedCardSlotCapacity = 4;

    [Header("Runtime Slots")]
    public List<MaterialSlotData> iniMaterialSlots = new List<MaterialSlotData>();
    public List<MaterialSlotData> materialSlots = new List<MaterialSlotData>();
    public List<MaterialSlotData> craftingSlots = new List<MaterialSlotData>();
    public List<MaterialSlotData> potionSlots = new List<MaterialSlotData>();
    [SerializeReference] public List<CardInfo> cardSlots = new List<CardInfo>();
    [SerializeReference] public List<CardInfo> deployedCardSlots = new List<CardInfo>();
    public int MaxMaterialSlots => MaterialSlotCapacity;
    public int MaxCraftingSlots => CraftingSlotCapacity;
    public int MaxPotionSlots => PotionSlotCapacity;
    public int MaxCardSlots => CardSlotCapacity;
    public int MaxDeployedCardSlots => DeployedCardSlotCapacity;
    public bool CanCraftCard => craftingSlots.Exists(slot => slot != null && slot.IsOccupied) && cardSlots.Exists(IsEmptyCard);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        materialSlots = new List<MaterialSlotData>(iniMaterialSlots);
        InitializeSlots();
    }

    public bool TryAddMaterial(SoupMaterialData material)
    {
        if (material is PotionData) return false;
        int index = materialSlots.FindIndex(IsEmptyMaterialSlot);
        if (index < 0) return false;
        materialSlots[index] = new MaterialSlotData() { materialData = material };
        return true;
    }
    static private bool IsEmptyMaterialSlot(MaterialSlotData slot)
    {
        if (slot == null) return true;
        if (!slot.IsOccupied) return true;
        if(slot.materialData == null) return true;
        return false;
    }

    public void SwapOrMoveMaterial(MaterialArea sourceArea, int sourceIndex, MaterialArea targetArea, int targetIndex)
    {
        if ((sourceArea == MaterialArea.Potion) != (targetArea == MaterialArea.Potion)) return;
        List<MaterialSlotData> sourceSlots = GetMaterialSlots(sourceArea);
        List<MaterialSlotData> targetSlots = GetMaterialSlots(targetArea);
        if (!IsValidIndex(sourceSlots, sourceIndex) || !IsValidIndex(targetSlots, targetIndex)) return;
        MaterialSlotData source = sourceSlots[sourceIndex];
        if (source == null || !source.IsOccupied) return;
        SoupMaterialData targetMaterial = targetSlots[targetIndex]?.materialData;
        EnsureMaterialSlot(targetSlots, targetIndex).materialData = source.materialData;
        source.materialData = targetMaterial;
    }

    public void SwapOrMoveCard(CardArea sourceArea, int sourceIndex, CardArea targetArea, int targetIndex)
    {
        List<CardInfo> sourceSlots = GetCardSlots(sourceArea);
        List<CardInfo> targetSlots = GetCardSlots(targetArea);
        if (!IsValidIndex(sourceSlots, sourceIndex) || !IsValidIndex(targetSlots, targetIndex)) return;
        CardInfo source = sourceSlots[sourceIndex];
        if (IsEmptyCard(source)) return;
        CardInfo target = targetSlots[targetIndex];
        targetSlots[targetIndex] = source;
        sourceSlots[sourceIndex] = target;
    }

    public bool TryCraftCard()
    {
        int emptyCardIndex = cardSlots.FindIndex(IsEmptyCard);
        if (emptyCardIndex < 0) return false;
        List<CardMaterialInfo> materials = new List<CardMaterialInfo>(CraftingSlotCapacity);
        for (int i = 0; i < craftingSlots.Count; i++)
        {
            SoupMaterialData data = craftingSlots[i]?.materialData;
            if (data != null) materials.Add(new CardMaterialInfo(data));
        }
        if (materials.Count == 0) return false;
        CardInfo card = new CardInfo(materials.ToArray());
        card.Initialize();
        cardSlots[emptyCardIndex] = card;
        for (int i = 0; i < craftingSlots.Count; i++) EnsureMaterialSlot(craftingSlots, i).materialData = null;
        return true;
    }

    public List<IRelic> CreateRelicSnapshot()
    {
        var relics = new List<IRelic>();
        foreach (var slot in materialSlots)
        {
            if (slot == null) continue;
            if (slot.materialData != null)
            {
                if (!slot.IsOccupied)
                {
                    continue;
                }

                if (!slot.isRelic)
                {
                    continue;
                }

                relics.Add((IRelic)slot.materialData);
            }
        }
        return relics;
    }

    private void InitializeSlots()
    {
        materialSlots ??= new List<MaterialSlotData>();
        craftingSlots ??= new List<MaterialSlotData>();
        potionSlots ??= new List<MaterialSlotData>();
        cardSlots ??= new List<CardInfo>();
        deployedCardSlots ??= new List<CardInfo>();
        ResizeMaterialSlots(materialSlots, MaterialSlotCapacity);
        ResizeMaterialSlots(craftingSlots, CraftingSlotCapacity);
        ResizeMaterialSlots(potionSlots, PotionSlotCapacity);
        ResizeCardSlots(cardSlots, CardSlotCapacity);
        ResizeCardSlots(deployedCardSlots, DeployedCardSlotCapacity);
    }

    private List<MaterialSlotData> GetMaterialSlots(MaterialArea area)
    {
        switch (area)
        {
            case MaterialArea.Inventory: return materialSlots;
            case MaterialArea.Crafting: return craftingSlots;
            case MaterialArea.Potion: return potionSlots;
            default: return null;
        }
    }
    private List<CardInfo> GetCardSlots(CardArea area) => area == CardArea.Inventory ? cardSlots : deployedCardSlots;
    private static bool IsValidIndex<T>(List<T> list, int index) => list != null && index >= 0 && index < list.Count;

    private static MaterialSlotData EnsureMaterialSlot(List<MaterialSlotData> slots, int index)
    {
        if (slots[index] == null) slots[index] = new MaterialSlotData();
        return slots[index];
    }

    private static void ResizeMaterialSlots(List<MaterialSlotData> slots, int size)
    {
        while (slots.Count < size) slots.Add(new MaterialSlotData());
        if (slots.Count > size) slots.RemoveRange(size, slots.Count - size);
        for (int i = 0; i < slots.Count; i++) EnsureMaterialSlot(slots, i);
    }

    private static void ResizeCardSlots(List<CardInfo> slots, int size)
    {
        while (slots.Count < size) slots.Add(null);
        if (slots.Count > size) slots.RemoveRange(size, slots.Count - size);
        for (int i = 0; i < slots.Count; i++) if (IsEmptyCard(slots[i])) slots[i] = null;
    }

    private static bool IsEmptyCard(CardInfo card)
    {
        if (card == null || card.materialInfoArray == null) return true;
        foreach (CardMaterialInfo material in card.materialInfoArray) if (material != null && material.Data != null) return false;
        return true;
    }
    public bool TryAddPotion(PotionData potion)
    {
        int index = potionSlots.FindIndex(IsEmptyMaterialSlot);
        if (index < 0) return false;
        potionSlots[index] = new MaterialSlotData() { materialData = potion };
        return true;
    }

    public void ConsumePotion(PotionData potion)
    {
        int index = potionSlots.FindIndex((MaterialSlotData data) => { return ReferenceEquals(data.materialData, potion); });
        if (IsValidIndex(potionSlots, index))
        {
            potionSlots[index] = new MaterialSlotData();
        }
    }
}
