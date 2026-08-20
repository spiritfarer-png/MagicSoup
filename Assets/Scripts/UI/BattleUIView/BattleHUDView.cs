using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUDView : UIView
{
    [SerializeField] TextMeshProUGUI clockText;
    [SerializeField] private Button extraActionButton;
    [SerializeField] private Button endRoundButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private RectTransform enemyRelicSlotParent;
    [SerializeField] private RectTransform playerRelicSlotParent;
    [SerializeField] private BattleHUDRelicSlot relicSlotPrefab;

    [SerializeField] private RectTransform potionSlotParent;
    [SerializeField] private BattleHUDPotionSlot potionSlotPrefab;


    private BattleManager battle;
    protected override void OnOpen(object param)
    {
        AudioManager.Instance.PlayBGM("战斗音乐");
        base.OnOpen(param);
        battle = (BattleManager)param;
        extraActionButton.onClick.AddListener(battle.RequestExtraAction);
        endRoundButton.onClick.AddListener(battle.EndPlayerTurn);
        cancelButton.onClick.AddListener(battle.CancelExtraAction);
        battle.PhaseChanged += Refresh;
        BattleClock.OnClockChanged += OnClockChanged;
        ConfigureTooltip(extraActionButton, "点击后选择一张我方卡牌额外行动一次。");
        ConfigureTooltip(cancelButton, "取消额外行动。");


        clockText.text = BattleClock.currentTime.ToString();

        // 遗物槽
        foreach(var relic in battle.PlayerRelics)
        {
            var slot = Instantiate(relicSlotPrefab,playerRelicSlotParent);
            slot.Inititalize(relic);
        }
        foreach(var relic in battle.EnemyRelics)
        {
            var slot = Instantiate(relicSlotPrefab, enemyRelicSlotParent);
            slot.Inititalize(relic);
        }

        // 药水槽
        foreach(var potion in battle.Potions)
        {
            var slot = Instantiate(potionSlotPrefab, potionSlotParent);
            slot.Inititalize(potion);
        }
    }

    private static void ConfigureTooltip(Button button, string text)
    {
        if (button == null) return;
        TooltipTrigger trigger = button.GetComponent<TooltipTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<TooltipTrigger>();
        trigger.Configure(text);
    }

    private void OnClockChanged(int time)
    {
        clockText.text = "当前时间"+time.ToString();
    }

    private void Refresh(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.PlayerDecision:
                {
                    extraActionButton.interactable = battle.ExtraActionAvailable;
                    endRoundButton.interactable = true;
                    cancelButton.interactable = false;
                    break;
                }
            case BattlePhase.SelectingExtraAction:
                {
                    extraActionButton.interactable = false;
                    endRoundButton.interactable = true;
                    cancelButton.interactable = true;
                    break;
                }
            default:
                {
                    extraActionButton.interactable = false;
                    endRoundButton.interactable = false;
                    cancelButton.interactable = false;

                    break;
                }
        }
    }

    protected override void OnClose()
    {
        BattleClock.OnClockChanged -= OnClockChanged;
        battle.PhaseChanged -= Refresh;
        extraActionButton.onClick.RemoveAllListeners();
        endRoundButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        foreach(var slot in playerRelicSlotParent.GetComponentsInChildren<BattleHUDRelicSlot>())
        {
            Destroy(slot.gameObject);
        }
        foreach(var slot in enemyRelicSlotParent.GetComponentsInChildren<BattleHUDRelicSlot>())
        {
            Destroy(slot.gameObject);
        }

        foreach(var slot in potionSlotParent.GetComponentsInChildren<BattleHUDPotionSlot>())
        {
            Destroy(slot.gameObject);
        }

        base.OnClose();
    }

}
