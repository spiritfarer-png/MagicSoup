using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleClock
{
    public static int currentTime = 0;
    public static int maxTime = 12;

    public static Action<int> OnClockChanged;

    //时针运动规则
    public static void AdvanceClock()
    {
        currentTime = (currentTime+1) % maxTime;
        OnClockChanged?.Invoke(currentTime);
    }

}
