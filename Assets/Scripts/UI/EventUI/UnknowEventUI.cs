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

    private UnknowEventData _currentEventData;

    protected override void OnOpen(object param)
    {
        if (param is UnknowEventData data)
        {
            RefreshUI(data);
        }
    }

    private void RefreshUI(UnknowEventData data)
    {
        _currentEventData = data;

        eventName.text = data.UnknowEventName;
        eventIntroduct.text = data.UnknowEventContent;

        if (data.UnknowEventIcon != null)
        {
            icon.gameObject.SetActive(true);
            icon.sprite = data.UnknowEventIcon;
        }
        else
        {
            icon.gameObject.SetActive(false);
        }


        //刷新选项列表
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < data.UnknowEventOptions.Count)
            {
                UnknowEventOptionData optionData = data.UnknowEventOptions[i];
                optionButtons[i].gameObject.SetActive(true);
                optionButtonsName[i].text = optionData.optionName;

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => {
                    OnOptionSelect(optionData);
                });

            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

    }

    //选项点击后响应
    private void OnOptionSelect(UnknowEventOptionData option)
    {
        ExecuteOptionAction(option.actionType, option.actionValue, option.actionText);
        
        if(option.nextEventData != null)
        {
            RefreshUI(option.nextEventData);
        }
        else
        {
            CloseSelf();
        }

    }

    //不同选项具体逻辑，后续如果要写单例，可以换到新写的单例里
    private void ExecuteOptionAction(UnknowEventActionType type, int value, string param)
    {
        switch (type)
        {
             case UnknowEventActionType.None:
                break;
                //....
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
