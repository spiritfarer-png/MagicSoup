using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneView:MonoBehaviour
{
    [SerializeField] Button quitButton;
    [SerializeField] Button startGameButton;

    private void Start()
    {
        AudioManager.Instance.PlayBGM("±≥æ∞“Ù¿÷");
        quitButton.onClick.AddListener(QuitButtonClick);
        startGameButton.onClick.AddListener(StartGameButtonClick);
    }

    void QuitButtonClick()
    {
        AudioManager.Instance.PlaySFX("µ„ª˜“Ù–ß");
        Application.Quit();
    }

    void StartGameButtonClick()
    {
        AudioManager.Instance.PlaySFX("µ„ª˜“Ù–ß");
        SceneManager.LoadScene("GamePlayScene");
    }

    private void OnDestroy()
    {
        quitButton?.onClick.RemoveListener(QuitButtonClick);
        startGameButton?.onClick.RemoveListener(StartGameButtonClick);
    }
}
