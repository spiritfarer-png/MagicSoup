using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeiBaoMgr : MonoBehaviour
{
    private static BeiBaoMgr instance;
    public static BeiBaoMgr Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        instance = this;
        InitListCapacity();
    }

    // 同星级素材合成所需数量 n (例如3个1星合成1个2星)
    public int synthesisN = 3;                  

    // 背包三区域数据
    // 放素材区域
    public List<SingleBeiBao> materialSlots;        
    // 放随从区域
    public List<MinionData> minionSlots;         
    // 合成区域 (最多支持放几格素材)
    public List<SingleBeiBao> craftingSlots;
    //已上阵角色
    public List<MinionData> deployedMinionSlots;

    public int maxMaterialSlots = 20;            // 素材区容量上限
    public int maxMinionSlots = 10;              // 随从区容量上限
    public int maxCraftingSlots = 3;             // 合成区大小
    public int maxDeployedSlots = 4;             //最大上阵角色数 (还有个战斗队伍上限)



    // 素材一键整理与排序
    public void SortMaterialSlots()
    {
        // 优先按星级排（越大越靠前），再按 ID 排（越小越靠前）
        materialSlots = materialSlots
            .Where(slot => slot.materialData != null && slot.count > 0)
            .OrderByDescending(slot => slot.materialData.Level)
            .ThenBy(slot => slot.materialData.materialID)
            .ToList();
    }

    //同星级 N合1 升星判定（用于同一素材升级） 暂时还没用
    public bool TryAutoUpgradeMaterial(SingleBeiBao targetSlot, MaterialData nextLevelMaterialConfig)
    {
        if (targetSlot.count >= synthesisN)
        {
            //能合成出多少高级的素材
            int upgradedCount = targetSlot.count / synthesisN;
            int remainCount = targetSlot.count % synthesisN;

            targetSlot.count = remainCount; // 消耗后剩下多少

            // 将 upgradedCount 个升级后的素材加入背包/合成区
            // AddMaterialToSlot(nextLevelMaterialConfig, upgradedCount);
            return true;
        }
        return false;
    }

    //随从合成检测与执行
    public bool TryCraftMinion(RecipeData recipe)
    {
        // 校验随从背包空间
        if (minionSlots.Count >= maxMinionSlots + 1) return false;

        // 校验合成区的素材是否满足 RecipeData 要求
        foreach (var req in recipe.requiredMaterials)
        {
            var matched = craftingSlots.Find(s => s.materialData == req.materialData);
            if (matched == null || matched.count < req.count)
            {
                print("素材不足");
                return false; // 素材不满足
            }
        }

        // 扣除合成区素材
        foreach (var req in recipe.requiredMaterials)
        {
            print("有对应配方");
            var matched = craftingSlots.Find(s => s.materialData == req.materialData);
            matched.count -= req.count;
        }

        //// 清理 count <= 0 的格子 引入了UI直接清楚会报错 需要去掉图标
        //craftingSlots.RemoveAll(s => s.count <= 0);
        for (int i = 0; i < craftingSlots.Count; i++)
        {
            if (craftingSlots[i].count <= 0)
            {
                craftingSlots[i].materialData = null;
                craftingSlots[i].count = 0;
            }
        }


        // 生成新随从放入随从背包
        // 循环找一个没有随从的地方放入新随从
        for (int i = 0; i < minionSlots.Count; i++)
        {
            //先判断 minionSlots[i] 是否为 null，避免 null 引用异常
            if (minionSlots[i] == null)
            {
                minionSlots[i] = recipe.resultMinion;
                return true;
            }
        }
        return true;
    }

    // 填满占位，使得 List[i] 不会报 OutOfRange 错误
    private void InitListCapacity()
    {
        while (materialSlots.Count < maxMaterialSlots) materialSlots.Add(new SingleBeiBao());
        while (craftingSlots.Count < maxCraftingSlots) craftingSlots.Add(new SingleBeiBao());
        while (minionSlots.Count < maxMinionSlots) minionSlots.Add(null);
        while (deployedMinionSlots.Count < maxDeployedSlots) deployedMinionSlots.Add(null);
    }

    // 操作1：将素材从素材区移动到合成区
    public void MoveMaterialToCrafting(int materialIndex)
    {
        //素材区域的一个素材提取出来
        var sourceSlot = materialSlots[materialIndex];
        if (sourceSlot == null || sourceSlot.materialData == null || sourceSlot.count <= 0) return;

        // 寻找合成区是否有相同素材可堆叠，或者空位
        for (int i = 0; i < craftingSlots.Count; i++)
        {
            if (craftingSlots[i].materialData == sourceSlot.materialData)
            {
                craftingSlots[i].count++;
                sourceSlot.count--;
                return;
            }
            else
            if (craftingSlots[i].materialData == null)
            {
                craftingSlots[i].materialData = sourceSlot.materialData;
                craftingSlots[i].count = 1;
                sourceSlot.count--;
                return;
            }
        }
        Debug.Log("合成区已满！");
    }

    // 操作2：将素材从合成区拿回素材区
    public void MoveCraftingToMaterial(int craftingIndex)
    {
        var craftSlot = craftingSlots[craftingIndex];
        if (craftSlot == null || craftSlot.materialData == null || craftSlot.count <= 0) return;

        // 归还到素材区
        for (int i = 0; i < materialSlots.Count; i++)
        {
            if (materialSlots[i].materialData == craftSlot.materialData)
            {
                materialSlots[i].count += craftSlot.count;
                craftSlot.materialData = null;
                craftSlot.count = 0;
                return;
            }
            else if (materialSlots[i].materialData == null)
            {
                materialSlots[i].materialData = craftSlot.materialData;
                materialSlots[i].count = craftSlot.count;
                craftSlot.materialData = null;
                craftSlot.count = 0;
                return;
            }
        }
    }

    // 核心：处理随从区 <-> 上阵区的拖拽与位置互换逻辑
    public void SwapOrMoveMinion(ItemSlotUI.SlotType srcType, int srcIndex, ItemSlotUI.SlotType dstType, int dstIndex)
    {
        // 只能在 随从区(MinionArea) 和 上阵区(DeployedArea) 之间操作
        if (!IsMinionSlot(srcType) || !IsMinionSlot(dstType)) return;

        // 获取源位置和目标位置的数据引用
        MinionData startMinion = GetMinionData(srcType, srcIndex);
        MinionData endMinion = GetMinionData(dstType, dstIndex);

        // 如果拖拽的是空格子，不做处理
        if (startMinion == null) return;
        
        // 互换数据 (不论目标位置是否有角色，直接互换引用)
        SetMinionData(srcType, srcIndex, endMinion); // 将目标位置的随从（可能为 null）放到源位置
        SetMinionData(dstType, dstIndex, startMinion); // 将源位置的随从放到目标位置
    }

    private bool IsMinionSlot(ItemSlotUI.SlotType type)
    {
        return type == ItemSlotUI.SlotType.MinionArea || type == ItemSlotUI.SlotType.DeployedArea;
    }

    private MinionData GetMinionData(ItemSlotUI.SlotType type, int index)
    {
        if (type == ItemSlotUI.SlotType.MinionArea) return minionSlots[index];
        if (type == ItemSlotUI.SlotType.DeployedArea) return deployedMinionSlots[index];
        return null;
    }
   
    // 设置随从数据到指定位置
    private void SetMinionData(ItemSlotUI.SlotType type, int index, MinionData data)
    {
        if (type == ItemSlotUI.SlotType.MinionArea)
        { 
        minionSlots[index] = data;
        
        } 
        else if (type == ItemSlotUI.SlotType.DeployedArea)
        {
            deployedMinionSlots[index] = data;
            BattleMgr.Instance.UpdateDeployedMinions(deployedMinionSlots[index] ,index); // 更新战斗管理器中的上阵随从列表
        }
    }
}
