using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleUnit : MonoBehaviour ,IPointerClickHandler
{
    public MinionData baseData;            // 引用的角色基础配置

    // 运行时属性
    public int currentHp;
    public bool isEnemy;                   // 是否是敌方角色
    public bool isDead;                    // 是否死亡

    // UI 选中提示 选中的角色脚下放个光圈
    public GameObject focusIndicator;

    // 状态检测 true可行动
    public bool CanAct(int currentClockTime)
    {
        if (isDead) return false;
        // 校验当前时刻是否满足 minionData 中的 validClockTimes 或 clockModulo 条件
        return CheckActionCondition(currentClockTime);
    }

    private void Start()
    {
        InitUnit(baseData, isEnemy);
    }

    ///初始化单位
    public void InitUnit(MinionData data, bool enemyStatus)
    {
        baseData = data;
        isEnemy = enemyStatus;
        currentHp = data.maxHp;
        isDead = false;
        SetFocusIndicator(false);
    }

    // 内部行动规则判定
    private bool CheckActionCondition(int currentClockTime)
    {
        if (baseData == null) return false;

        //落在指定时刻列表内行动 (例如 2, 5, 8 点)
        if (baseData.validClockTimes != null && baseData.validClockTimes.Contains(currentClockTime))
        {
            return true;
        }

        //按周期整除 (例如每 3 步触发一次)
        if (baseData.clockModulo > 0 && (currentClockTime % baseData.clockModulo == 0))
        {
            return true;
        }
        //其它的后续可以再加


        return false;
    }

    // 执行行动（攻击/技能）
    public void ExecuteAction(BattleUnit target)
    {
        if (isDead || target == null) return;

        // 具体的伤害计算逻辑 (预留)
        int damage = CalculateDamage(target);
        target.TakeDamage(damage);
    }

    private int CalculateDamage(BattleUnit target)
    {
        // 伤害计算公式，后续可修改
        return Mathf.Max(1, baseData.attack - target.baseData.defense);
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        // 播放死亡动画/清理节点
    }

    public void SetFocusIndicator(bool active)
    {
        if (focusIndicator != null)
        {
            focusIndicator.gameObject.SetActive(active);
        }
    }

    //处理UI点击逻辑
    public void OnPointerClick(PointerEventData eventData)
    {
       // 将点击事件传给交互管理器统一处理
        PlayerTurnController.Instance.OnUnitClicked(this);
    }
}
