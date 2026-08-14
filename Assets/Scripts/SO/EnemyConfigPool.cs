using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyConfigPool", menuName = "ScriptableObject/敌人池")]
public class EnemyConfigPool : ScriptableObject
{
    [Header("小怪池")]
    [SerializeField] private EnemyConfig[] normalEnemyConfigs;
    [Header("精英池")]
    [SerializeField] private EnemyConfig[] eliteEnemyConfigs;
    [Header("Boss池子")]
    [SerializeField] private EnemyConfig[] bossEnemyConfigs;

    private List<EnemyConfig> normalEnemyConfigPool;
    private List<EnemyConfig> eliteEnemyConfigPool;
    private List<EnemyConfig> bossEnemyConfigPool;

    public void Initialize()
    {
        normalEnemyConfigPool = new List<EnemyConfig>(normalEnemyConfigs);
        eliteEnemyConfigPool = new List<EnemyConfig>(eliteEnemyConfigs);
        bossEnemyConfigPool = new List<EnemyConfig>(bossEnemyConfigs);
    }

    public EnemyConfig PopNormalEnemy()
    {
        if (normalEnemyConfigPool == null || normalEnemyConfigPool.Count == 0)
        {
            normalEnemyConfigPool = new List<EnemyConfig>(normalEnemyConfigs);
        }
        int index = Random.Range(0, normalEnemyConfigPool.Count);
        var pop = normalEnemyConfigPool[index];
        normalEnemyConfigPool.RemoveAt(index);
        return pop;
    }

    public EnemyConfig PopEliteEnemy()
    {
        if (eliteEnemyConfigPool == null || eliteEnemyConfigPool.Count == 0)
        {
            eliteEnemyConfigPool = new List<EnemyConfig>(eliteEnemyConfigs);
        }
        int index = Random.Range(0, eliteEnemyConfigPool.Count);
        var pop = eliteEnemyConfigPool[index];
        eliteEnemyConfigPool.RemoveAt(index);
        return pop;
    }

    public EnemyConfig PopBoss()
    {
        if (bossEnemyConfigPool == null || bossEnemyConfigPool.Count == 0)
        {
            bossEnemyConfigPool = new List<EnemyConfig>(bossEnemyConfigs);
        }
        int index = Random.Range(0, bossEnemyConfigPool.Count);
        var pop = bossEnemyConfigPool[index];
        bossEnemyConfigPool.RemoveAt(index);
        return pop;
    }
}
