using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleClock : MonoBehaviour
{
    //当前时钟时刻
    public int currentTime = 0;
    //每回合时针走多少
    public int stepPerTurn = 0;

    //时针运动规则
    public void AdvanceClock()
    {
        currentTime = currentTime + stepPerTurn;
    }

}
