using UnityEngine;

[CreateAssetMenu(fileName = "StarchRelic", menuName = "ScriptableObject/素材数据/遗物/StarchRelic")]
public class StarchRelic : SoupMaterialData, IRelic
{
    [Header("淀粉")]
    [SerializeField]
    private int heal = -2;
    [SerializeField]
    private int defence = 8;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager,bool isEnemy)
    {
        return false;
    }
    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        CardEntity card = battleManager.GetRandomLivingCard(isEnemy);
        if (card == null)
        {
            return false;
        }
        card.Defence(defence);
        card.Heal(heal);
        return true;
    }
    public bool OnCardAction(BattleManager battleManager, bool isEnemy,CardEntity actingCard)
    {
        return false;
    }

    public string GetRelicInfo()
    {
        return $"回合开始使随机己方单位获得{defence}点护盾，失去{-heal}点hp";
    }
}
