using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleWinView : UIView
{
    // todo:战利品
    [SerializeField] Button next;
    [SerializeField] RectTransform lootParent;
    [SerializeField] BattleWinLootSlot lootSlotPrefab;
    private CardMaterialData[] loots;
    protected override void OnOpen(object param)
    {
        loots = (CardMaterialData[])param;
        foreach(var loot in loots)
        {
            if (loot != null)
            {
                var lootSlot = Instantiate(lootSlotPrefab, lootParent);
                lootSlot.Initialize(loot);
            }
        }
        next.onClick.AddListener(NextButtonClick);
    }

    void NextButtonClick()
    {
        MapManager.Instance.CompleteCurrentNode();
        MapManager.Instance.OpenMap();
        UIManager.instance.Open<InventoryPanelUI>();
        CloseSelf();
    }

    protected override void OnClose()
    {
        foreach(var loot in lootParent.GetComponentsInChildren<BattleWinLootSlot>())
        {
            Destroy(loot.gameObject);
        }
        next.onClick.RemoveAllListeners();
    }
}
