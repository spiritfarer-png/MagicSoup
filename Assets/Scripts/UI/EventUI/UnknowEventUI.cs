using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnknowEventUI : UIView
{
    public Image icon;
    public Text eventName;
    public Text eventIntroduct;
    public List<Button> optionButtons = new List<Button>();
    public List<Text> optionButtonsName = new List<Text>();

    //public Button btnOption1;
    //public Button btnOption2;
    //public Button btnOption3;

    protected override void OnOpen(object param)
    {
        if (param is UnknowEventData data)
        {
            eventName = data.UnknowEventName;
            eventIntroduct = data.UnknowEventIntroduct;

            for (int i = 0; i < optionButtons.Count; i++)
            {
                if(i < data.UnknowEventOptions.Count)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtonsName[i].text = data.UnknowEventOptions[i].optionName;

                    optionButtons[i].onClick.RemoveAllListeners();

                    Action onClickAction = data.UnknowEventOptions[i].optionClicked;
                    optionButtons[i].onClick.AddListener(() => {
                        onClickAction?.Invoke();
                        }
                        );
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
                optionButtons[i].onClick.AddListener(() => { });
            }
        }


    }

    override protected void OnClose()
    {
        foreach (var btn in optionButtons)
        {
            btn.onClick.RemoveAllListeners();
        }
    }
}
