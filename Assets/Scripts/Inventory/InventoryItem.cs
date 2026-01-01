using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryItem 
    //This class essentially translates a conceptual block into a data object so references to scene objects arent manipulated
{
    public string blockDataName;
    public BlockData.BlockColor blockColor;
    public List<BlockData.BlockColor> containedColors;
}