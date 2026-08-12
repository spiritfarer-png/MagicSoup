using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMgr : MonoBehaviour
{
    private static BattleMgr instance;
    public static BattleMgr Instance
    {
        get { return instance; }
    }

    // 战斗阶段枚举 战斗准备（开始前）  每回合开始的瞬间  玩家行动阶段  战斗阶段过场  回合结束瞬间 战斗结束 
    public enum BattlePhase { Prep, TurnStart, PlayerAction, AutoAction, TurnEnd, BattleOver }

    public BattlePhase currentPhase;

    // 阵容管理
    public int maxDeployCount = 4;              // 最大上阵人数
    public List<BattleUnit> playerDeployedUnits; // 我方上阵角色列表
    public List<BattleUnit> enemyDeployedUnits;  // 敌方上阵角色列表

    // 战斗结果
    public bool isBattleOver;
    public bool isPlayerWin;

    //指定攻击的敌方目标
    public BattleUnit focusedEnemyTarget;
    //指定强制行动的玩家对象
    public BattleUnit focusPlayerTarget;

    [Header("需要拖入组件引用")]
    public BattleClock battleClock;
    public PlayerTurnController playerController;

    [Header("站位配置")]
    public Transform[] playerSpawnPoints; // 己方角色的场景生成点 (在 Inspector 中拖拽 4 个空物体)

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    // 阶段切换状态机
    public void ChangePhase(BattlePhase newPhase)
    {
        currentPhase = newPhase;

        switch (currentPhase)
        {
            case BattlePhase.Prep:
                // 战斗准备：等待玩家上阵随从
                //一个检测按钮的方法 按了切换到回合开始
                 SyncDeployedUnitsFromInventory();
                ChangePhase(BattlePhase.TurnStart);
                break;

            case BattlePhase.TurnStart:
                if (playerDeployedUnits.Count == 0)
                {
                    SyncDeployedUnitsFromInventory();
                }
                // 重置玩家手动操作标记
                playerController.ResetTurnFlags();
                //  时钟向前走步
                battleClock.AdvanceClock();
                //  转入玩家手动操作阶段
                ChangePhase(BattlePhase.PlayerAction);
                break;

            case BattlePhase.PlayerAction:
                // 等待 UI 操作，玩家点击“结束回合”按钮后转入 AutoAction
                print("玩家操作阶段");
                break;

            case BattlePhase.AutoAction:
                // 执行符合时钟条件的自动行动
                ProcessAutoActions();
                ChangePhase(BattlePhase.TurnEnd);
                break;

            case BattlePhase.TurnEnd:
                // 检测胜负状态
                CheckBattleOverCondition();
                if (!isBattleOver)
                {
                    ChangePhase(BattlePhase.TurnStart); // 开启下一回合
                }
                else
                {
                    ChangePhase(BattlePhase.BattleOver);
                }
                break;

            case BattlePhase.BattleOver:
                // 处理结算奖励或失败UI
                break;
        }
    }

    // 自动检测并执行可行动单位的逻辑
    private void ProcessAutoActions()
    {
        int currentTime = battleClock.currentTime;

        // 己方行动检测
        foreach (var unit in playerDeployedUnits)
        {
            if (unit.CanAct(currentTime))
            {
                BattleUnit target = GetFirstAliveUnit(enemyDeployedUnits);
                if (target != null) unit.ExecuteAction(target);
            }
        }

        // 敌方行动检测
        foreach (var unit in enemyDeployedUnits)
        {
            if (unit.CanAct(currentTime))
            {
                BattleUnit target = GetFirstAliveUnit(playerDeployedUnits);
                if (target != null) unit.ExecuteAction(target);
            }
        }
    }

    //检测游戏是否结束
    private void CheckBattleOverCondition()
    {
        bool allPlayerDead = playerDeployedUnits.TrueForAll(u => u.isDead);
        bool allEnemyDead = enemyDeployedUnits.TrueForAll(u => u.isDead);

        if (allEnemyDead)
        {
            isBattleOver = true;
            isPlayerWin = true;
        }
        else if (allPlayerDead)
        {
            isBattleOver = true;
            isPlayerWin = false;
        }
    }
    private BattleUnit GetFirstAliveUnit(List<BattleUnit> units)
    {
        return units.Find(u => !u.isDead);
    }

    // 设置或取消集火目标
    public void ToggleFocusEnemy(BattleUnit enemy)
    {
        if (enemy == null || !enemy.isEnemy || enemy.isDead) return;

        // 重复点击同一个敌人  取消
        if (focusedEnemyTarget == enemy)
        {
            focusedEnemyTarget.SetFocusIndicator(false);
            focusedEnemyTarget = null;
            Debug.Log("取消集火目标");
        }
        else
        {
            // 取消上一个敌人的高亮
            if (focusedEnemyTarget != null)
            {
                focusedEnemyTarget.SetFocusIndicator(false);
            }

            // 设为新集火目标
            focusedEnemyTarget = enemy;
            focusedEnemyTarget.SetFocusIndicator(true);
            Debug.Log($"设置集火目标为: {enemy.baseData.minionName}");
        }
    }


    // 获取己方角色的攻击目标：有集火目标且存活则优先集火，否则随机攻击
    public BattleUnit GetPlayerTarget()
    {
        if (focusedEnemyTarget != null && !focusedEnemyTarget.isDead)
        {
            return focusedEnemyTarget;
        }
        // 没点敌人或集火目标已死  随机攻击任意活着的敌人
        return GetRandomAliveUnit(enemyDeployedUnits);
    }
    //实现随机攻击方法
    private BattleUnit GetRandomAliveUnit(List<BattleUnit> units)
    {
        //去除死亡单位
        List<BattleUnit> aliveUnits = units.FindAll(u => u != null && !u.isDead);
        if (aliveUnits.Count == 0) return null;
        int randomIndex = Random.Range(0, aliveUnits.Count);
        return aliveUnits[randomIndex];
    }

    public BattleUnit SetFocusPlayer(BattleUnit player)
    {
        return player;
    }


    public void UpdateDeployedMinions(MinionData minionData ,int index)
    {
        playerDeployedUnits.Add(new BattleUnit());
    }

    // 同步背包中的上阵随从数据到战斗场景中
    public void SyncDeployedUnitsFromInventory()
    {
        // 1. 销毁并清理上一次生成的己方实体
        foreach (var unit in playerDeployedUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        playerDeployedUnits.Clear();

        if (BeiBaoMgr.Instance == null) return;

        // 2. 获取背包中的上阵数据
        var deployedDataList = BeiBaoMgr.Instance.deployedMinionSlots;

        for (int i = 0; i < deployedDataList.Count; i++)
        {
            MinionData minionData = deployedDataList[i];

            // 当前上阵位置有角色数据且配置了 Prefab
            if (minionData != null && minionData.prefab != null)
            {
                // 确定生成位置（若超过站位点数量则默认生成在 BattleMgr 位置）
                Transform spawnPoint = (playerSpawnPoints != null && i < playerSpawnPoints.Length)
                    ? playerSpawnPoints[i]
                    : transform;

                // 3. 实例化角色模型预制体
                GameObject unitObj = Instantiate(minionData.prefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                // 4. 获取或自动添加 BattleUnit 组件
                BattleUnit unitComponent = unitObj.GetComponent<BattleUnit>();
                if (unitComponent == null)
                {
                    unitComponent = unitObj.AddComponent<BattleUnit>();
                }

                // 5. 初始化角色运行时属性并加入 playerDeployedUnits 列表
                unitComponent.InitUnit(minionData, enemyStatus: false);
                playerDeployedUnits.Add(unitComponent);
            }
        }

        Debug.Log($"己方战斗阵容同步完成，当前实际上阵人数：{playerDeployedUnits.Count}");
    }
}


