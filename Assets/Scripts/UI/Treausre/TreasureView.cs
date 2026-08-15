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

        relicNameText.text = "New Relic";
        hintText.text = "You have found a treasure!\n\nOpen chest to get it!";

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
        var relic = BattleManager.instance.PopRelic();
        if (relic != null)
        {
            relicRoot.GetComponentInChildren<Image>().sprite = relic.icon;
        }
        InventoryManager.Instance.TryAddMaterial(relic);

        state = TreasureState.RewardShown;
        hintText.text = "You got a Relic!";
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