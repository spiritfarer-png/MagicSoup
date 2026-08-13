using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗内数据。每场战斗重置。
/// </summary>
public class BattleCardState
{
    public CardInfo cardInfo;
    public bool isEnemy;
    public int currentHealth;
    public int defence;
    public bool isDead { get { return currentHealth <= 0; } }
    public BattleCardState(CardInfo cardInfo, bool isEnemy)
    {
        this.cardInfo = cardInfo;
        this.isEnemy = isEnemy;
        currentHealth = cardInfo.iniHealth;
        defence = 0;
    }

    public void TakeDamage(int amount)
    {
        defence -= amount;
        if (defence < 0) 
        {
            currentHealth += defence;
            defence = 0;
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
    }

    public void Defence(int amount) 
    {
        defence += amount;
    }
}
