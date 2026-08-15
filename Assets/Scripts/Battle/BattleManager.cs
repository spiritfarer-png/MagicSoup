using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public CardEntity[] EnemyEntities { get { return enemyEntities; } }
    public CardEntity[] PlayerEntities { get { return playerEntities; } }
    [SerializeField] private CardEntity[] enemyEntities;
    [SerializeField] private CardEntity[] playerEntities;
    [SerializeField] private IRelic[] playerRelics;
    [SerializeField] private IRelic[] enemyRelics;
    [SerializeField] private Transform[] playerCardEntitySpawnRoots;
    [SerializeField] private Transform[] enemyCardEntitySpawnRoots;
    [SerializeField] private CardEntity cardEntityPrefab;
    [SerializeField] private EnemyConfigPool enemyConfigPool;
    private int playerEntityCount = 0;
    private int enemyEntityCount = 0;
    private CardEntity firstPlayerEntity = null;
    private CardEntity firstEnemyEntity = null;
    private int roundCount = 0;

    private void ClearBattle()
    {
        foreach(var transform in playerCardEntitySpawnRoots)
        {
            foreach(Transform _t in transform)
            {
                if(_t.TryGetComponent<CardEntity>(out var card))
                {
                    Destroy(card.gameObject);
                }
            }
        }

        foreach(var transform in enemyCardEntitySpawnRoots)
        {
            foreach(Transform _t in transform)
            {
                if (_t.TryGetComponent<CardEntity>(out var card))
                {
                    Destroy(card.gameObject);
                }
            }
        }
    }
    public void InitializeBattle()
    {
        Phase = BattlePhase.Initializing;
        ClearBattle();
        var enemyConfig = enemyConfigPool.PopNormalEnemy();
        roundCount = 0;
        var playerCardInfos = InventoryManager.Instance.deployedCardSlots.ToArray();
        var enemyCardInfos = enemyConfig.enemyCardInfos;
        playerEntities = new CardEntity[4];
        enemyEntities = new CardEntity[4];
        int index = 0;

        // 生成玩家卡牌
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

        // 生成敌方卡牌
        index = 0;
        foreach(var cardInfo in enemyCardInfos)
        {
            if(cardInfo != null)
            {
                var enemyCardEntity = Instantiate(cardEntityPrefab, enemyCardEntitySpawnRoots[index]);
                if(firstEnemyEntity == null)
                {
                    firstEnemyEntity = enemyCardEntity;
                }

                enemyCardEntity.Initialize(cardInfo,true);
                enemyEntities[index] = enemyCardEntity;
                enemyEntityCount++;
            }
            index++;
        }


        // 收集双方遗物
        playerRelics = InventoryManager.Instance.CreateRelicSnapshot().ToArray();
        enemyRelics = enemyConfig.enemyRelics.Select(relic => (IRelic)relic).ToArray();

        // todo:药水

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

    private void ClearDefence(CardEntity[] cards)
    {
        foreach (var cardEntity in cards) 
        {
            if( cardEntity != null && !cardEntity.cardState.isDead)
            {
                cardEntity.cardState.defence = 0;
                cardEntity.UpdateVisual();
            }
        }
    }



    public void TryInvokeIntent(CardEntity card)
    {
        if (card == null||card.cardState.isDead) { return;}

        // 触发遗物的卡牌行动节点
        bool isEnemy = card.cardState.isEnemy;
        var relics = isEnemy ? enemyRelics : playerRelics;
        if (relics != null)
        {
            foreach (var relic in relics)
            {
                relic.OnCardAction(this, isEnemy, card);
                if (IsBattleOver())
                {
                    DoBattleOver();
                    return;
                }

            }
        }


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
                            if (isEnemy)
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
                    DoBattleOver();
                    return;
                }
            }
        }
    }

    private void DoBattleOver()
    {
        Phase = BattlePhase.BattleOver;
        PhaseChanged?.Invoke(Phase);
        BattleEnded?.Invoke(BattleWin);
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

    private void InvokeIntents(CardEntity[] cards)
    {
        
        foreach(var card in cards)
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
        BattleClock.AdvanceClock();
        roundCount++;
        // 清空防御
        ClearDefence(playerEntities);

        // 触发战斗开始遗物节点
        if(roundCount == 1)
        {
            if (playerRelics != null)
            {
                foreach (var relic in playerRelics)
                {
                    relic?.OnBattleStart(this, false);
                    if (IsBattleOver())
                    {
                        DoBattleOver();
                        return;
                    }
                }
            }
            
            if(enemyRelics != null)
            {
                foreach (var relic in enemyRelics)
                {
                    relic?.OnBattleStart(this, true);
                    if (IsBattleOver())
                    {
                        DoBattleOver();
                        return;
                    }
                }
            }
            
        }


        // 触发玩家遗物的每回合行动节点
        if (playerRelics != null)
        {
            foreach(var relic in playerRelics)
            {
                relic?.OnRoundStart(this, false);
                if (IsBattleOver())
                {
                    DoBattleOver();
                    return;
                }
            }
        }
            

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
        InvokeIntents(playerEntities);
        if (Phase == BattlePhase.BattleOver) 
            return;
        ClearDefence(enemyEntities);
        // 触发敌方遗物效果
        if(enemyRelics != null)
        {
            foreach(var relic in enemyRelics){
                relic.OnRoundStart(this, true);
                if (IsBattleOver())
                {
                    DoBattleOver();
                    return;
                }
            }
        }
        if (Phase == BattlePhase.BattleOver)
            return;
        InvokeIntents(enemyEntities);
        if (Phase == BattlePhase.BattleOver)
            return;
        StartRound();
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


    public CardEntity GetRandomLivingCard(bool isEnemy)
    {
        CardEntity[] entities = isEnemy ? enemyEntities : playerEntities;
        int livingCount = 0;

        foreach (CardEntity entity in entities)
        {
            if (entity != null && !entity.cardState.isDead)
                livingCount++;
        }

        if (livingCount == 0)
            return null;

        int targetIndex = UnityEngine.Random.Range(0, livingCount);

        foreach (CardEntity entity in entities)
        {
            if (entity == null || entity.cardState.isDead)
                continue;

            if (targetIndex == 0)
                return entity;

            targetIndex--;
        }

        return null;
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

public enum BattleType
{
    Normal,
    Elite,
    Boss,
}
