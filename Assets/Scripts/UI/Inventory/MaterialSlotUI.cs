using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MaterialSlotUI : MonoBehaviour, 
    IBeginDragHandler, 
    IDragHandler, 
    IEndDragHandler, 
    IDropHandler,
    IPointerExitHandler,
    IPointerEnterHandler,
    ITooltipSource
{
    private static MaterialSlotUI activeDragSource;
    [FormerlySerializedAs("imgIcon")]
    [SerializeField] private Image materialView;
    [FormerlySerializedAs("emptyBg")]
    [SerializeField] private GameObject emptyBackground;
    public InventoryManager.MaterialArea Area { get; private set; }
    public int SlotIndex { get; private set; }
    private MaterialSlotData slotData;

    private RectTransform draggedRect;
    private Transform originalParent;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchoredPosition;

    private void Awake()
    {
        if (materialView == null)
        {
            Transform icon = transform.Find("Icon");
            materialView = icon != null ? icon.GetComponent<Image>() : null;
        }
        if (materialView != null)
        {
            canvasGroup = materialView.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = materialView.gameObject.AddComponent<CanvasGroup>();
        }
        Canvas canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
        Transform count = transform.Find("CountText");
        Transform level = transform.Find("LevelText");
        if (count != null) count.gameObject.SetActive(false);
        if (level != null) level.gameObject.SetActive(false);
    }

    public void Bind(MaterialSlotData data, int index, InventoryManager.MaterialArea area)
    {
        slotData = data;
        SlotIndex = index;
        Area = area;
        bool occupied = data != null && data.IsOccupied;
        if (materialView != null)
        {
            materialView.sprite = occupied ? data.materialData.icon : null;
            materialView.gameObject.SetActive(occupied);
            materialView.preserveAspect = true;
        }
        if (emptyBackground != null) emptyBackground.SetActive(!occupied);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (materialView == null || !materialView.gameObject.activeSelf || rootCanvas == null) return;
        draggedRect = materialView.rectTransform;
        activeDragSource = this;
        originalParent = draggedRect.parent;
        originalAnchorMin = draggedRect.anchorMin;
        originalAnchorMax = draggedRect.anchorMax;
        originalPivot = draggedRect.pivot;
        originalSizeDelta = draggedRect.sizeDelta;
        originalAnchoredPosition = draggedRect.anchoredPosition;
        draggedRect.SetParent(rootCanvas.transform, true);
        draggedRect.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedRect != null) UpdateDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RestoreDraggedView();
        activeDragSource = null;
        InventoryPanelUI.Instance?.RefreshAllUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        MaterialSlotUI source = activeDragSource;
        if (source == null) return;
        InventoryManager.Instance.SwapOrMoveMaterial(source.Area, source.SlotIndex, Area, SlotIndex);
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, camera, out Vector2 position)) draggedRect.anchoredPosition = position;
    }

    private void RestoreDraggedView()
    {
        if (draggedRect == null) return;
        draggedRect.SetParent(originalParent, false);
        draggedRect.anchorMin = originalAnchorMin;
        draggedRect.anchorMax = originalAnchorMax;
        draggedRect.pivot = originalPivot;
        draggedRect.sizeDelta = originalSizeDelta;
        draggedRect.anchoredPosition = originalAnchoredPosition;
        canvasGroup.blocksRaycasts = true;
        draggedRect = null;
        originalParent = null;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.Close<TooltipView>();

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.Open<TooltipView>(this);

    }

    public string GetToolTip()
    {
        if (slotData != null && slotData.IsOccupied) return slotData.materialData.GetTooltipText();
        return Area switch
        {
            InventoryManager.MaterialArea.Inventory => "<b>素材栏位</b>\n素材可用于合成卡牌，卡牌效果和数值为素材的叠加，有些素材在背包中也能发挥效果。",
            InventoryManager.MaterialArea.Crafting => "<b>合成栏位</b>\n将素材放置于此来合成卡牌。",
            InventoryManager.MaterialArea.Potion => "<b>药水栏位</b>\n在战斗中使用药水可以获得强化或伤害敌人。",
            _ => null
        };
    }
}
