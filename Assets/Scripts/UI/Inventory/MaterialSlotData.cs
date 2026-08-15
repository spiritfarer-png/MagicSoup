using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialSlotData
{
    public bool isRelic => materialData is IRelic;
    public SoupMaterialData materialData;
    public bool IsOccupied => materialData != null;
}
