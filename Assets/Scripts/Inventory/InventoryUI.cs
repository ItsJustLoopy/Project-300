using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventory;
    public Image blockIcon;


    private void Awake()
    {
        //Find runtime InventoryManager (player is spawned at runtime)
        inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("InventoryUI: No InventoryManager found in scene at runtime.");
        }

        if (blockIcon == null)
        {
            Debug.LogError("InventoryUI: blockIcon is not assigned.");
        }

        //makes sure the players inventory component is found when loaded at runtime
        Debug.Log($"InventoryUI: inventory found = {(inventory != null ? inventory.name : "NULL")}");

    }
    public void Update()
    {

        if (inventory == null || blockIcon == null) return;

        if (inventory.IsEmpty())
        {
            blockIcon.enabled = false;
        }
        else
        {
            blockIcon.enabled = true;
            //blockIcon.color = inventory.heldBlock.GetColorFromBlockColor(inventory.heldBlock.data.blockColor);
            blockIcon.color = inventory.heldBlock.GetColorFromBlockColor(inventory.heldBlock.currentColor);

        }
    }
    //private Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
    //{
    //    switch (blockColor)
    //    {
    //        case BlockData.BlockColor.Red: return Color.red;
    //        case BlockData.BlockColor.Yellow: return Color.yellow;
    //        case BlockData.BlockColor.Blue: return Color.blue;
    //        case BlockData.BlockColor.Purple: return Color.blueViolet;
    //        case BlockData.BlockColor.Orange: return Color.orange;
    //        case BlockData.BlockColor.Green: return Color.green;
    //        case BlockData.BlockColor.Black: return Color.black;
    //        default: return Color.white;
    //    }
    //}
}