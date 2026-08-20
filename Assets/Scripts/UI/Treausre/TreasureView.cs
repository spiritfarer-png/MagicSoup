using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TreasureView : UIView
{
    private enum TreasureState
    {
        WaitingForOpen,
        Revealing,
        RewardShown
    }

    [Header("Chest")]
    [SerializeField] private Button chestButton;
    [SerializeField] private Image chestImage;
    [SerializeField] private Sprite closedChestSprite;
    [SerializeField] private Sprite openedChestSprite;

    [Header("Relic")]
    [SerializeField] private RectTransform relicRoot;
    [SerializeField] private CanvasGroup relicCanvasGroup;
    [SerializeField] private TMP_Text relicNameText;
    [SerializeField] private RandomTreasureView relicRewardView;

    [Header("Other")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Button leaveButton;

    [Header("Relic Float")]
    [SerializeField] private float riseDistance = 200f;

    private Vector2 relicStartPosition;
    private Vector2 relicEndPosition;
    [SerializeField] private float revealDuration = 0.6f;

    private MapNodeData currentNode;
    private TreasureState state;
    private Coroutine revealCoroutine;

    private void Awake()
    {
        relicStartPosition = relicRoot.anchoredPosition;
        relicEndPosition = relicStartPosition + Vector2.up * riseDistance;

        chestButton.onClick.AddListener(OnChestClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    protected override void OnOpen(object param)
    {
        currentNode = param as MapNodeData;

        if (currentNode == null || currentNode.NodeType != MapNodeType.Treasure)
        {
            Debug.LogError(
                "TreasureView 没有收到有效的 Treasure 节点。", this);

            return;
        }

        ResetView();
    }

    protected override void OnClose()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        // 关闭遗物图标的悬停 Tooltip，避免离开宝箱房后残留
        if (relicRewardView != null && UIManager.instance != null &&
            UIManager.instance.TryGet<TooltipView>(out var tipView) &&
            ReferenceEquals(tipView.source, relicRewardView) && tipView.IsOpen)
        {
            UIManager.instance.Close<TooltipView>();
        }

        currentNode = null;
    }

    private void ResetView()
    {
        state = TreasureState.WaitingForOpen;

        chestImage.sprite = closedChestSprite;
        chestButton.interactable = true;

        relicRoot.anchoredPosition = relicStartPosition;
        relicRoot.localScale = Vector3.one * 0.7f;
        relicCanvasGroup.alpha = 0f;
        relicRoot.gameObject.SetActive(false);

        relicNameText.text = string.Empty;
        hintText.text = "你找到了一个宝箱！\n\n打开宝箱获取宝藏吧！";

        leaveButton.gameObject.SetActive(false);
    }

    private void OnChestClicked()
    {
        if (state != TreasureState.WaitingForOpen)
        {
            return;
        }

        state = TreasureState.Revealing;
        chestImage.sprite = openedChestSprite;
        chestButton.interactable = false;
        hintText.text = string.Empty;

        relicRoot.anchoredPosition = relicStartPosition;
        relicRoot.localScale = Vector3.one * 0.7f;
        relicCanvasGroup.alpha = 0f;
        relicRoot.gameObject.SetActive(true);

        revealCoroutine = StartCoroutine(RevealRelic());
    }

    private IEnumerator RevealRelic()
    {
        // 动画开始前先取出遗物并绑定图标，避免动画期间显示空白
        var relic = BattleManager.instance.PopRelic();
        if (relic != null)
        {
            relicNameText.text = relic.materialName;

            RandomTreasureView rewardView = relicRewardView;
            if (rewardView == null)
            {
                Image relicIcon = relicRoot.GetComponentInChildren<Image>();
                relicIcon.raycastTarget = true;
                rewardView = relicIcon.GetComponent<RandomTreasureView>();
                if (rewardView == null)
                {
                    rewardView = relicIcon.gameObject.AddComponent<RandomTreasureView>();
                }
                relicRewardView = rewardView;
            }
            rewardView.Bind(relic);
        }
        // 提前入包，即使动画中途关闭也不会丢失遗物
        InventoryManager.Instance.TryAddMaterial(relic);

        float elapsedTime = 0f;

        while (elapsedTime < revealDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / revealDuration);

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            relicRoot.anchoredPosition = Vector2.Lerp(relicStartPosition, relicEndPosition, smoothProgress);
            relicRoot.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one, smoothProgress);
            relicCanvasGroup.alpha = progress;
            yield return null;
        }

        relicRoot.anchoredPosition = relicEndPosition;
        relicRoot.localScale = Vector3.one;
        relicCanvasGroup.alpha = 1f;

        state = TreasureState.RewardShown;
        hintText.text = "你得到了一件新遗物！";
        leaveButton.gameObject.SetActive(true);

        revealCoroutine = null;
    }

    private void OnLeaveClicked()
    {
        if (state != TreasureState.RewardShown)
        {
            return;
        }

        MapManager.Instance.OnTreasureRoomFinished();
    }
}