using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomEventOptionView : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text optionText;
    private RandomEventOptionData optionData;
    private Action<RandomEventOptionData> onClicked;

    public void Initialize(RandomEventOptionData data, Action<RandomEventOptionData> clickedCallback)
    {
        optionData = data;
        onClicked = clickedCallback;

        optionText.text = optionData.optionText;
        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
    }

    private void HandleClicked()
    {
        button.interactable = false;
        onClicked?.Invoke(optionData);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();

        onClicked = null;
        optionData = null;
    }
}