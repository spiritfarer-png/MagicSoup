using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager Instance { get; private set; }

    [Header("所有随机事件")]
    [SerializeField]
    private List<RandomEventSO> randomEventSOs = new List<RandomEventSO>();
    private RandomEventSO currentEvent;
    private int currentDialogueIndex;
    public RandomEventSO CurrentEvent => currentEvent;
    public CardInfo RewardCard_1 { get; private set; }
    public CardInfo RewardCard_2 { get; private set; }
    public SoupMaterialData RewardItem_1 { get; private set; }
    public SoupMaterialData RewardItem_2 { get; private set; }

    public bool RewardSucceeded
    {
        get;
        private set;
    }
    public RandomEventDialogueData CurrentDialogue
    {
        get
        {
            if (currentEvent == null)
                return null;

            if (currentEvent.dialogues == null)
                return null;

            if (currentDialogueIndex < 0 || currentDialogueIndex >= currentEvent.dialogues.Count)
                return null;

            return currentEvent.dialogues[currentDialogueIndex];
        }
    }

    public RandomEventSO BeginRandomEvent()
    {
        currentEvent = DrawRandomEvent();
        currentDialogueIndex = 0;
        RewardCard_1 = null;
        RewardCard_2 = null;
        RewardItem_1 = null;
        RewardItem_2 = null;
        RewardSucceeded = false;
        return currentEvent;
    }

    public bool SelectOption(RandomEventOptionData option)
    {
        if (option.isFinalOption)
            return true;

        int nextIndex = option.nextDialogueIndex;
        currentDialogueIndex = nextIndex;
        return false;
    }

    private RandomEventSO DrawRandomEvent()
    {
        int randomIndex = Random.Range(0, randomEventSOs.Count);
        RandomEventSO selectedEvent = randomEventSOs[randomIndex];
        return selectedEvent;
    }

    public void ResolveCurrentEvent()
    {
        RewardCard_1 = null;
        RewardCard_2 = null;
        RewardItem_1 = null;
        RewardItem_2 = null;
        RewardSucceeded = false;
        if (currentEvent == null)
            return;

        switch (currentEvent.randomEvent)
        {
            case RandomEventType.UpgradeCardEvent:
                UpgradeRandomCards(currentEvent.upgradeCardCount);
                break;
            case RandomEventType.MaterialEvent:
                GiveMaterialRewards(currentEvent.materialRewardCount);
                break;

            case RandomEventType.RelicEvent:
                GiveMaterialReward(BattleManager.instance.PopRelic());
                break;

            case RandomEventType.PotionEvent:
                GivePotionReward(BattleManager.instance.PopPotion());
                break;

        }
    }
    private void UpgradeRandomCards(int count)
    {
        List<CardInfo> candidates = new List<CardInfo>();
        candidates.AddRange(InventoryManager.Instance.cardSlots);
        candidates.AddRange(InventoryManager.Instance.deployedCardSlots);
        candidates.RemoveAll(card => card == null || card.isAscended);

        int upgradeCount = Mathf.Min(count, candidates.Count);

        for (int i = 0; i < upgradeCount; i++)
        {
            int index = Random.Range(0, candidates.Count);
            CardInfo card = candidates[index];
            candidates.RemoveAt(index);

            card.Ascend();

            if (i == 0)
                RewardCard_1 = card;
            else if (i == 1)
                RewardCard_2 = card;
        }

        RewardSucceeded = upgradeCount > 0;
    }
    private void GiveMaterialRewards(int count)
    {
        int addedCount = 0;
        int rewardCount = Mathf.Min(count, 2);

        for (int i = 0; i < rewardCount; i++)
        {
            SoupMaterialData material = BattleManager.instance.PopMaterial();

            if (material == null)
                break;

            if (!InventoryManager.Instance.TryAddMaterial(material))
                break;

            if (addedCount == 0)
                RewardItem_1 = material;
            else
                RewardItem_2 = material;

            addedCount++;
        }

        RewardSucceeded = addedCount > 0;
    }
    private void GiveMaterialReward(SoupMaterialData material)
    {
        if (material == null)
            return;

        if (!InventoryManager.Instance.TryAddMaterial(material))
            return;

        RewardItem_1 = material;
        RewardSucceeded = true;
    }

    private void GivePotionReward(PotionData potion)
    {
        if (potion == null)
            return;

        bool added = InventoryManager.Instance.TryAddPotion(potion);

        if (!added)
            return;

        RewardItem_1 = potion;
        RewardSucceeded = true;
    }



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}