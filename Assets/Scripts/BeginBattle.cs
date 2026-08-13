using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginBattle : MonoBehaviour
{
    public Button btnBeginBattle;
    public Button btnInventory;
    
    private void Start()
    {
        btnBeginBattle.onClick.AddListener(()=>SceneManager.LoadScene("TestBattleScene"));
        if (btnInventory != null) btnInventory.onClick.AddListener(() => UIManager.instance.Open<InventoryPanelUI>());
    }
}
