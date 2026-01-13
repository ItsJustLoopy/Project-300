using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Block heldBlock;
    //public BlockData BlockData;

    public bool IsEmpty()
    {
        return heldBlock == null;
    }

    public void Store(Block block)
    {
        heldBlock = block;
        block.gameObject.SetActive(false);
    }

    public Block Take()
    {
        Block block = heldBlock;
        heldBlock = null;
        return block;
    }

}