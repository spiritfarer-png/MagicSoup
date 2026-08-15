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


    private BattleManager battle;
    protected override void OnOpen(object param)
    {
        base.OnOpen(param);
        battle = (BattleManager)param;
        extraActionButton.onClick.AddListener(battle.RequestExtraAction);
        endRoundButton.onClick.AddListener(battle.EndPlayerTurn);
        cancelButton.onClick.AddListener(battle.CancelExtraAction);
        battle.PhaseChanged += Refresh;
        BattleClock.OnClockChanged += OnClockChanged;
        clockText.text = BattleClock.currentTime.ToString();
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
        base.OnClose();
    }

}
