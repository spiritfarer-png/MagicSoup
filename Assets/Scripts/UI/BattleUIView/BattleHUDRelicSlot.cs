using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleHUDRelicSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ITooltipSource
{
    private IRelic relic;

    public string GetToolTip()
    {
        return relic?.MaterialData.GetRelicTooltipText();
    }

    public void Inititalize(IRelic relic)
    {
        this.relic = relic;
        GetComponent<Image>().sprite = relic.MaterialData.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.Open<TooltipView>(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.Close<TooltipView>();
    }

    private void OnDestroy()
    {
        if (UIManager.instance == null)
        {
            return;
        }
        if (UIManager.instance.TryGet<TooltipView>(out var tipView))
        {
            if (ReferenceEquals(tipView.source, this) && tipView.IsOpen)
            {
                UIManager.instance.Close<TooltipView>();
            }
        }
    }
}
