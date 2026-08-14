using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    public bool BattleWin { get; private set; }
    public BattlePhase Phase { get; private set; }
    public bool ExtraActionAvailable => !extraActed;
    public event Action<BattlePhase> PhaseChanged;
    public event Action<bool> BattleEnded;

    [SerializeField] private CardEntity[] enemyEntities;
    [SerializeField] private CardEntity[] playerEntities;
    [SerializeField] private Transform[] playerCardEntitySpawnRoots;
    [SerializeField] private Transform[] enemyCardEntitySpawnRoots;
    [SerializeField] private CardEntity cardEntityPrefab;
    private int playerEntityCount = 0;
    private int enemyEntityCount = 0;
    private CardEntity firstPlayerEntity = null;
    private CardEntity firstEnemyEntity = null;
    public void InitializeBattle()
    {
        Phase = BattlePhase.Initializing;
        var playerCardInfos = InventoryManager.Instance.deployedCardSlots.ToArray();
        playerEntities = new CardEntity[4];
        //enemyEntities = new CardEntity[4];
        int index = 0;
        foreach (var cardInfo in playerCardInfos)
        {
            if(cardInfo != null)
            {
                var playerCardEntity = Instantiate(cardEntityPrefab, playerCardEntitySpawnRoots[index]);
                if (firstPlayerEntity == null)
                {
                    firstPlayerEntity = playerCardEntity;
                }
                playerCardEntity.Initialize(cardInfo,false);
                playerEntities[index] = playerCardEntity;
                playerEntityCount++;
            }
            index++;
        }

        foreach (var cardEntity in enemyEntities)
        {
            if (cardEntity != null)
            {
                if (firstEnemyEntity == null)
                {
                    firstEnemyEntity = cardEntity;
                }
                cardEntity.Initialize(cardEntity.CardInfo, true);
                enemyEntityCount++;
            }
        }

        CardEntity.OnCardEntityDead += OnCardEntityDead;
        UIManager.instance.Open<BattleHUDView>(this);
        PhaseChanged?.Invoke(Phase);
    }

    private void OnCardEntityDead(CardEntity obj)
    {
        // 刷新前排entity的引用
        if (firstEnemyEntity == obj) 
        {
            firstEnemyEntity = null;
            foreach (var cardEntity in enemyEntities)
            {
                if (cardEntity != null && !cardEntity.cardState.isDead)
                {
                    firstEnemyEntity = cardEntity;
                    return;
                }
            }
        }

        if(firstPlayerEntity == obj)
        {
            firstPlayerEntity = null;
            foreach (var cardEntity in playerEntities)
            {
                if (cardEntity != null && !cardEntity.cardState.isDead)
                {
                    firstPlayerEntity = cardEntity;
                    return;
                }
            }
        }
    }

    private void ClearDefence()
    {
        foreach (var cardEntity in playerEntities) 
        {
            if( cardEntity != null && !cardEntity.cardState.isDead)
            {
                cardEntity.cardState.defence = 0;
                cardEntity.UpdateVisual();
            }
        }

        foreach(var cardEntity in enemyEntities)
        {
            if(cardEntity != null && !cardEntity.cardState.isDead)
            {
                cardEntity.cardState.defence = 0;
                cardEntity.UpdateVisual();
            }
        }
    }

    private void TriggerRelic()
    {
        // 依次触发双方遗物的效果。
    }

    public void TryInvokeIntent(CardEntity card)
    {
        if (card == null||card.cardState.isDead) { return;}
        int time = BattleClock.currentTime;
        var intents = card.CardInfo.intents;
        foreach (var intent in intents) 
        {
            if (intent.Match(time))
            {
                switch (intent.action.type)
                {
                    case MaterialAction.ActionType.Attack:
                        {
                            CardEntity targetCard;
                            if (card.cardState.isEnemy)
                            {
                                targetCard = firstPlayerEntity;
                            }
                            else
                            {
                                targetCard = firstEnemyEntity;
                            }
                            targetCard?.TakeDamage(intent.action.value);
                            break;
                        }
                    case MaterialAction.ActionType.Heal:
                        {
                            card.Heal(intent.action.value); break;
                        }
                    case MaterialAction.ActionType.Defend:
                        {
                            card.Defence(intent.action.value); break;
                        }
                    default: break;
                }

                if (IsBattleOver())
                {
                    Phase = BattlePhase.BattleOver;
                    PhaseChanged?.Invoke(Phase);
                    BattleEnded?.Invoke(BattleWin);
                    return;
                }
            }
        }
    }

    public void HandleCardClicked(CardEntity card)
    {
        if(Phase == BattlePhase.SelectingExtraAction)
        {
            SelectExtraActionCard(card);
        }
    }

    public void HandlePointerEnterCard(CardEntity card)
    {
        // 如果是选择阶段，就设置高亮等动效
    }

    private void InvokeAllIntents()
    {
        
        foreach(var card in playerEntities)
        {
            TryInvokeIntent(card);
            if (Phase == BattlePhase.BattleOver) return;
            // todo:卡片动效
        }

        foreach(var card in enemyEntities)
        {
            TryInvokeIntent(card);
            if (Phase == BattlePhase.BattleOver) return;
            // todo:卡片动效
        }

    }

    public bool IsBattleOver()
    {
        int playerAliveCount = 0;
        foreach(var card in playerEntities)
        {
            if (card != null)
            {
                if (!card.cardState.isDead)
                {
                    playerAliveCount++;
                }
            }
        }
        // 玩家死亡
        if(playerAliveCount == 0) 
        {
            BattleWin = false;
            return true;
        }

        int enemyAliveCount = 0;
        foreach(var card in enemyEntities)
        {
            if(card!= null)
            {
                if (!card.cardState.isDead)
                {
                    enemyAliveCount++;
                }
            }
        }

        // 玩家胜利
        if(enemyAliveCount == 0)
        {
            BattleWin = true;
            return true;
        }

        // 未结束
        BattleWin = false;
        return false;
    }

    private void StartRound()
    {
        ClearDefence();
        BattleClock.AdvanceClock();
        TriggerRelic();
        extraActed = false;
        Phase = BattlePhase.PlayerDecision;
        PhaseChanged?.Invoke(Phase);

    }
    private bool extraActed = false;

    public void RequestExtraAction()
    {
        if (Phase != BattlePhase.PlayerDecision) return;
        if (extraActed) return;
        Phase = BattlePhase.SelectingExtraAction;
        PhaseChanged?.Invoke(Phase);
    }
    public void CancelExtraAction()
    {
        if(Phase != BattlePhase.SelectingExtraAction)
        {
            return;
        }
        Phase = BattlePhase.PlayerDecision;
        PhaseChanged?.Invoke(Phase);

    }
    public void SelectExtraActionCard(CardEntity card)
    {
        if (Phase!= BattlePhase.SelectingExtraAction)
        {
            return;
        }
        if (card == null || card.cardState.isDead || card.cardState.isEnemy) return;
        if(extraActed) return;
        extraActed = true;
        TryInvokeIntent(card);
        if (Phase == BattlePhase.BattleOver) return;
        Phase = BattlePhase.PlayerDecision;
        PhaseChanged?.Invoke(Phase);
    }
    public void EndPlayerTurn()
    {
        if (Phase != BattlePhase.PlayerDecision && Phase != BattlePhase.SelectingExtraAction) return;
        Phase = BattlePhase.Resolving;
        PhaseChanged?.Invoke(Phase);
        InvokeAllIntents();
        if (Phase != BattlePhase.BattleOver) StartRound();
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeBattle();
        StartRound();
    }

    private void OnDestroy()
    {
        CardEntity.OnCardEntityDead -= OnCardEntityDead;
    }

}

public enum BattlePhase
{
    Initializing,
    PlayerDecision,
    SelectingExtraAction,
    Resolving,
    BattleOver
}