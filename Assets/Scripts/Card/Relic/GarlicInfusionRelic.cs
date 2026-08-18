using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "GarlicInfusionRelic", menuName = "ScriptableObject/素材数据/遗物/GarlicInfusionRelic")]
public class GarlicInfusionRelic : SoupMaterialData, IRelic
{
    [Header("蒜水")]
    [SerializeField]
    int value = 1;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }
    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }
    public bool OnCardAction(BattleManager battleManager, bool isEnemy, CardEntity actingCard)
    {
        bool triggered = false;
        if (actingCard == null || actingCard.cardState.isDead)
        {
            return triggered;
        }
        actingCard.Heal(value);
        triggered = true;

        return triggered;
    }

    public string GetRelicInfo()
    {
        return $"我方单位行动前，恢复{value}点hp";
    }
}
