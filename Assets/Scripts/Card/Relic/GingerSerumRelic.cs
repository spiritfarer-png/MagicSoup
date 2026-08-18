using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GingerSerumRelic", menuName = "ScriptableObject/素材数据/遗物/GingerSerumRelic")]
public class GingerSerumRelic : SoupMaterialData, IRelic
{
    [Header("生姜精华液")]
    [SerializeField]
    int value = -3;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }
    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        bool triggered = false;
        CardEntity[] cards = isEnemy ? battleManager.PlayerEntities : battleManager.EnemyEntities;
        if (cards == null)
        {
            return triggered;
        }
        foreach (CardEntity card in cards)
        {
            if (card == null || card.cardState.isDead)
                continue;

            card.Heal(value);
            triggered = true;
        }
        return triggered;
    }
    public bool OnCardAction(BattleManager battleManager, bool isEnemy, CardEntity actingCard)
    {
        return false;
    }

    public string GetRelicInfo()
    {
        return $"回合开始使所有敌方受到{-value}点伤害";
    }
}
