using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class RandomEventRewardItemView : MonoBehaviour, ITooltipSource, IPointerEnterHandler, IPointerExitHandler
{
    private Image itemImage;
    private SoupMaterialData item;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
    }

    public void Bind(SoupMaterialData item)
    {
        this.item = item;

        itemImage.sprite = item != null ? item.icon : null;
        itemImage.preserveAspect = true;
        itemImage.gameObject.SetActive(item != null);
    }

    public string GetToolTip()
    {
        return item?.GetTooltipText();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
            UIManager.instance.Open<TooltipView>(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.Close<TooltipView>();
    }
}
