using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    //public Block HeldBlock;
    //public BlockData BlockData;

    public InventoryItem heldItem;

    public bool IsEmpty()
    {
        return heldItem == null;
    }

    public void Store(Block block)
    {
        heldItem = new InventoryItem
        {
            blockDataName = block.data.name,
            blockColor = block.data.blockColor,
            containedColors = new List<BlockData.BlockColor>(block.containedPrimaryColors)
        };
    }

    public InventoryItem Take()
    {
        InventoryItem item = heldItem;
        heldItem = null;
        return item;
    }
}