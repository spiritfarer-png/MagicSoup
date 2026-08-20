using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StiffDough", menuName = "ScriptableObject/素材数据/遗物/StiffDough")]
public class StiffDough : SoupMaterialData, IRelic
{
    [Header("硬面团")]
    [SerializeField]
    private int defence = 1;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager,bool isEnemy)
    {
        List<CardEntity> cards = new();
        cards.AddRange(battleManager.PlayerEntities);
        cards.AddRange(battleManager.EnemyEntities);
        if (cards.Count == 0)
        {
            return false;
        }
        foreach (CardEntity card in cards)
        {
            if (card == null || card.cardState.isDead)
                continue;
            card.Defence(defence);
        }
        return true;
    }
    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }
    public bool OnCardAction(BattleManager battleManager, bool isEnemy,CardEntity actingCard)
    {
        return false;
    }

    public string GetRelicInfo()
    {
        return $"战斗开始，所有单位防御增加{defence}点";
    }
}
