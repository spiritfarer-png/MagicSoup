using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, ITooltipSource, IPointerEnterHandler, IPointerExitHandler
{
    private string tooltip;

    public void Configure(string text) { tooltip = text; }
    public string GetToolTip() { return tooltip; }
    public void OnPointerEnter(PointerEventData eventData) { UIManager.instance.Open<TooltipView>(this); }
    public void OnPointerExit(PointerEventData eventData) { UIManager.instance.Close<TooltipView>(); }
}
