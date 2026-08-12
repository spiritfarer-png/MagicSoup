using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "ScriptableObject/合成角色数据", order = 0)]
public class RecipeData : ScriptableObject
{
     public string recipeId;
     public List<SingleBeiBao> requiredMaterials; // 所需素材及数量
     public MinionData resultMinion;           // 合成出的随从/角色配置 
}
