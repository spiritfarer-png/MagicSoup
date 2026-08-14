using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginBattle : MonoBehaviour
{
    public Button btnBeginBattle;
    public Button btnInventory;

    private void Start()
    {
        /*btnBeginBattle.onClick.AddListener(() =>
        {
            if (MapManager.Instance.CurrentMap == null)
            {
                int seed = DateTime.Now.Millisecond;
                MapManager.Instance.GenerateNewMap(seed);
            }

            MapManager.Instance.OpenMap();
        });*/

        btnBeginBattle.onClick.AddListener(() => SceneManager.LoadScene("TestBattleScene"));

        if (btnInventory != null)
        {
            btnInventory.onClick.AddListener(() =>
                UIManager.instance.Open<InventoryPanelUI>());
        }
    }
}
