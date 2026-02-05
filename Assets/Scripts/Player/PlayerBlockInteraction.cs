using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerBlockInteraction : MonoBehaviour
{
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }
    
    public bool TryHandleBlockInteraction(Block block, Vector2Int direction, Vector2Int newPosition)
    {
        if (block == null)
            return false;

        bool blockIsImmovable = block.runtimeData != null
            ? block.runtimeData.isImmovable
            : (block.data != null && block.data.isImmovable);

        if (blockIsImmovable)
            return false;

        if (block._isInHole)
            return true; 

        Vector2Int blockTargetPosition = newPosition + direction;
        bool targetIsHole = blockTargetPosition == LevelManager.Instance.GetCurrentLevelData().holePosition;
        bool targetOutOfBounds = LevelManager.Instance.CheckOutOfBounds(blockTargetPosition);

        if (targetIsHole && block.canBePlacedInHole)
        {
            PushBlockIntoHole(block, holePos: blockTargetPosition, currentPos: newPosition);
            return true;
        }

        if (targetOutOfBounds || targetIsHole)
            return false;

        GroundTile targetTile = LevelManager.Instance.GetTileAt(blockTargetPosition);
        if (targetTile == null)
            return false;

        if (targetTile.isOccupied && targetTile.occupant != null)
        {
            Block targetBlock = targetTile.occupant;

            bool targetIsImmovable = targetBlock.runtimeData != null
                ? targetBlock.runtimeData.isImmovable
                : (targetBlock.data != null && targetBlock.data.isImmovable);

            if (targetIsImmovable)
                return false;

            if (block.CanCombineWith(targetBlock))
            {
                GroundTile currentTile = LevelManager.Instance.GetTileAt(newPosition);
                if (currentTile != null)
                {
                    currentTile.ClearOccupant();
                }

                block.levelIndex = LevelManager.Instance.currentLevelIndex;
                block.PushTo(blockTargetPosition);

                StartCoroutine(CombineBlocksAfterMove(block, targetBlock, blockTargetPosition, direction));
                return true;
            }

            return false;
        }

        if (!targetTile.isOccupied)
        {
            PushBlock(block, targetPos: blockTargetPosition, currentPos: newPosition);
            StartCoroutine(ApplyArrowAfterMove(block, blockTargetPosition, direction));
            return true;
        }

        return false;
    }

    private void PushBlock(Block block, Vector2Int targetPos, Vector2Int currentPos)
    {
        GroundTile fromTile = LevelManager.Instance.GetTileAt(currentPos);
        GroundTile toTile = LevelManager.Instance.GetTileAt(targetPos);

        if (fromTile != null)
            fromTile.ClearOccupant();

        if (toTile != null)
            toTile.SetOccupant(block);

        block.levelIndex = LevelManager.Instance.currentLevelIndex;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBlockPushSound();

        block.PushTo(targetPos);
    }

    private void PushBlockIntoHole(Block block, Vector2Int holePos, Vector2Int currentPos)
    {
        GroundTile oldTile = LevelManager.Instance.GetTileAt(currentPos);
        if (oldTile != null)
            oldTile.ClearOccupant();

        block.levelIndex = LevelManager.Instance.currentLevelIndex;

        block.PushToThenPlaceInHole(holePos);
        LevelManager.Instance.RegisterElevator(holePos, block);

        Debug.Log("Block placed in hole, it now acts as an elevator.");
    }

    private IEnumerator CombineBlocksAfterMove(Block movingBlock, Block targetBlock, Vector2Int position, Vector2Int pushDirection)
    {
        while (movingBlock != null && movingBlock.isMoving)
            yield return null;

        yield return new WaitForSeconds(0.1f);

        GroundTile tile = LevelManager.Instance.GetTileAt(position);
        if (tile != null)
            tile.ClearOccupant();

        if (targetBlock != null)
        {
            LevelManager.Instance.loader.UnregisterBlockInstance(targetBlock.levelIndex, targetBlock.gameObject);
        }

        if (movingBlock != null)
            movingBlock.CombineWith(targetBlock);

        if (tile != null && movingBlock != null)
            tile.SetOccupant(movingBlock);

        ApplyArrowAtPosition(movingBlock, position, pushDirection);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }

    private IEnumerator ApplyArrowAfterMove(Block block, Vector2Int targetPos, Vector2Int pushDirection)
    {
        if (block == null)
            yield break;

        while (block.isMoving)
            yield return null;

        ApplyArrowAtPosition(block, targetPos, pushDirection);
    }

    private void ApplyArrowAtPosition(Block block, Vector2Int position, Vector2Int pushDirection)
    {
        if (block == null)
            return;

        GroundTile tile = LevelManager.Instance.GetTileAt(position);
        if (tile == null)
            return;

        ArrowTile arrow = tile.GetComponent<ArrowTile>();
        if (arrow == null)
            return;

        arrow.ApplyToBlock(block, pushDirection);
    }
}