using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, ITooltipSource
{
    private static CardSlotUI activeDragSource;
    public InventoryManager.CardArea Area { get; private set; }
    public int SlotIndex { get; private set; }
    private CardViewUI cardView;
    private CardInfo cardInfo;
    private GameObject emptyBackground;
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
        Canvas canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
    }

    public void Configure(CardViewUI view, GameObject emptyView)
    {
        cardView = view;
        emptyBackground = emptyView;
        canvasGroup = cardView.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(CardInfo data, int index, InventoryManager.CardArea area)
    {
        cardInfo = data;
        SlotIndex = index;
        Area = area;
        if (cardView != null) cardView.Bind(data);
        if (emptyBackground != null) emptyBackground.SetActive(data == null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardView == null || !cardView.gameObject.activeSelf || rootCanvas == null) return;
        draggedRect = cardView.transform as RectTransform;
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
        CardSlotUI source = activeDragSource;
        if (source == null) return;
        InventoryManager.Instance.SwapOrMoveCard(source.Area, source.SlotIndex, Area, SlotIndex);
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

    public string GetToolTip()
    {
        if (cardInfo != null) return cardInfo.ToString();
        return Area == InventoryManager.CardArea.Inventory
            ? "<b>卡牌备战席</b>\n用不到的卡牌暂存于此。"
            : "<b>卡牌部署栏</b>\n将卡牌放置于此，这些卡牌将会参与战斗。";
    }

    public void OnPointerEnter(PointerEventData eventData) { UIManager.instance.Open<TooltipView>(this); }
    public void OnPointerExit(PointerEventData eventData) { UIManager.instance.Close<TooltipView>(); }
}
