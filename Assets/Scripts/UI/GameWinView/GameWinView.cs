using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameWinView : UIView
{

    [SerializeField] Button quit;
    [SerializeField] Button restart;

    protected override void OnOpen(object param)
    {
        quit.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("点击音效");
            Application.Quit();
        });

        restart.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("点击音效");
            BattleManager.instance.ResartGame();
        }
        );
    }

    protected override void OnClose()
    {
        restart.onClick.RemoveAllListeners();
    }
}
