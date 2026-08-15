using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SeaweedClumpRelic", menuName = "ScriptableObject/素材数据/遗物/SeaweedClumpRelic")]

public class SeaweedClumpRelic : CardMaterialData, IRelic 
{
    [Header("海藻团块")]

    [SerializeField] int value = 1;

    public CardMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager, bool isEnemy)
    {
        CardEntity[] cards = isEnemy ? battleManager.EnemyEntities : battleManager.PlayerEntities;
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
    public bool OnCardAction(BattleManager battleManager, bool isEnemy, CardEntity actingCard)
    {
        return false;
    }

    public string GetRelicInfo()
    {
        return "战斗开始时所有友方目标的进攻意图加1伤害";
    }
}
