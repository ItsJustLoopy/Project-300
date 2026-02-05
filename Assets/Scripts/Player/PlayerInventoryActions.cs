using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(InventoryManager))]
public class PlayerInventoryActions : MonoBehaviour
{
    private Player _player;
    private InventoryManager _inventory;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _inventory = GetComponent<InventoryManager>();
    }

    public void TryPickup()
    {
        if (_inventory.IsEmpty() == false)
            return;

        Vector2Int targetPos = _player.gridPosition + _player.facingDirection;
        GroundTile tile = LevelManager.Instance.GetTileAt(targetPos);
        if (tile == null)
            return;

        Block block = tile.occupant;
        if (block == null || !block.CanBePickedUp)
            return;
        
        tile.ClearOccupant();

        LevelManager.Instance.loader.UnregisterBlockInstance(block.levelIndex, block.gameObject);

        _inventory.Store(block);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }

    public void TryPlace()
    {
        if (_inventory.IsEmpty())
            return;

        Vector2Int targetPos = _player.gridPosition + _player.facingDirection;
        if (!LevelManager.Instance.CanPlaceBlockAt(targetPos))
            return;

        if (!_inventory.TryTake(out var asset, out var runtimeSnapshot))
            return;

        float yOffset = LevelManager.Instance.currentLevelIndex * LevelManager.Instance.verticalSpacing;
        Vector3 spawnPos = new Vector3(targetPos.x, yOffset + 1f, targetPos.y);

        GameObject blockObj = Instantiate(LevelManager.Instance.blockPrefab, spawnPos, Quaternion.identity);
        Block placedBlock = blockObj.GetComponent<Block>();
        if (placedBlock == null)
        {
            Destroy(blockObj);
            return;
        }

        placedBlock.data = asset;
        placedBlock.runtimeData = runtimeSnapshot != null ? runtimeSnapshot : (asset != null ? Instantiate(asset) : null);
        placedBlock.levelIndex = LevelManager.Instance.currentLevelIndex;
        placedBlock.gridPosition = targetPos;

        if (placedBlock.runtimeData != null)
        {
            placedBlock.runtimeData.BlockPosition = new Vector3(targetPos.x, spawnPos.y, targetPos.y);
        }

        placedBlock.ApplyRuntimeData();

        GroundTile tile = LevelManager.Instance.GetTileAt(targetPos);
        if (tile != null)
        {
            tile.SetOccupant(placedBlock);
        }

        LevelManager.Instance.loader.RegisterBlockInstance(placedBlock.levelIndex, placedBlock.gameObject);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }
}