using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{

    [Header("UI 组件引用")]
    public Image imgIcon;           // 图标 Image
    public Text txtCount;           // 数量 Text (素材用)
    public Text txtLevel;           // 星级 Text
    public GameObject emptyBg;      // 空格子背景/遮罩 (可选)

    // 格子所属区域类型
    public enum SlotType
    {
        MaterialArea,   // 素材区
        CraftingArea,   // 合成区
        MinionArea ,     // 随从区
        DeployedArea    // 上阵区
    }

    public SlotType currentSlotType;
    public int slotIndex; // 当前格子对应 List 数据里的索引(下标)

    // 拖拽过程中生成的图标跟随镜像
    private static GameObject dragIconObj;
    private Canvas parentCanvas;

    private void Awake()
    {
        // 提前获取父级 Canvas 引用
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // 1. 渲染素材数据 (用于素材区 / 合成区)
    public void UpdateMaterialSlot(SingleBeiBao slotData, int index, SlotType type)
    {
        this.slotIndex = index;
        this.currentSlotType = type;

        // 如果格子中有有效素材
        if (slotData != null && slotData.materialData != null && slotData.count > 0)
        {
            imgIcon.gameObject.SetActive(true);
            imgIcon.sprite = slotData.materialData.icon;

            if (txtCount != null)
            {
                txtCount.gameObject.SetActive(true);
                // 数量大于 1 才显示数字，等于 1 不显示或只显示 1
                txtCount.text = slotData.count > 1 ? slotData.count.ToString() : "";
            }

            if (txtLevel != null)
            {
                txtLevel.gameObject.SetActive(true);
                txtLevel.text = $"{slotData.materialData.Level}星";
            }

            if (emptyBg != null) emptyBg.SetActive(false);
        }
        else
        {
            ClearSlot();
        }
    }

    // 2. 渲染随从数据 (用于随从区)
    public void UpdateMinionSlot(MinionData minionData, int index, SlotType type)
    {
        this.slotIndex = index;
        this.currentSlotType = type;

        if (minionData != null)
        {
            imgIcon.gameObject.SetActive(true);
            imgIcon.sprite = minionData.portrait;

            if (txtCount != null) txtCount.gameObject.SetActive(false); // 随从没有堆叠数量

            if (txtLevel != null)
            {
                txtLevel.gameObject.SetActive(true);
                txtLevel.text = $"{minionData.starLevel}星";
            }

            if (emptyBg != null) emptyBg.SetActive(false);
        }
        else
        {
            ClearSlot();
        }
    }

    // 清空格子显示
    public void ClearSlot()
    {
        if (imgIcon != null) imgIcon.gameObject.SetActive(false);
        if (txtCount != null) txtCount.gameObject.SetActive(false);
        if (txtLevel != null) txtLevel.gameObject.SetActive(false);
        if (emptyBg != null) emptyBg.SetActive(true);
    }

    // 响应玩家点击格子
    public void OnPointerClick(PointerEventData eventData)
    {
        // 点击时，把自己的“区域类型”和“下标”通知给 UI 总控管理器
        BeiBaoPanelUI.Instance.OnSlotClicked(currentSlotType, slotIndex);
    }

    //  UGUI 拖拽接口实现 
    // 1. 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 只有随从区和上阵区有图标时才能拖拽
        if (currentSlotType != SlotType.MinionArea && currentSlotType != SlotType.DeployedArea) return;
        if (imgIcon == null || !imgIcon.gameObject.activeSelf) return;

        // 创建临时生成的跟随 UI 图标 (拖到 Canvas 最外层避免被父级 Mask 遮挡)
       
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        Canvas rootCanvas = parentCanvas.rootCanvas;

        dragIconObj = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        dragIconObj.transform.SetParent(rootCanvas.transform, false);
        dragIconObj.transform.SetAsLastSibling(); // 保证最上层渲染

        Image dragImage = dragIconObj.GetComponent<Image>();
        dragImage.sprite = imgIcon.sprite;
        dragImage.raycastTarget = false;

        //RectTransform dragRect = dragIconObj.GetComponent<RectTransform>();
        //RectTransform srcRect = imgIcon.GetComponent<RectTransform>();
        //dragRect.sizeDelta = srcRect.sizeDelta; // 强行同步源图标大小
        //dragRect.localScale = Vector3.one;

        // 3. 穿透设置
        CanvasGroup cg = dragIconObj.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // 4. 初始化一次坐标
        UpdateDragPosition(eventData);


        // 拖拽时原格子图标半透明
        Color color = imgIcon.color;
        color.a = 0.5f;
        imgIcon.color = color;
    }

    // 2. 拖拽进行中 (图标跟随鼠标移动)
    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Break();
        if (dragIconObj != null)
        {
            if (dragIconObj != null)
            {
                UpdateDragPosition(eventData);
            }
           // dragIconObj.transform.position = eventData.position;
        }
    }

    // 3. 结束拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconObj != null)
        {
            Destroy(dragIconObj);
            dragIconObj = null;
        }

        // 恢复原格子透明度
        if (imgIcon != null)
        {
            Color color = imgIcon.color;
            color.a = 1.0f;
            imgIcon.color = color;
        }
    }

    // 4. 当有物体拖拽并释放到了【当前格子】上
    public void OnDrop(PointerEventData eventData)
    {
        // 取得拖拽源头（起点格子）的 ItemSlotUI 脚本
        ItemSlotUI sourceSlot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ItemSlotUI>() : null;

        if (sourceSlot != null)
        {
            // 调用数据层，对【源格子】与【当前目标格子】进行数据移动/互换
            BeiBaoMgr.Instance.SwapOrMoveMinion(
                sourceSlot.currentSlotType,
                sourceSlot.slotIndex,
                this.currentSlotType,
                this.slotIndex
            );

            // 重新刷新全界面 UI
            BeiBaoPanelUI.Instance.RefreshAllUI();
        }
    }

    
    private void UpdateDragPosition(PointerEventData eventData)//游戏场景鼠标坐标
    {
        //出现位置应该显示的游戏位置
        Canvas rootCanvas = parentCanvas.rootCanvas;

        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Overlay 模式直接赋屏幕坐标
            dragIconObj.transform.position = eventData.position;
        }
        else
        {
            RectTransform canvasRect = parentCanvas.rootCanvas.GetComponent<RectTransform>();

            Vector2 localPos;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                parentCanvas.worldCamera,
                out localPos))
            {
                dragIconObj.GetComponent<RectTransform>().localPosition = localPos;
            }
        }
    }
}

