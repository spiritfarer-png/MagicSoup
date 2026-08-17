using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardViewUI : MonoBehaviour,ITooltipSource,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image soup;
    [SerializeField] private Image[] materialIcons;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text defenceText;
    private CardInfo cardInfo;

    public void Bind(CardInfo cardInfo)
    {
        gameObject.SetActive(cardInfo != null);
        this.cardInfo = cardInfo;
        if (cardInfo == null) return;
        CardMaterialInfo[] materials = cardInfo.materialInfoArray;
        Color soupColor = Color.clear;
        int materialCount = materials?.Length ?? 0;
        for (int i = 0; i < materialIcons.Length; i++)
        {
            bool visible = i < materialCount && materials[i] != null;
            materialIcons[i].gameObject.SetActive(visible);
            materialIcons[i].sprite = visible ? materials[i].Icon : null;
            if (visible) soupColor += materials[i].Color;
        }
        if (materialCount > 0) { soupColor /= materialCount; soupColor.a = 1f; }
        if (soup != null) soup.color = soupColor;
        if (nameText != null) nameText.text = cardInfo.CardName;
        if (healthText != null) healthText.text = cardInfo.MaxHealth.ToString();
        if (defenceText != null) defenceText.text = "";
    }

    public void Configure(Image background, Image soupImage, Image[] icons, TMP_Text cardName, TMP_Text health, TMP_Text defence)
    {
        cardBackground = background;
        soup = soupImage;
        materialIcons = icons;
        nameText = cardName;
        healthText = health;
        defenceText = defence;
    }

    public string GetToolTip()
    {
        if(cardInfo != null)
        {
            return cardInfo.ToString();
        }
        return null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.Open<TooltipView>(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.Close<TooltipView>();
    }
}
