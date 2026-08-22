using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RandomEventView : UIView
{
    [Header("事件内容")]
    [SerializeField]
    private TMP_Text titleText;
    [SerializeField]
    private Image eventIcon;
    [SerializeField]
    private TMP_Text dialogueText;
    [SerializeField]
    private TMP_Text endDeclare;
    [Header("选项")]
    [SerializeField]
    private RectTransform optionRoot;
    [SerializeField]
    private RandomEventOptionView optionPrefab;
    [Header("离开事件")]
    [SerializeField]
    private Button leaveButton;
    [Header("奖励展示")]
    [SerializeField]
    private GameObject rewardRoot;
    [SerializeField]
    private CardViewUI cardRewardView_1;
    [SerializeField]
    private CardViewUI cardRewardView_2;
    [SerializeField]
    private RandomEventRewardItemView itemRewardView_1;
    [SerializeField]
    private RandomEventRewardItemView itemRewardView_2;

    [SerializeField]
    private RectTransform rewardRoot_1;
    [SerializeField]
    private RectTransform rewardRoot_2;

    [SerializeField]
    private float riseDistance = 100f;
    [SerializeField]
    private float splitDistance = 120f;
    [SerializeField]
    private float riseDuration = 0.45f;
    [SerializeField]
    private float splitDuration = 0.3f;
    [SerializeField]
    private float secondRewardDelay = 0.08f;

    private RandomEventSO eventSO;
    private MapNodeData currentNode;
    private Vector2 rewardStartPosition;
    private Sequence rewardSequence;


    protected override void OnOpen(object param)
    {
        currentNode = param as MapNodeData;
        if (currentNode == null || currentNode.NodeType != MapNodeType.RandomEvent)
        {
            Debug.LogError("RandomEventView 没有收到有效的 RandomEvent 节点。", this);

            return;
        }
        Initialize();
        RefreshDialogue();
    }
    public void Initialize()
    {
        ResetRewardAnimation();
        eventSO = RandomEventManager.Instance.BeginRandomEvent();
        titleText.text = eventSO.eventTitle;
        eventIcon.sprite = eventSO.eventIcon;
        eventIcon.gameObject.SetActive(eventSO.eventIcon != null);
        dialogueText.gameObject.SetActive(true);
        optionRoot.gameObject.SetActive(true);
        rewardRoot.SetActive(false);
        endDeclare.gameObject.SetActive(false);
        endDeclare.text = null;
        cardRewardView_1.gameObject.SetActive(false);
        cardRewardView_2.gameObject.SetActive(false);
        itemRewardView_1.gameObject.SetActive(false);
        itemRewardView_2.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(true);
        leaveButton.interactable = true;
        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    private void OnLeaveClicked()
    {
        AudioManager.Instance.PlaySFX("点击音效");
        FinishEvent();
    }


    protected override void OnClose()
    {
        rewardSequence?.Kill();
        leaveButton.onClick.RemoveAllListeners();
        ClearOptions();
    }

    private void ResetRewardAnimation()
    {
        rewardSequence?.Kill();

        rewardRoot_1.DOKill();
        rewardRoot_2.DOKill();

        rewardRoot_1.anchoredPosition = rewardStartPosition;
        rewardRoot_2.anchoredPosition = rewardStartPosition;

        rewardRoot_1.gameObject.SetActive(false);
        rewardRoot_2.gameObject.SetActive(false);
    }
    private void RefreshDialogue()
    {
        RandomEventDialogueData dialogue = RandomEventManager.Instance.CurrentDialogue;

        if (dialogue == null)
        {
            Debug.LogError("当前随机事件对话无效。", this);
            return;
        }

        dialogueText.text = dialogue.dialogueText;

        ClearOptions();

        if (dialogue.options == null)
            return;

        foreach (RandomEventOptionData option in dialogue.options)
        {
            if (option == null)
                continue;

            RandomEventOptionView optionView = Instantiate(optionPrefab, optionRoot, false);

            optionView.Initialize(option, OnOptionClicked);
        }
    }

    private void OnOptionClicked(RandomEventOptionData option)
    {
        bool isFinalOption = RandomEventManager.Instance.SelectOption(option);
        bool isGetReward = option.isGetReward;

        if (!isFinalOption)
        {
            RefreshDialogue();
            return;
        }
        if (!isGetReward)
        {
            FinishEvent();
            return;
        }

        RandomEventManager.Instance.ResolveCurrentEvent();
        ShowReward();
    }
    private void FinishEvent()
    {
        leaveButton.interactable = false;
        MapManager.Instance.OnRandomEventFinished();
    }

    private void ShowReward()
    {
        ClearOptions();

        optionRoot.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        rewardRoot.SetActive(true);

        RandomEventManager manager = RandomEventManager.Instance;

        if (!manager.RewardSucceeded)
        {
            cardRewardView_1.gameObject.SetActive(false);
            cardRewardView_2.gameObject.SetActive(false);
            itemRewardView_1.gameObject.SetActive(false);
            itemRewardView_2.gameObject.SetActive(false);

            dialogueText.gameObject.SetActive(true);
            dialogueText.text = GetFailureDescription();
            return;
        }

        bool hasCardReward = manager.RewardCard_1 != null;
        bool hasItemReward = manager.RewardItem_1 != null;

        if (!hasCardReward && !hasItemReward)
            return;

        bool hasSecondReward = manager.RewardCard_2 != null || manager.RewardItem_2 != null;
        rewardRoot_1.gameObject.SetActive(true);
        rewardRoot_2.gameObject.SetActive(hasSecondReward);

        if (hasCardReward)
        {
            ShowCardReward(manager.RewardCard_1, manager.RewardCard_2);
        }
        else
        {
            ShowItemReward(manager.RewardItem_1, manager.RewardItem_2);
        }

        if (hasSecondReward)
            PlayDoubleRewardAnimation();
        else
            PlaySingleRewardAnimation();

    }

    private void ShowCardReward(CardInfo first, CardInfo second)
    {
        itemRewardView_1.gameObject.SetActive(false);
        itemRewardView_2.gameObject.SetActive(false);

        cardRewardView_1.Bind(first);
        cardRewardView_2.Bind(second);

        endDeclare.gameObject.SetActive(true);

        endDeclare.text = second == null
            ? $"{first.CardName}已升级！"
            : $"{first.CardName}、{second.CardName}已升级！";
    }

    private void ShowItemReward(SoupMaterialData first, SoupMaterialData second)
    {
        cardRewardView_1.gameObject.SetActive(false);
        cardRewardView_2.gameObject.SetActive(false);

        if (first != null)
        {
            itemRewardView_1.gameObject.SetActive(true);
            itemRewardView_1.Bind(first);
        }
        else
        {
            itemRewardView_1.gameObject.SetActive(false);
        }

        if (second != null)
        {
            itemRewardView_2.gameObject.SetActive(true);
            itemRewardView_2.Bind(second);
        }
        else
        {
            itemRewardView_2.gameObject.SetActive(false);
        }

        endDeclare.gameObject.SetActive(true);

        endDeclare.text = second == null
            ? $"获得{first.materialName}！"
            : $"获得{first.materialName}、{second.materialName}！";
    }


    private string GetFailureDescription()
    {
        switch (eventSO.randomEvent)
        {
            case RandomEventType.UpgradeCardEvent:
                return "当前没有可以升级的卡牌。";
            case RandomEventType.MaterialEvent:
            case RandomEventType.RelicEvent:
                return "素材背包已满，无法获得物品。";
            case RandomEventType.PotionEvent:
                return "药水栏已满，无法获得药水。";
            default:
                return "没有获得奖励。";
        }
    }

    private void ClearOptions()
    {
        if (optionRoot == null)
            return;

        for (int i = optionRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(optionRoot.GetChild(i).gameObject);
        }
    }

    private void PlaySingleRewardAnimation()
    {
        Vector2 target = rewardStartPosition + Vector2.up * riseDistance;

        rewardSequence?.Kill();

        rewardSequence = DOTween.Sequence();

        rewardSequence.Append(rewardRoot_1.DOAnchorPos(target, riseDuration).SetEase(Ease.OutCubic));

        rewardSequence.SetUpdate(true).SetLink(gameObject, LinkBehaviour.KillOnDisable);

    }

    private void PlayDoubleRewardAnimation()
    {
        Vector2 riseTarget = rewardStartPosition + Vector2.up * riseDistance;
        Vector2 leftTarget = riseTarget + Vector2.left * splitDistance;
        Vector2 rightTarget = riseTarget + Vector2.right * splitDistance;
        rewardSequence?.Kill();
        rewardSequence = DOTween.Sequence();
        rewardSequence.Append(rewardRoot_1.DOAnchorPos(riseTarget, riseDuration).SetEase(Ease.OutCubic));
        rewardSequence.AppendInterval(secondRewardDelay);

        rewardSequence.Append(rewardRoot_2.DOAnchorPos(riseTarget, riseDuration).SetEase(Ease.OutCubic));
        rewardSequence.AppendInterval(0.08f);

        rewardSequence.Append(rewardRoot_1.DOAnchorPos(leftTarget, splitDuration).SetEase(Ease.OutBack));
        rewardSequence.Join(rewardRoot_2.DOAnchorPos(rightTarget, splitDuration).SetEase(Ease.OutBack));
        rewardSequence.SetUpdate(true).SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }
    private void Awake()
    {
        rewardStartPosition = rewardRoot_1.anchoredPosition;
    }
}