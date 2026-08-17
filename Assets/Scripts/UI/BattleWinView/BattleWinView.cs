using DG.Tweening;
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
    private SoupMaterialData[] loots;
    protected override void OnOpen(object param)
    {
        loots = (SoupMaterialData[])param;
        foreach(var loot in loots)
        {
            if (loot != null)
            {
                var lootSlot = Instantiate(lootSlotPrefab, lootParent);
                lootSlot.Initialize(loot);
            }
        }
        next.onClick.AddListener(NextButtonClick);

        PlayOpenTween();
    }



    void NextButtonClick()
    {
        MapManager.Instance.CompleteCurrentNode();
        PlayCloseTween(() =>
        {
            MapManager.Instance.OpenMap();
            UIManager.instance.Open<InventoryPanelUI>();
            CloseSelf();
        });
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
