using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private BlockData heldAsset;
    [NonSerialized] private BlockData heldRuntime;

    public event Action OnChanged;

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
        if (heldAsset == null)
        {
            string normalized = BlockDataUtils.NormalizeBlockDataName(source.name);
            heldAsset = BlockDataUtils.FindBlockDataByName(normalized);
        }

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
        if (asset == null && heldRuntime != null)
        {
            string normalized = BlockDataUtils.NormalizeBlockDataName(heldRuntime.name);
            asset = BlockDataUtils.FindBlockDataByName(normalized);
            heldAsset = asset;
        }

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

        var color = BlockDataUtils.GetColorFromBlockColor(src.blockColor);
        return color;
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