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
    public int maxHealth;
    public int currentHealth;
    public int defence;
    public bool isDead { get { return currentHealth <= 0; } }
    public BattleCardState(CardInfo cardInfo, bool isEnemy)
    {
        this.cardInfo = cardInfo;
        this.isEnemy = isEnemy;
        maxHealth = cardInfo.MaxHealth;
        currentHealth = maxHealth;
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

    public int Heal(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        return currentHealth - previousHealth;
    }

    public void Defence(int amount) 
    {
        defence += amount;
    }
}
