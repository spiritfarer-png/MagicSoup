using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelic
{
    CardMaterialData MaterialData { get; }
    bool OnBattleStart(BattleManager battleManager,bool isEnemy);
    bool OnRoundStart(BattleManager battleManager, bool isEnemy);
    bool OnCardAction(BattleManager battleManager, bool isEnemy,CardEntity actingCard);

    string GetRelicInfo();
}
