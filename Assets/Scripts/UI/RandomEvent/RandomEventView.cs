using System.Text;
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
    private CardViewUI cardRewardView;
    [SerializeField]
    private Image itemRewardImage;
    [SerializeField]
    private TMP_Text rewardDescriptionText;
    private RandomEventSO eventSO;
    private MapNodeData currentNode;


    protected override void OnOpen(object param)
    {
        currentNode = param as MapNodeData;
        if (currentNode == null || currentNode.NodeType != MapNodeType.RandomEvent)
        {
            Debug.LogError("RandomEventView 没有收到有效的 RandomEvent 节点。", this);

            return;
        }
        Initialize();
        titleText.text = eventSO.eventTitle;
        eventIcon.sprite = eventSO.eventIcon;
        eventIcon.gameObject.SetActive(eventSO.eventIcon != null);
        dialogueText.gameObject.SetActive(true);
        optionRoot.gameObject.SetActive(true);
        rewardRoot.SetActive(false);
        cardRewardView.gameObject.SetActive(false);
        itemRewardImage.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(true);
        leaveButton.interactable = true;
        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveClicked);

        RefreshDialogue();
    }

    public void Initialize()
    {
        eventSO = RandomEventManager.Instance.BeginRandomEvent();
    }

    private void OnLeaveClicked()
    {
        leaveButton.interactable = false;
        MapManager.Instance.OnRandomEventFinished();
    }


    protected override void OnClose()
    {
        leaveButton.onClick.RemoveAllListeners();
        ClearOptions();
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

        if (isFinalOption)
        {
            RandomEventManager.Instance.ResolveCurrentEvent();
            ShowReward();
            return;
        }

        RefreshDialogue();
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
            cardRewardView.gameObject.SetActive(false);
            itemRewardImage.gameObject.SetActive(false);
            rewardDescriptionText.text = GetFailureDescription();
            return;
        }

        if (manager.RewardCard != null)
        {
            ShowCardReward(manager.RewardCard);
            return;
        }

        if (manager.RewardItem != null)
        {
            ShowItemReward(manager.RewardItem);
        }
    }

    private void ShowCardReward(CardInfo card)
    {
        itemRewardImage.gameObject.SetActive(false);
        cardRewardView.gameObject.SetActive(true);

        cardRewardView.Bind(card);
        rewardDescriptionText.text = card.ToString();
    }

    private void ShowItemReward(SoupMaterialData item)
    {
        cardRewardView.gameObject.SetActive(false);
        itemRewardImage.gameObject.SetActive(true);
        itemRewardImage.sprite = item.icon;
        itemRewardImage.preserveAspect = true;
        rewardDescriptionText.text = BuildItemDescription(item);
    }

    private string BuildItemDescription(SoupMaterialData item)
    {
        StringBuilder builder = new StringBuilder();
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
            builder.AppendLine(relic.GetRelicInfo());
        }

        return builder.ToString();
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
}