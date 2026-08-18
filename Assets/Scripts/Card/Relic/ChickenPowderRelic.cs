using UnityEngine;

[CreateAssetMenu(
    fileName = "ChickenPowderRelic",
    menuName = "ScriptableObject/素材数据/遗物/ChickenPowderRelic")]
public class ChickenPowderRelic : SoupMaterialData, IRelic
{
    [Header("鸡精")]
    [SerializeField] private int defence = 3;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }

    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        CardEntity[] cards = isEnemy ? battleManager.EnemyEntities : battleManager.PlayerEntities;

        foreach (CardEntity card in cards)
        {
            if (card == null || card.cardState.isDead)
                continue;

            card.Defence(defence);
            return true;
        }

        return false;
    }

    public bool OnCardAction(BattleManager battleManager, bool isEnemy, CardEntity actingCard)
    {
        return false;
    }

    public string GetRelicInfo()
    {
        return $"回合开始时，使最前方的友方单位获得{defence}点护盾";
    }
}