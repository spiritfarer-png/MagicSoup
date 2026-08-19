using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleWinLootSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource, IPointerClickHandler
{
    private SoupMaterialData material;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI tmp;

    public void Initialize(SoupMaterialData material)
    {
        this.material = material;
        icon.sprite = material.icon;
        tmp.text = material.materialName;
    }


    public string GetToolTip()
    {
        return material?.GetTooltipText();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.Open<TooltipView>(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.Close<TooltipView>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == 0)
        {
            if(material is PotionData)
            {
                InventoryManager.Instance.TryAddPotion((PotionData)material);
            }
            else
            {
                InventoryManager.Instance.TryAddMaterial(material);
            }
            Destroy(gameObject);
            UIManager.instance.Close<TooltipView>();
        }
    }
}
