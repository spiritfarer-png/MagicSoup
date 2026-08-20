using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    public bool BattleWin { get; private set; }
    public BattlePhase Phase { get; private set; }
    public bool ExtraActionAvailable => !extraActed;
    public event Action<BattlePhase> PhaseChanged;
    public event Action<bool> BattleEnded;
    public BattleType BattleType { get; private set; }
    public CardEntity[] EnemyEntities { get { return enemyEntities; } }
    public CardEntity[] PlayerEntities { get { return playerEntities; } }
    public IRelic[] PlayerRelics { get { return playerRelics; } }
    public IRelic[] EnemyRelics { get { return enemyRelics; } }
    public PotionData[] Potions { get { return potions; } }
    [SerializeField] private CardEntity[] enemyEntities;
    [SerializeField] private CardEntity[] playerEntities;
    [SerializeField] private IRelic[] playerRelics;
    [SerializeField] private IRelic[] enemyRelics;
    private PotionData[] potions;
    [SerializeField] private Transform[] playerCardEntitySpawnRoots;
    [SerializeField] private Transform[] enemyCardEntitySpawnRoots;
    [SerializeField] private CardEntity cardEntityPrefab;
    [SerializeField] private EnemyConfigPool enemyConfigPool;
    [SerializeField] private LootPool lootPool;
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

        firstPlayerEntity = null;
        firstEnemyEntity = null;
        playerEntityCount = 0;
        enemyEntityCount = 0;
        playerEntities = Array.Empty<CardEntity>();
        enemyEntities = Array.Empty<CardEntity>();
    }
    public void InitializeBattle(BattleType battleType = BattleType.Normal)
    {
        Phase = BattlePhase.Initializing;
        BattleType = battleType;
        ClearBattle();
        EnemyConfig enemyConfig;
        switch (battleType)
        {
            case BattleType.Normal: enemyConfig = enemyConfigPool.PopNormalEnemy(); break;
            case BattleType.Elite: enemyConfig = enemyConfigPool.PopEliteEnemy(); break;
            case BattleType.Boss: enemyConfig = enemyConfigPool.PopBoss(); break;
            default: enemyConfig = null; Debug.LogError("未定义"); break;
        }

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

        // 收集药水
        var validpotionSlotDatas = InventoryManager.Instance.potionSlots.Select(data => data.IsOccupied);
        var potionList = new List<PotionData>();
        int i = 0;
        foreach(var valid in validpotionSlotDatas)
        {
            if (valid)
            {
                potionList.Add((PotionData)InventoryManager.Instance.potionSlots[i].materialData);
            }
            i++;
        }
        potions = potionList.ToArray();

        CardEntity.OnCardEntityDead -= OnCardEntityDead;
        CardEntity.OnCardEntityDead += OnCardEntityDead;
        UIManager.instance.Open<BattleHUDView>(this);
        PhaseChanged?.Invoke(Phase);

        StartRound();
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



    public IEnumerator TryInvokeIntent(CardEntity card)
    {
        if (card == null || card.cardState.isDead) yield break;

        bool isEnemy = card.cardState.isEnemy;
        var relics = isEnemy ? enemyRelics : playerRelics;

        yield return ResolveIntents(card.CardInfo.intents, card, isEnemy);
        if (Phase == BattlePhase.BattleOver) yield break;

        if (relics != null)
        {
            foreach (var relic in relics)
            {
                relic.OnCardAction(this, isEnemy, card);

                if (IsBattleOver())
                {
                    DoBattleOver();
                    yield break;
                }
            }
        }
    }

    public void UsePotion(PotionData potion)
    {
        StartCoroutine(PotionCoroutine(potion));
    }

    private IEnumerator PotionCoroutine(PotionData potion)
    {
        Phase = BattlePhase.Resolving;
        PhaseChanged?.Invoke(Phase);
        InventoryManager.Instance.ConsumePotion(potion);
        yield return ResolveIntents(potion.normalIntents, firstPlayerEntity, false);
        if (Phase == BattlePhase.BattleOver) 
        {
            yield break;
        }
        Phase = BattlePhase.PlayerDecision;
        PhaseChanged?.Invoke(Phase);
    }


    private IEnumerator ResolveIntents(Intent[] intents, CardEntity card, bool isEnemy)
    {
        int time = BattleClock.currentTime;

        foreach (var intent in intents)
        {
            if (!intent.Match(time)) continue;

            Tween tween = null;
            CardEntity target = null;

            switch (intent.action.type)
            {
                case MaterialAction.ActionType.Attack:
                    target = isEnemy ? firstPlayerEntity : firstEnemyEntity;
                    if (target == null) break;

                    AudioManager.Instance.PlaySFX("攻击音效");

                    card.StopAnimation();
                    target.StopAnimation();
                    target.SetHitColor(true);
                    target.TakeDamage(intent.action.value);
                    bool targetDied = target.cardState.isDead;

                    tween = DOTween.Sequence()
                        .Join(card.HorizontalShake())
                        .Join(target.VerticalShake())
                        .OnComplete(() =>
                        {
                            target.SetHitColor(false);
                            if (targetDied) target.gameObject.SetActive(false);
                        })
                        .OnKill(() => target.SetHitColor(false));
                    break;

                case MaterialAction.ActionType.Heal:
                    card.StopAnimation();
                    card.Heal(intent.action.value);
                    if (intent.action.value < 0)
                    {
                        card.SetHitColor(true);
                    }
                    tween = card.VerticalShake().OnComplete(()=>
                    {
                        card.SetHitColor(false);
                    });
                    break;

                case MaterialAction.ActionType.Defend:
                    card.StopAnimation();
                    card.Defence(intent.action.value);
                    tween = card.VerticalShake();
                    break;

                case MaterialAction.ActionType.AttackAll:
                    CardEntity[] enemyTargets = isEnemy ? playerEntities : enemyEntities;
                    card.StopAnimation();
                    DG.Tweening.Sequence attackAllSequence = DOTween.Sequence().Join(card.HorizontalShake());
                    foreach (CardEntity enemyTarget in enemyTargets)
                    {
                        if (enemyTarget == null || enemyTarget.cardState.isDead) continue;
                        enemyTarget.StopAnimation();
                        enemyTarget.SetHitColor(true);
                        enemyTarget.TakeDamage(intent.action.value);
                        bool died = enemyTarget.cardState.isDead;
                        attackAllSequence.Join(enemyTarget.VerticalShake().OnComplete(() =>
                        {
                            enemyTarget.SetHitColor(false);
                            if (died) enemyTarget.gameObject.SetActive(false);
                        }).OnKill(() => enemyTarget.SetHitColor(false)));
                    }
                    tween = attackAllSequence;
                    break;

                case MaterialAction.ActionType.HealAll:
                    CardEntity[] allyTargets = isEnemy ? enemyEntities : playerEntities;
                    DG.Tweening.Sequence healAllSequence = DOTween.Sequence();
                    foreach (CardEntity allyTarget in allyTargets)
                    {
                        if (allyTarget == null || allyTarget.cardState.isDead) continue;
                        allyTarget.StopAnimation();
                        allyTarget.Heal(intent.action.value);
                        healAllSequence.Join(allyTarget.VerticalShake());
                    }
                    tween = healAllSequence;
                    break;
            }

            if (IsBattleOver())
            {
                DoBattleOver();
                yield break;
            }

            if (tween != null) yield return tween.WaitForCompletion();
        }
    }

    private void DoBattleOver()
    {
        Phase = BattlePhase.BattleOver;
        PhaseChanged?.Invoke(Phase);
        BattleEnded?.Invoke(BattleWin);
        UIManager.instance.Close<BattleHUDView>();
        // 打开战利品界面/死亡界面
        if (BattleWin)
        {
            if(BattleType == BattleType.Boss)
            {
                // 游戏胜利
                UIManager.instance.Open<GameWinView>();
            }
            else
            {
                SoupMaterialData[] loots;
                if(BattleType == BattleType.Elite)
                {
                    loots = new SoupMaterialData[] { PopMaterial(),PopRelic(),PopPotion()};
                }
                else
                {
                    loots = new SoupMaterialData[] { PopMaterial(), PopPotion()};
                }

                UIManager.instance.Open<BattleWinView>(loots);
            }
        }
        else
        {
            UIManager.instance.Open<GameOverView>();
        }
    }

    public void ResartGame()
    {
        SceneManager.LoadScene("GamePlayScene");
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

    private IEnumerator InvokeIntents(CardEntity[] cards)
    {
        
        foreach(var card in cards)
        {
            yield return TryInvokeIntent(card);
            if (Phase == BattlePhase.BattleOver) yield break;
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

    public SoupMaterialData PopMaterial()
    {
        return lootPool.PopMaterial();
    }

    public SoupMaterialData PopRelic()
    {
        return lootPool.PopRelic();
    }

    public PotionData PopPotion()
    {
        return lootPool.PopPotion();
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
        AudioManager.Instance.PlaySFX("点击音效");
        if (Phase != BattlePhase.PlayerDecision) return;
        if (extraActed) return;
        Phase = BattlePhase.SelectingExtraAction;
        PhaseChanged?.Invoke(Phase);
    }
    public void CancelExtraAction()
    {
        AudioManager.Instance.PlaySFX("点击音效");

        if (Phase != BattlePhase.SelectingExtraAction)
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
        StartCoroutine(ExtraActionCoroutine(card));
    }

    private IEnumerator ExtraActionCoroutine(CardEntity card)
    {
        Phase = BattlePhase.Resolving;
        PhaseChanged?.Invoke(Phase);
        yield return TryInvokeIntent(card);
        if (Phase == BattlePhase.BattleOver) yield break;
        Phase = BattlePhase.PlayerDecision;
        PhaseChanged?.Invoke(Phase);
    }
    public void EndPlayerTurn()
    {
        AudioManager.Instance.PlaySFX("点击音效");
        if (Phase != BattlePhase.PlayerDecision &&
            Phase != BattlePhase.SelectingExtraAction) return;

        StartCoroutine(ResolveCoroutine());
    }

    private IEnumerator ResolveCoroutine()
    {
        Phase = BattlePhase.Resolving;
        PhaseChanged?.Invoke(Phase);
        // 玩家卡片行动
        yield return InvokeIntents(playerEntities);
        if (Phase == BattlePhase.BattleOver) yield break;


        ClearDefence(enemyEntities);
        // 触发敌方遗物效果
        if (enemyRelics != null)
        {
            foreach (var relic in enemyRelics)
            {
                relic.OnRoundStart(this, true);
                if (IsBattleOver())
                {
                    DoBattleOver();
                    yield break;
                }
            }
        }

        yield return InvokeIntents(enemyEntities);
        if (Phase == BattlePhase.BattleOver) yield break;

        StartRound();
    }

    private void Awake()
    {
        instance = this;
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
