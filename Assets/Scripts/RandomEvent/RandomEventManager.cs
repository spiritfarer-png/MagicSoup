using System.Collections.Generic;
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
    public CardInfo RewardCard { get; private set; }
    public SoupMaterialData RewardItem
    {
        get;
        private set;
    }

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
        RewardCard = null;
        RewardItem = null;
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
        RewardCard = null;
        RewardItem = null;
        RewardSucceeded = false;
        if (currentEvent == null)
            return;

        switch (currentEvent.randomEvent)
        {
            case RandomEventType.UpgradeCardEvent:
                UpgradeRandomCard();
                break;
            case RandomEventType.MaterialEvent:
                GiveMaterialReward(BattleManager.instance.PopMaterial());
                break;

            case RandomEventType.RelicEvent:
                GiveMaterialReward(BattleManager.instance.PopRelic());
                break;

            case RandomEventType.PotionEvent:
                GivePotionReward(BattleManager.instance.PopPotion());
                break;

        }
    }
    private void UpgradeRandomCard()
    {
        List<CardInfo> candidates = new List<CardInfo>();
        candidates.AddRange(InventoryManager.Instance.cardSlots);

        candidates.AddRange(InventoryManager.Instance.deployedCardSlots);

        candidates.RemoveAll(card => card == null || card.isAscended);

        if (candidates.Count == 0)
            return;

        RewardCard = candidates[Random.Range(0, candidates.Count)];

        RewardCard.Ascend();
        RewardSucceeded = true;
    }
    private void GiveMaterialReward(SoupMaterialData material)
    {
        if (material == null)
            return;

        bool added = InventoryManager.Instance.TryAddMaterial(material);

        if (!added)
            return;

        RewardItem = material;
        RewardSucceeded = true;
    }

    private void GivePotionReward(PotionData potion)
    {
        if (potion == null)
            return;

        bool added = InventoryManager.Instance.TryAddPotion(potion);

        if (!added)
            return;

        RewardItem = potion;
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