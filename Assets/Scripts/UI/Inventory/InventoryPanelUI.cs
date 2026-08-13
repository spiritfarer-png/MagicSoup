using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryPanelUI : UIView
{
    public static InventoryPanelUI Instance { get; private set; }

    [Header("Prefabs")]
    [FormerlySerializedAs("slotPrefab")]
    [SerializeField] private GameObject materialSlotPrefab;
    [SerializeField] private GameObject cardSlotPrefab;

    [Header("Grid Contents")]
    [SerializeField] private Transform materialGridContent;
    [SerializeField] private Transform craftingGridContent;
    [FormerlySerializedAs("minionGridContent")]
    [SerializeField] private Transform cardGridContent;
    [SerializeField] private Transform deployedGridContent;

    [Header("Controls")]
    [FormerlySerializedAs("btnCraft")]
    [SerializeField] private Button craftButton;
    [FormerlySerializedAs("btnClose")]
    [SerializeField] private Button closeButton;

    private readonly List<MaterialSlotUI> materialSlotViews = new List<MaterialSlotUI>();
    private readonly List<MaterialSlotUI> craftingSlotViews = new List<MaterialSlotUI>();
    private readonly List<CardSlotUI> cardSlotViews = new List<CardSlotUI>();
    private readonly List<CardSlotUI> deployedSlotViews = new List<CardSlotUI>();
    private bool initialized;

    private void Awake() { Instance = this; }

    protected override void OnOpen(object param)
    {
        if (!initialized)
        {
            if (InventoryManager.Instance == null) { Debug.LogError("InventoryManager is missing."); return; }
            SpawnAllSlots();
            if (craftButton != null) craftButton.onClick.AddListener(CraftCard);
            if (closeButton != null) closeButton.onClick.AddListener(CloseSelf);
            initialized = true;
        }
        RefreshAllUI();
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void RefreshAllUI()
    {
        InventoryManager manager = InventoryManager.Instance;
        if (manager == null) return;
        for (int i = 0; i < materialSlotViews.Count; i++) materialSlotViews[i].Bind(manager.materialSlots[i], i, InventoryManager.MaterialArea.Inventory);
        for (int i = 0; i < craftingSlotViews.Count; i++) craftingSlotViews[i].Bind(manager.craftingSlots[i], i, InventoryManager.MaterialArea.Crafting);
        for (int i = 0; i < cardSlotViews.Count; i++) cardSlotViews[i].Bind(manager.cardSlots[i], i, InventoryManager.CardArea.Inventory);
        for (int i = 0; i < deployedSlotViews.Count; i++) deployedSlotViews[i].Bind(manager.deployedCardSlots[i], i, InventoryManager.CardArea.Deployed);
        if (craftButton != null) craftButton.interactable = manager.CanCraftCard;
    }

    private void SpawnAllSlots()
    {
        InventoryManager manager = InventoryManager.Instance;
        ClearChildren(materialGridContent);
        ClearChildren(craftingGridContent);
        ClearChildren(cardGridContent);
        ClearChildren(deployedGridContent);
        for (int i = 0; i < manager.MaxMaterialSlots; i++) materialSlotViews.Add(CreateMaterialSlot(materialGridContent));
        for (int i = 0; i < manager.MaxCraftingSlots; i++) craftingSlotViews.Add(CreateMaterialSlot(craftingGridContent));
        for (int i = 0; i < manager.MaxCardSlots; i++) cardSlotViews.Add(CreateCardSlot(cardGridContent));
        for (int i = 0; i < manager.MaxDeployedCardSlots; i++) deployedSlotViews.Add(CreateCardSlot(deployedGridContent));
    }

    private MaterialSlotUI CreateMaterialSlot(Transform parent)
    {
        GameObject slot = Instantiate(materialSlotPrefab, parent);
        slot.name = "MaterialSlot";
        MaterialSlotUI view = slot.GetComponent<MaterialSlotUI>();
        if (view == null) view = slot.AddComponent<MaterialSlotUI>();
        return view;
    }

    private CardSlotUI CreateCardSlot(Transform parent)
    {
        if (cardSlotPrefab != null)
        {
            GameObject instance = Instantiate(cardSlotPrefab, parent);
            CardSlotUI configured = instance.GetComponent<CardSlotUI>();
            if (configured != null) return configured;
        }

        GameObject slotObject = CreateUIObject("CardSlot", parent, typeof(Image));
        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(100f, 100f);
        Image slotImage = slotObject.GetComponent<Image>();
        slotImage.color = new Color(0.13f, 0.09f, 0.06f, 0.8f);

        GameObject empty = CreateUIObject("Empty", slotObject.transform, typeof(Image));
        Stretch(empty.GetComponent<RectTransform>(), 5f);
        empty.GetComponent<Image>().color = new Color(0.45f, 0.34f, 0.22f, 0.45f);

        GameObject card = CreateUIObject("CardView", slotObject.transform, typeof(Image), typeof(CanvasGroup), typeof(CardViewUI));
        Stretch(card.GetComponent<RectTransform>(), 3f);
        Image background = card.GetComponent<Image>();
        background.color = new Color(0.56f, 0.32f, 0.14f, 1f);

        Image soup = CreateImage("Soup", card.transform, new Vector2(0.5f, 0.5f), new Vector2(78f, 54f), new Vector2(0f, 2f), new Color(1f, 1f, 1f, 0.9f));
        Image[] icons = new Image[3];
        for (int i = 0; i < icons.Length; i++) icons[i] = CreateImage($"Material{i + 1}", card.transform, new Vector2(0.5f, 0.5f), new Vector2(58f, 58f), new Vector2((i - 1) * 8f, 4f + i * 2f), Color.white);
        TMP_Text name = CreateText("Name", card.transform, new Vector2(0.5f, 1f), new Vector2(94f, 25f), new Vector2(0f, -15f), 10f, TextAlignmentOptions.Center);
        TMP_Text defence = CreateText("Defence", card.transform, new Vector2(0f, 0f), new Vector2(28f, 22f), new Vector2(16f, 13f), 14f, TextAlignmentOptions.Center);
        TMP_Text health = CreateText("Health", card.transform, new Vector2(1f, 0f), new Vector2(28f, 22f), new Vector2(-16f, 13f), 14f, TextAlignmentOptions.Center);
        CardViewUI cardView = card.GetComponent<CardViewUI>();
        cardView.Configure(background, soup, icons, name, health, defence);
        CardSlotUI cardSlot = slotObject.AddComponent<CardSlotUI>();
        cardSlot.Configure(cardView, empty);
        return cardSlot;
    }

    private void CraftCard()
    {
        InventoryManager.Instance.TryCraftCard();
        RefreshAllUI();
    }

    private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.layer = 5;
        result.transform.SetParent(parent, false);
        foreach (System.Type component in components) if (component != typeof(RectTransform)) result.AddComponent(component);
        return result;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, Color color)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }
}
