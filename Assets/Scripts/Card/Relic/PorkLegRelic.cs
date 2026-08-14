using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PorkLegRelic", menuName = "ScriptableObject/素材数据/遗物/PorkLegRelic")]
[System.Serializable]
public class PorkLegRelic:CardMaterialData,IRelic
{
    [Header("精猪后腿肉")]
    [SerializeField] int defence = 4;
    public CardMaterialData MaterialData => this;
    public bool OnBattleStart(BattleManager battleManager,bool isEnemy)
    {
        CardEntity entity = battleManager.GetRandomLivingCard(isEnemy);
        if(entity == null)
            return false;
        entity.Defence(defence);
        return true;
    }
    public bool OnRoundStart(BattleManager battleManager, bool isEnemy)
    {
        return false;
    }
    public bool OnCardAction(BattleManager battleManager, bool isEnemy, CardEntity actingCard)
    {
        return false;
    }
}
