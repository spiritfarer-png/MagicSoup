using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Slider hpSlider;          // 血条滑动条
    public Text txtHp;               // 血量数字 (例如: 80/100)
    public Transform statusContainer;// 状态图标的父节点 (加了 Horizontal Layout Group)
    public GameObject statusIconPrefab; // 状态图标预制体 (用于动态生成状态)

    // 1. 更新血条显示 当前血量和最大血量
    public void UpdateHP(int currentHp, int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (txtHp != null)
        {
            txtHp.text = $"{currentHp}/{maxHp}";
        }
    }

    // 2. 刷新状态栏图标（预留状态系统接口）
    public void UpdateStatusIcons(List<Sprite> statusSprites)
    {
        if (statusContainer == null || statusIconPrefab == null) return;

        // 清除旧状态图标
        foreach (Transform child in statusContainer)
        {
            Destroy(child.gameObject);
        }

        // 生成新状态图标
        if (statusSprites != null)
        {
            foreach (var sprite in statusSprites)
            {
                GameObject iconObj = Instantiate(statusIconPrefab, statusContainer);
                Image img = iconObj.GetComponent<Image>();
                if (img != null) img.sprite = sprite;
            }
        }
    }
}
