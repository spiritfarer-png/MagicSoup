using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleHUDPotionSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipSource,IPointerClickHandler
{
    private PotionData potion;

    public string GetToolTip()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(potion.materialName);
        return sb.ToString();
    }

    public void Inititalize(PotionData potion)
    {
        this.potion = potion;
        GetComponent<Image>().sprite = potion.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(BattleManager.instance.Phase == BattlePhase.PlayerDecision)
        {
            BattleManager.instance.UsePotion(potion);
            Destroy(gameObject);
        }
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
