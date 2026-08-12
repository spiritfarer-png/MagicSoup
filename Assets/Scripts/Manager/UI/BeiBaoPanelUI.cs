using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeiBaoPanelUI : MonoBehaviour
{
    public static BeiBaoPanelUI Instance { get; private set; }

    [Header("格子预制体")]
    public GameObject slotPrefab; // 挂载了 ItemSlotUI 的预制体

    [Header("三个区域的 Content (添加了 Grid Layout Group 组件)")]
    public Transform materialGridContent; // 素材区 UI 父节点
    public Transform craftingGridContent; // 合成区 UI 父节点
    public Transform minionGridContent;   // 随从区 UI 父节点
    public Transform deployedGridContent; // 上阵区 UI 父节点

    [Header("合成控制")]
    public Button btnCraft;               // 合成角色按钮
    public List<RecipeData> recipeList;   // 所有的合成配方
    
    [Header("关闭按钮")]
    public Button btnClose;
    

    // 内部保存生成的 UI 格子实例
    private List<ItemSlotUI> materialUIList = new List<ItemSlotUI>();
    private List<ItemSlotUI> craftingUIList = new List<ItemSlotUI>();
    private List<ItemSlotUI> minionUIList = new List<ItemSlotUI>();
    private List<ItemSlotUI> deployedUIList = new List<ItemSlotUI>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnAllSlotsUI();
        RefreshAllUI();

        if (btnCraft != null)
        {
            btnCraft.onClick.AddListener(OnCraftButtonClicked);
        }
        btnClose.onClick.AddListener(() => { 
           this.gameObject.SetActive(false);
        });

    }

    // 1. 实例化生成全部 UI 格子节点
    private void SpawnAllSlotsUI()
    {
        var mgr = BeiBaoMgr.Instance;

        // 实例素材区格子
        for (int i = 0; i < mgr.maxMaterialSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, materialGridContent);
            ItemSlotUI slotUI = obj.GetComponent<ItemSlotUI>();
            materialUIList.Add(slotUI);
        }

        // 实例化合成区格子
        for (int i = 0; i < mgr.maxCraftingSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, craftingGridContent);
            ItemSlotUI slotUI = obj.GetComponent<ItemSlotUI>();
            craftingUIList.Add(slotUI);
        }

        // 实例化随从区格子
        for (int i = 0; i < mgr.maxMinionSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, minionGridContent);
            ItemSlotUI slotUI = obj.GetComponent<ItemSlotUI>();
            minionUIList.Add(slotUI);
        }

        // 实例化上阵区格子
        for (int i = 0; i < mgr.maxDeployedSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, deployedGridContent);
            deployedUIList.Add(obj.GetComponent<ItemSlotUI>());
        }
    }

    // 2. 刷新整个背包 UI（将 数据 逐个写入 UI 格子）
    public void RefreshAllUI()
    {
        var mgr = BeiBaoMgr.Instance;

        // 刷新素材区 UI
        for (int i = 0; i < materialUIList.Count; i++)
        {
            materialUIList[i].UpdateMaterialSlot(
                mgr.materialSlots[i],
                i,
                ItemSlotUI.SlotType.MaterialArea
            );
        }

        // 刷新合成区 UI
        for (int i = 0; i < craftingUIList.Count; i++)
        {
            craftingUIList[i].UpdateMaterialSlot(
                mgr.craftingSlots[i],
                i,
                ItemSlotUI.SlotType.CraftingArea
            );
        }

        // 刷新随从区 UI
        for (int i = 0; i < minionUIList.Count; i++)
        {
            minionUIList[i].UpdateMinionSlot(
                mgr.minionSlots[i],
                i , ItemSlotUI.SlotType.MinionArea
            );
        }

        // 刷新上阵区 UI
        for (int i = 0; i < deployedUIList.Count; i++)
        {
            deployedUIList[i].UpdateMinionSlot(
                mgr.deployedMinionSlots[i], i, ItemSlotUI.SlotType.DeployedArea);
        }
    }

    // 3. 点击某个格子的回调处理
    public void OnSlotClicked(ItemSlotUI.SlotType slotType, int index)
    {
        var mgr = BeiBaoMgr.Instance;

        switch (slotType)
        {
            case ItemSlotUI.SlotType.MaterialArea:
                // 点击素材区 -> 把素材放入合成区
                mgr.MoveMaterialToCrafting(index);
                break;

            case ItemSlotUI.SlotType.CraftingArea:
                // 点击合成区 -> 把素材放回素材区
                mgr.MoveCraftingToMaterial(index);
                break;

            case ItemSlotUI.SlotType.MinionArea:
                // 点击随从区 -> 可以做准备上阵选中等
                Debug.Log($"点击了随从格 [{index}]，随从名：{mgr.minionSlots[index]?.minionName}");
                break;
        }

        // 修改完数据后，重新渲染 UI
        RefreshAllUI();
    }

    // 点击合成按钮事件
    private void OnCraftButtonClicked()
    {
        // 匹配 RecipeData，合成随从逻辑...
        for (int i = 0; i < recipeList.Count; i++)
        {
            BeiBaoMgr.Instance.TryCraftMinion(recipeList[i]);

        }
        Debug.Log("点击了合成随从按钮");
        RefreshAllUI();
    }
}
