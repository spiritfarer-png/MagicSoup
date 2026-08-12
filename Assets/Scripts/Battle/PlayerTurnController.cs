using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerTurnController : MonoBehaviour
{
    private static PlayerTurnController instance;
    public static PlayerTurnController Instance
    {
        get { return instance; }
    }

    // 每回合限制标记
    public bool isUseForceAction;     // 本回合是否已无视条件让角色行动
    public bool isUsePotion;          // 本回合是否已使用药水

    // 药水资源
    public List<PotionData> potionMgrs;
    public PotionData selectedPotion; // 当前选中的药水

    // 玩家当前交互模式
    public enum InteractionMode
    {
        Normal,                     // 正常模式（点击敌人可切换集火）
        SelectingAllyForPotion,     // 正在选择药水使用对象（等待点击己方角色）
        SelectingAllyForForceAction // 正在选择强制行动对象（等待点击己方角色）
    }
    public InteractionMode currentMode = InteractionMode.Normal;

    // 当前选中的己方角色下标/索引
    private int currentSelectedIndex = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 只有处于选择己方角色的模式时，才持续侦听键盘输入
        if (currentMode == InteractionMode.SelectingAllyForPotion ||
            currentMode == InteractionMode.SelectingAllyForForceAction)
        {
            HandleKeyboardInput();
        }
    }

    public void ResetTurnFlags()
    {
        isUseForceAction = false;
        isUsePotion = false;
    }

    //强行让指定单位行动
    //public bool ForceUnitAct(BattleUnit targetUnit, BattleUnit targetEnemy)
    //{
    //    if (isUseForceAction) return false; // 本回合已用过
    //    if (targetUnit == null || targetUnit.isDead) return false;

    //    targetUnit.ExecuteAction(targetEnemy);
    //    isUseForceAction = true;
    //    return true;
    //}

    //确认使用药水
    //public bool UsePotion(PotionData potion, BattleUnit targetUnit)
    //{
    //    print("使用了药水 数量减1");

    //    if (isUsePotion) return false; // 本回合已用过
    //    if (potion == null || potion.potionCount <= 0) return false;

    //    应用药水效果
    //    ApplyPotionEffect(potion, targetUnit);

    //    potion.potionCount--;
    //    isUsePotion = true;
    //    return true;
    //}

    private void ApplyPotionEffect(PotionData potion, BattleUnit targetUnit)
    {
        print("使用药水" + potion + "给" + targetUnit);
        // 药水效果具体数值计算：治疗、加攻击等
    }

    // 点击 UI 按钮：使用药水
    public void RequestUsePotion(PotionData potion)
    {
        if (BattleMgr.Instance.currentPhase != BattleMgr.BattlePhase.PlayerAction) return;

        if (isUsePotion)
        {
            Debug.Log("本回合已使用过药水");
            return;
        }

        if (potion == null || potion.potionCount <= 0)
        {
            Debug.Log("药水数量不足");
            return;
        }

        selectedPotion = potion;
        currentMode = InteractionMode.SelectingAllyForPotion;



        // 默认将下标指向索引 0 的角色，并开启光圈/下标
        currentSelectedIndex = 0;
        UpdateAllySelectionIndicators();

       // Debug.Log("按下 A/D 切换角色，Space 确认；或点击角色选择/再次点击确认");

        //Debug.Log("点击选择一个 己方角色 使用药水 (再次点击按钮可取消)");
    }

    // 点击 UI 按钮：请求强制行动
    public void RequestForceAction()
    {
        if (BattleMgr.Instance.currentPhase != BattleMgr.BattlePhase.PlayerAction) return;

        if (isUseForceAction)
        {
            Debug.Log("本回合已使用过强制行动");
            return;
        }

        currentMode = InteractionMode.SelectingAllyForForceAction;

        // 默认将下标指向索引 0 的角色，并开启光圈/下标
        currentSelectedIndex = 0;
        UpdateAllySelectionIndicators();

        Debug.Log("按下 A/D 切换角色，Space 确认；或点击角色选择/再次点击确认");
    }

    // 取消当前选择模式
    public void CancelSelectionMode()
    {
        currentMode = InteractionMode.Normal;
        selectedPotion = null;
        ClearAllAllyIndicators();
        Debug.Log("已取消选择模式");
    }

    // 统一处理场景中单位的点击回调
    public void OnUnitClicked(BattleUnit clickedUnit)
    {
        if (BattleMgr.Instance.currentPhase != BattleMgr.BattlePhase.PlayerAction) return;

        // 1. 如果点击的是 敌方角色
        if (clickedUnit.isEnemy)
        {
            // 在普通模式下点击敌人  切换集火目标
            if (currentMode == InteractionMode.Normal)
            {
                BattleMgr.Instance.ToggleFocusEnemy(clickedUnit);
            }
            else
            {
                Debug.Log("当前处于技能/药水选择模式，请选择己方角色");
            }
            return;
        }

        // 2. 如果点击的是 己方角色
        if (!clickedUnit.isEnemy)
        {
            if (currentMode == InteractionMode.SelectingAllyForPotion ||
                currentMode == InteractionMode.SelectingAllyForForceAction)
            {
                int clickedIndex = BattleMgr.Instance.playerDeployedUnits.IndexOf(clickedUnit);
                if (clickedIndex < 0) return;

                // 如果点击的就是当前已经选中的角色 -> 再次点击确认使用
                if (clickedIndex == currentSelectedIndex)
                {
                    ConfirmCurrentSelection();
                }
                // 如果点击的是其他角色 -> 将下标移动到该角色
                else
                {
                    currentSelectedIndex = clickedIndex;
                    UpdateAllySelectionIndicators();
                }
            }
            else
            {
                Debug.Log($"点击了己方角色: {clickedUnit.baseData.minionName}");
            }
        }

    }
    private void ExecutePotion(BattleUnit targetAlly)
    {
        if (selectedPotion == null || targetAlly.isDead) return;

        // 应用效果并扣除数量
        ApplyPotionEffect(selectedPotion, targetAlly);
        selectedPotion.potionCount--;
        isUsePotion = true;

        Debug.Log($"成功对 {targetAlly.baseData.minionName} 使用了药水！");

        // 重置交互模式并关闭光圈
        CancelSelectionMode();
    }

    private void ExecuteForceAction(BattleUnit targetAlly)
    {
        if (targetAlly.isDead) return;

        BattleUnit targetEnemy = BattleMgr.Instance.GetPlayerTarget();

        if (targetEnemy != null)
        {
            targetAlly.ExecuteAction(targetEnemy);
            isUseForceAction = true;

            Debug.Log($"成功强制 {targetAlly.baseData.minionName} 发动攻击！");

            // 重置交互模式并关闭光圈
            CancelSelectionMode();
        }
        else
        {
            Debug.Log("场上没有可攻击的敌方目标！");
        }
    }

    private void HandleKeyboardInput()
    {
        var units = BattleMgr.Instance.playerDeployedUnits;
        if (units == null || units.Count == 0) return;

        // A 键：下标左移
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentSelectedIndex > 0)
            {
                currentSelectedIndex--;
                UpdateAllySelectionIndicators();
            }
        }
        // D 键：下标右移
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentSelectedIndex < units.Count - 1)
            {
                currentSelectedIndex++;
                UpdateAllySelectionIndicators();
            }
        }
        // Space 键：确认使用
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmCurrentSelection();
        }
        // Esc 键：取消选择
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSelectionMode();
        }
    }

    // 确认选中的目标并执行对应逻辑
    private void ConfirmCurrentSelection()
    {
        var units = BattleMgr.Instance.playerDeployedUnits;
        if (currentSelectedIndex < 0 || currentSelectedIndex >= units.Count) return;

        BattleUnit targetAlly = units[currentSelectedIndex];

        if (currentMode == InteractionMode.SelectingAllyForPotion)
        {
            ExecutePotion(targetAlly);
        }
        else if (currentMode == InteractionMode.SelectingAllyForForceAction)
        {
            ExecuteForceAction(targetAlly);
        }
    }

    private void UpdateAllySelectionIndicators()
    {
        var units = BattleMgr.Instance.playerDeployedUnits;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].SetFocusIndicator(i == currentSelectedIndex);
            }
        }
    }

    // 清除所有己方角色的下标光圈
    private void ClearAllAllyIndicators()
    {
        var units = BattleMgr.Instance.playerDeployedUnits;
        if (units == null) return;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].SetFocusIndicator(false);
            }
        }
    }
}
