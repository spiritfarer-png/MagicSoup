using DG.Tweening;
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
    public Vector2 iniLocalPosition { get; private set; }
    public Vector3 iniLocalScale { get; private set; }
    public Quaternion iniLocalRotation { get; private set; }
    [SerializeField] private Image background;
    public Color initialBackgroundColor {  get; private set; }
    public float animationDuration = 0.25f;
    public float horizontalStrength = 30f;
    public float verticalStrength = 20f;
    public int vibrato = 12;
    public Color hitColor = new Color(1f, 0.35f, 0.35f, 1f);
    private void Awake()
    {

        iniLocalPosition = transform.localPosition;
        iniLocalScale = transform.localScale;
        iniLocalRotation = transform.localRotation;
        initialBackgroundColor = background.color;
    }
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
     
        AudioManager.Instance.PlaySFX("受击音效");

        if (cardState.isDead)
        {
            OnCardEntityDead?.Invoke(this);
        }
    }

    public void Heal(int amount)
    {
        int healedAmount = cardState.Heal(amount);
        UpdateVisual();
        Debug.Log(string.Format("{0}获得{1}治疗", cardInfo.CardName, healedAmount));
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
        var phase = BattleManager.instance.Phase;
        if (!cardState.isEnemy && (phase == BattlePhase.PlayerDecision || phase == BattlePhase.SelectingExtraAction))
        {
            transform.DOScale(1.2f, 0.2f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 关闭高亮等动效
        UIManager.instance.Close<TooltipView>();
        transform.DOScale(1f, 0.2f);
    }

    public string GetToolTip()
    {
        return cardInfo.ToString();
    }

    public Tween HorizontalShake()
    {
        return transform.DOShakePosition(animationDuration, new Vector2(horizontalStrength, 0f), vibrato);
    }

    public Tween VerticalShake() 
    {
        return transform.DOShakePosition(animationDuration,new Vector2(0f, verticalStrength),vibrato);
    }

    public void SetHitColor(bool isHit)
    {
        background.color = isHit ? hitColor : initialBackgroundColor;
    }

    public void StopAnimation()
    {
        transform.DOKill();
        transform.localPosition = iniLocalPosition;
        transform.localScale = iniLocalScale;
        transform.localRotation = iniLocalRotation;
        SetHitColor(false);
    }

    private void OnDestroy()
    {
        StopAnimation();
    }

}

