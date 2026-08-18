using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DriedMeat", menuName = "ScriptableObject/素材数据/遗物/DriedMeat")]
public class DriedMeat : SoupMaterialData, IRelic
{
    [Header("肉干")]
    [SerializeField]
    int value = 1;
    public SoupMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager,bool isEnemy)
    {
        List<CardEntity> cards = new();
        cards.AddRange(battleManager.PlayerEntities);
        cards.AddRange(battleManager.EnemyEntities);
        
        bool triggered = false;

        foreach (CardEntity card in cards)
        {
            if (card == null || card.cardState.isDead) continue;

            foreach (CardMaterialInfo material in card.CardInfo.materialInfoArray)
            {
                if (material == null) continue;

                for (int i = 0; i < material.Intents.Length; i++)
                {
                    Intent intent = material.Intents[i];
                    if (intent.action.type != MaterialAction.ActionType.Attack) continue;

                    intent.action.value += value;
                    material.Intents[i] = intent;
                    triggered = true;
                }
            }
        }

        return triggered;
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
        return $"战斗开始，所有单位伤害增加{value}点";
    }
}
