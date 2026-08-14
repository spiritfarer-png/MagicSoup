using System;
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
        btnBeginBattle.onClick.AddListener(() =>
        {
            if (MapManager.Instance.CurrentMap == null)
            {
                int seed = DateTime.Now.Millisecond;
                MapManager.Instance.GenerateNewMap(seed);
            }

            MapManager.Instance.OpenMap();
        });
        btnBeiBao.onClick.AddListener(() =>
        {
            panelBeiBao.SetActive(true);
        });
    }
}
