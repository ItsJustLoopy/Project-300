using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private BlockData heldAsset;
    [NonSerialized] private BlockData heldRuntime;

    public event Action OnChanged;
    //fhohfouweheofhworhjifohioihf
    public bool IsEmpty()
    {
        return heldRuntime == null && heldAsset == null;
    }

    public bool HasItem => !IsEmpty();

    public BlockData HeldAsset => heldAsset;
    public BlockData HeldRuntime => heldRuntime;

    public void Store(Block block)
    {
        if (block == null) return;


        if (block.data == null)
        {
            Debug.LogWarning("InventoryManager Tried to store a block with no BlockData asset reference (block.data == null). Inventory will not be saveable.", block);
        }

        BlockData source = block.runtimeData != null ? block.runtimeData : block.data;
        if (source == null)
        {
            Debug.LogWarning("InventoryManager Tried to store a block with no data (both runtimeData and data are null).", block);
            return;
        }

        heldAsset = block.data;             
        heldRuntime = Instantiate(source);


        OnChanged?.Invoke();
        Destroy(block.gameObject);

    }

    public bool TryTake(out BlockData asset, out BlockData runtimeSnapshot)
    {
        if (IsEmpty())
        {
            asset = null;
            runtimeSnapshot = null;
            return false;
        }

        asset = heldAsset;
        runtimeSnapshot = heldRuntime != null ? Instantiate(heldRuntime) : (heldAsset != null ? Instantiate(heldAsset) : null);

        heldAsset = null;
        heldRuntime = null;

        OnChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        heldAsset = null;
        heldRuntime = null;
        OnChanged?.Invoke();
    }

    public Color TryGetHeldColor()
    {
        var defaultcolor = Color.white;
       
        if (heldRuntime == null && heldAsset == null) return defaultcolor;

        var src = heldRuntime != null ? heldRuntime : heldAsset;
        if (src == null) return defaultcolor;

        var color = GetColorFromBlockColor(src.blockColor);
        return color;
    }

    private static Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
    {
        switch (blockColor)
        {
            case BlockData.BlockColor.White: return Color.white;
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

    public void SetHeldFromSave(BlockData asset, BlockData runtimeSnapshot)
    {
        heldAsset = asset;

        if (runtimeSnapshot != null)
        {
            heldRuntime = Instantiate(runtimeSnapshot);
        }
        else if (asset != null)
        {
            heldRuntime = Instantiate(asset);
        }
        else
        {
            heldRuntime = null;
        }

        OnChanged?.Invoke();
    }
}