using System.Text;
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
        if (item == null)
            return null;

        StringBuilder builder = new StringBuilder();

        if (item is IRelic)
            builder.Append("遗物：");
        else if (item is PotionData)
            builder.Append("药水：");
        else
            builder.Append("素材：");

        builder.AppendLine(item.materialName);

        if (item.normalIntents != null)
        {
            foreach (Intent intent in item.normalIntents)
            {
                builder.AppendLine(intent.ToString());
            }
        }

        if (item is IRelic relic)
        {
            builder.Append("遗物效果：");
            builder.Append(relic.GetRelicInfo());
        }

        return builder.ToString();
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