using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginBattle : MonoBehaviour
{
    public Button btnBeginBattle;
    public Button btnInventory;
    
    public GameObject panelBeiBao;


    private void Start()
    {
        if (btnBeginBattle != null) btnBeginBattle.gameObject.SetActive(false);
        btnInventory.onClick.AddListener(() => {
          panelBeiBao.SetActive(true);
        });
    }
}
