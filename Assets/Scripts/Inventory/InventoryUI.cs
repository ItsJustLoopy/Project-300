using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventory;
    public Image blockIcon;

    void Update()
    {
        if (inventory.IsEmpty())
        {
            blockIcon.enabled = false;
        }
        else
        {
            blockIcon.enabled = true;
            blockIcon.color = GetColorFromBlockColor(inventory.heldItem.blockColor);
        }
    }
    private Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
    {
        switch (blockColor)
        {
            case BlockData.BlockColor.Red: return Color.red;
            case BlockData.BlockColor.Yellow: return Color.yellow;
            case BlockData.BlockColor.Blue: return Color.blue;
            case BlockData.BlockColor.Purple: return Color.blueViolet;
            case BlockData.BlockColor.Orange: return Color.orange;
            case BlockData.BlockColor.Green: return Color.green;
            case BlockData.BlockColor.Black: return Color.black;
            default: return Color.white;
        }
    }
}