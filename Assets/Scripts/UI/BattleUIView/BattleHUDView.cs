using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUDView : UIView
{
    [SerializeField] private RectTransform shortPointer;
    [SerializeField] private RectTransform longPointer;
    [SerializeField] private float clockTweenDuration = 0.3f;
    [SerializeField] private Button extraActionButton;
    [SerializeField] private Button endRoundButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private RectTransform enemyRelicSlotParent;
    [SerializeField] private RectTransform playerRelicSlotParent;
    [SerializeField] private BattleHUDRelicSlot relicSlotPrefab;

    [SerializeField] private RectTransform potionSlotParent;
    [SerializeField] private BattleHUDPotionSlot potionSlotPrefab;
    private Quaternion shortPointerIniRotation;
    private Quaternion longPointerIniRotation;

    private BattleManager battle;

    private void Awake()
    {
        shortPointerIniRotation = shortPointer.localRotation;
        longPointerIniRotation = longPointer.localRotation;
    }
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
        SetClockTime(BattleClock.currentTime, false);

        ConfigureTooltip(extraActionButton, "点击后选择一张我方卡牌额外行动一次。");
        ConfigureTooltip(cancelButton, "取消额外行动。");


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
        SetClockTime(time);
    }

    private void SetClockTime(int time, bool animated = true)
    {
        Quaternion longTarget = longPointerIniRotation * Quaternion.Euler(0f, 0f, -time * 30f);
        Quaternion shortTarget = shortPointerIniRotation * Quaternion.Euler(0f, 0f, -BattleClock.totalTime * 2.5f);

        longPointer.DOKill();
        shortPointer.DOKill();

        if (!animated)
        {
            longPointer.localRotation = longTarget;
            shortPointer.localRotation = shortTarget;
            return;
        }

        longPointer.DOLocalRotateQuaternion(longTarget, clockTweenDuration).SetEase(Ease.OutCubic);
        shortPointer.DOLocalRotateQuaternion(shortTarget, clockTweenDuration).SetEase(Ease.OutCubic);
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

        longPointer.DOKill();
        shortPointer.DOKill();

        foreach (var slot in playerRelicSlotParent.GetComponentsInChildren<BattleHUDRelicSlot>())
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
