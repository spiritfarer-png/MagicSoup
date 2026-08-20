using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : UIView
{
    [SerializeField] Button quitButton;
    [SerializeField] Button restartButton;

    protected override void OnOpen(object param)
    {
        quitButton.onClick.AddListener(() =>{
            Application.Quit();
            AudioManager.Instance.PlaySFX("点击音效");
        });
        restartButton.onClick.AddListener(RestartGame);
        PlayOpenTween();
    }

    void RestartGame()
    {
        PlayCloseTween(() =>
        {
            AudioManager.Instance.PlaySFX("点击音效");
            BattleManager.instance.ResartGame();
        });
    }

    protected override void OnClose()
    {
        restartButton.onClick.RemoveAllListeners();
    }
}
