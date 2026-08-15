using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardEntity : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler,ITooltipSource
{
    public static event Action<CardEntity> OnCardEntityDead;

    public CardInfo CardInfo 
    {
        get { return cardInfo; }
        private set { cardInfo = value; }
    }
    [SerializeField] CardInfo cardInfo;
    public BattleCardState cardState;
    [SerializeField] private Image soup;
    [SerializeField] private Image[] materialIcons;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI defenceText;
    [SerializeField] private TextMeshProUGUI nameText;

    public void Initialize(CardInfo cardInfo,bool isEnemy)
    {
        CardInfo = cardInfo;
        CardInfo.Initialize();
        InitializeBattleState(cardInfo, isEnemy);
        InitializeVisual();
    }

    public void InitializeBattleState(CardInfo cardInfo,bool isEnemy)
    {
        cardState = new BattleCardState(cardInfo,isEnemy);
    }

    public void InitializeVisual()
    {
        var materials = CardInfo.materialInfoArray;
        if (materials == null || materials.Length == 0) return;

        Color soupColor = Color.clear;
        for (int i = 0; i < materials.Length; i++)
        {
            soupColor += materials[i].Color;
            materialIcons[i].sprite = materials[i].Icon;
        }

        soupColor /= materials.Length;
        soupColor.a = 1f;
        soup.color = soupColor;

        nameText.text = CardInfo.CardName;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        healthText.text = cardState.currentHealth.ToString();
        if (cardState.defence <= 0)
        {
            defenceText.text = "";
        }
        else
        {
            defenceText.text = cardState.defence.ToString();
        }
    }

    public void TakeDamage(int amount)
    {
        cardState.TakeDamage(amount);
        UpdateVisual();
        Debug.Log(string.Format("{0}受到{1}伤害", cardInfo.CardName, amount));
        if (cardState.isDead)
        {
            OnCardEntityDead?.Invoke(this);
            gameObject?.SetActive(false);
        }
    }

    public void Heal(int amount)
    {
        cardState.Heal(amount);
        UpdateVisual();
        Debug.Log(string.Format("{0}获得{1}治疗", cardInfo.CardName, amount));
    }

    public void Defence(int amount)
    {
        cardState.Defence(amount);
        UpdateVisual();
        Debug.Log(string.Format("{0}获得{1}护盾", cardInfo.CardName, amount));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleManager.instance.HandleCardClicked(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleManager.instance.HandlePointerEnterCard(this);
        UIManager.instance.Open<TooltipView>(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 关闭高亮等动效
        UIManager.instance.Close<TooltipView>();
    }

    public string GetToolTip()
    {
        return cardInfo.ToString();
    }
}

