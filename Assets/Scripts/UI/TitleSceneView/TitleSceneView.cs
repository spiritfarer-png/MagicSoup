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
        Application.Quit();
    }

    void StartGameButtonClick()
    {
        SceneManager.LoadScene("GamePlayScene");
    }

    private void OnDestroy()
    {
        quitButton?.onClick.RemoveListener(QuitButtonClick);
        startGameButton?.onClick.RemoveListener(StartGameButtonClick);
    }
}
