using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialData", menuName = "ScriptableObject/素材数据", order = 0)]
public class MaterialData : ScriptableObject
{
    //素材唯一ID
    public string materialID; 
    //素材名称
    public string materialName;
    //素材星级
    public int Level;
    //图标
    public Sprite icon;
}
