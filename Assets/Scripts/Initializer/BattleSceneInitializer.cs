using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneInitializer : MonoBehaviour
{
    [Header("战斗场景专属生成点")]
    public Transform[] playerSpawnPoints; // 己方生成点
    public Transform[] enemySpawnPoints;  // 敌方生成点 (预留)
    
    public List<BattleUnit> enemyUnits; // 敌方角色单位

    private void Start()
    {
        if (BattleMgr.Instance != null)
        {
            // 1. 将当前战斗场景里有效的生成点绑定给单例 BattleMgr
            BattleMgr.Instance.playerSpawnPoints = playerSpawnPoints;


            BattleMgr.Instance.enemyDeployedUnits = enemyUnits;

            // 2. 触发进入准备阶段，自动实例化生成角色模型
            BattleMgr.Instance.ChangePhase(BattleMgr.BattlePhase.Prep);
        }
        else
        {
            Debug.LogError("未找到 BattleMgr 实例，请检查启动流程！");
        }
    }
}
