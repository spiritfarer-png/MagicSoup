using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public enum MaterialArea { Inventory, Crafting }
    public enum CardArea { Inventory, Deployed }
    public static InventoryManager Instance { get; private set; }
    private const int MaterialSlotCapacity = 20;
    private const int CraftingSlotCapacity = 3;
    private const int CardSlotCapacity = 10;
    private const int DeployedCardSlotCapacity = 4;

    [Header("Runtime Slots")]
    public List<MaterialSlotData> materialSlots = new List<MaterialSlotData>();
    public List<MaterialSlotData> craftingSlots = new List<MaterialSlotData>();
    public List<CardInfo> cardSlots = new List<CardInfo>();
    public List<CardInfo> deployedCardSlots = new List<CardInfo>();
    public int MaxMaterialSlots => MaterialSlotCapacity;
    public int MaxCraftingSlots => CraftingSlotCapacity;
    public int MaxCardSlots => CardSlotCapacity;
    public int MaxDeployedCardSlots => DeployedCardSlotCapacity;
    public bool CanCraftCard => craftingSlots.Exists(slot => slot != null && slot.IsOccupied) && cardSlots.Exists(card => card == null);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSlots();
    }

    public void SwapOrMoveMaterial(MaterialArea sourceArea, int sourceIndex, MaterialArea targetArea, int targetIndex)
    {
        List<MaterialSlotData> sourceSlots = GetMaterialSlots(sourceArea);
        List<MaterialSlotData> targetSlots = GetMaterialSlots(targetArea);
        if (!IsValidIndex(sourceSlots, sourceIndex) || !IsValidIndex(targetSlots, targetIndex)) return;
        MaterialSlotData source = sourceSlots[sourceIndex];
        if (source == null || !source.IsOccupied) return;
        CardMaterialData targetMaterial = targetSlots[targetIndex]?.materialData;
        EnsureMaterialSlot(targetSlots, targetIndex).materialData = source.materialData;
        source.materialData = targetMaterial;
    }

    public void SwapOrMoveCard(CardArea sourceArea, int sourceIndex, CardArea targetArea, int targetIndex)
    {
        List<CardInfo> sourceSlots = GetCardSlots(sourceArea);
        List<CardInfo> targetSlots = GetCardSlots(targetArea);
        if (!IsValidIndex(sourceSlots, sourceIndex) || !IsValidIndex(targetSlots, targetIndex)) return;
        CardInfo source = sourceSlots[sourceIndex];
        if (source == null) return;
        CardInfo target = targetSlots[targetIndex];
        targetSlots[targetIndex] = source;
        sourceSlots[sourceIndex] = target;
    }

    public bool TryCraftCard()
    {
        int emptyCardIndex = cardSlots.FindIndex(card => card == null);
        if (emptyCardIndex < 0) return false;
        List<CardMaterialInfo> materials = new List<CardMaterialInfo>(CraftingSlotCapacity);
        for (int i = 0; i < craftingSlots.Count; i++)
        {
            CardMaterialData data = craftingSlots[i]?.materialData;
            if (data != null) materials.Add(new CardMaterialInfo(data));
        }
        if (materials.Count == 0) return false;
        CardInfo card = new CardInfo(materials.ToArray());
        card.Initialize();
        cardSlots[emptyCardIndex] = card;
        for (int i = 0; i < craftingSlots.Count; i++) EnsureMaterialSlot(craftingSlots, i).materialData = null;
        return true;
    }

    private void InitializeSlots()
    {
        materialSlots ??= new List<MaterialSlotData>();
        craftingSlots ??= new List<MaterialSlotData>();
        cardSlots ??= new List<CardInfo>();
        deployedCardSlots ??= new List<CardInfo>();
        ResizeMaterialSlots(materialSlots, MaterialSlotCapacity);
        ResizeMaterialSlots(craftingSlots, CraftingSlotCapacity);
        ResizeCardSlots(cardSlots, CardSlotCapacity);
        ResizeCardSlots(deployedCardSlots, DeployedCardSlotCapacity);
    }

    private List<MaterialSlotData> GetMaterialSlots(MaterialArea area) => area == MaterialArea.Inventory ? materialSlots : craftingSlots;
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
    }
}
