using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginBattle : MonoBehaviour
{
    public Button btnBeginBattle;
    public Button btnBeiBao;
    
    public GameObject panelBeiBao;


    private void Start()
    {
        btnBeginBattle.onClick.AddListener(() => {
            SceneManager.LoadScene("BattleScene");

        });
        btnBeiBao.onClick.AddListener(() => {
          panelBeiBao.SetActive(true);
        });
    }
}
