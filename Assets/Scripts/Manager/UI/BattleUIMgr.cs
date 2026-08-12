using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIMgr : MonoBehaviour
{
    [Header("按钮组件")]
    public Button btnStartTurn;         // 开始本回合战斗按钮
    public Button btnPotion;            // 使用药水按钮
    public Button btnForceAction;       // 强制行动按钮

    [Header("UI状态显示")]
    public Text txtPrompt;              // 状态/操作提示文本
    public Text txtPotionCount;         // 药水数量显示

    private void Start()
    {
        // 绑定按钮点击监听
        if (btnStartTurn != null)
            btnStartTurn.onClick.AddListener(OnStartTurnButtonClicked);

        if (btnPotion != null)
            btnPotion.onClick.AddListener(OnPotionButtonClicked);

        if (btnForceAction != null)
        {
            btnForceAction.onClick.AddListener(OnForceActionButtonClicked);
        }
    }

    private void Update()
    {
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        var battleMgr = BattleMgr.Instance;
        var turnCtrl = PlayerTurnController.Instance;

        if (battleMgr == null || turnCtrl == null) return;

        bool isPlayerTurn = (battleMgr.currentPhase == BattleMgr.BattlePhase.PlayerAction);

        // 按钮可交互状态更新
        if (btnStartTurn != null)
            btnStartTurn.interactable = isPlayerTurn;

        if (btnPotion != null)
            btnPotion.interactable = isPlayerTurn && !turnCtrl.isUsePotion;

        if (btnForceAction != null)
            btnForceAction.interactable = isPlayerTurn && !turnCtrl.isUseForceAction;

        // 药水数量更新
        if (txtPotionCount != null && turnCtrl.potionMgrs != null && turnCtrl.potionMgrs.Count > 0)
        {
            txtPotionCount.text = $"x{turnCtrl.potionMgrs[0].potionCount}";
        }

        // 操作提示更新
        if (txtPrompt != null)
        {
            switch (turnCtrl.currentMode)
            {
                case PlayerTurnController.InteractionMode.SelectingAllyForPotion:
                    txtPrompt.text = "【请选择己方角色使用药水】(再次点击取消)";
                    break;

                case PlayerTurnController.InteractionMode.SelectingAllyForForceAction:
                    txtPrompt.text = "【请选择己方角色强制行动】(再次点击取消)";
                    break;

                case PlayerTurnController.InteractionMode.Normal:
                    if (battleMgr.focusedEnemyTarget != null)
                    {
                        txtPrompt.text = $"当前集火目标：{battleMgr.focusedEnemyTarget.baseData.minionName}";
                    }
                    else
                    {
                        txtPrompt.text = "点击敌人可设置集火目标，或点击[开始战斗]";
                    }
                    break;
            }
        }
    }

    // 1. 点击“开始本回合战斗”按钮
    private void OnStartTurnButtonClicked()
    {
        // 如果还处于选择模式，先取消
        PlayerTurnController.Instance.CancelSelectionMode();
        // 状态转入 AutoAction 结算战斗
        BattleMgr.Instance.ChangePhase(BattleMgr.BattlePhase.AutoAction);
    }

    // 2. 点击“使用药水”按钮
    private void OnPotionButtonClicked()
    {
        var turnCtrl = PlayerTurnController.Instance;

        // 如果已经在选择药水模式，再次点击则取消选择
        if (turnCtrl.currentMode == PlayerTurnController.InteractionMode.SelectingAllyForPotion)
        {
            turnCtrl.CancelSelectionMode();
        }
        else
        {
            // 默认使用背包里的第一种药水
            if (turnCtrl.potionMgrs != null && turnCtrl.potionMgrs.Count > 0)
            {
                turnCtrl.RequestUsePotion(turnCtrl.potionMgrs[0]);
            }
        }
    }

    // 3. 点击“强制行动”按钮
    public void OnForceActionButtonClicked()
    {
        var turnCtrl = PlayerTurnController.Instance;

        // 如果已经在选择强制行动模式，再次点击则取消选择
        if (turnCtrl.currentMode == PlayerTurnController.InteractionMode.SelectingAllyForForceAction)
        {
            turnCtrl.CancelSelectionMode();
        }
        else
        {
            turnCtrl.RequestForceAction();
        }
    }
}
