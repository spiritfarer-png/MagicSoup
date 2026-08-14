using System;
using UnityEngine;
using UnityEngine.UI;

public class BeginBattle : MonoBehaviour
{
    public Button btnBeginBattle;
    public Button btnInventory;

    private void Start()
    {
        btnBeginBattle.onClick.AddListener(OnBeginBattleClicked);

        if (btnInventory != null)
        {
            btnInventory.onClick.AddListener(() =>
                UIManager.instance.Open<InventoryPanelUI>());
        }
    }

    private void OnBeginBattleClicked()
    {
        if (MapManager.Instance.CurrentMap == null)
        {
            MapManager.Instance.StartNewMap(Environment.TickCount);
            return;
        }

        MapManager.Instance.OpenMap();
    }
}
