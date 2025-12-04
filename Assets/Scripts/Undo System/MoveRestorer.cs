using System.Collections.Generic;
using UnityEngine;

public static class MoveRestorer
{
    // -------------------------
    // CREATE RECORD
    // -------------------------

    public static MoveRecord CreateMoveRecord()
    {
        MoveRecord record = new MoveRecord();

        LevelManager lm = LevelManager.Instance;

        // Player
        record.player = new PlayerRecord
        {
            //gridPos = lm._playerScript.gridPosition
        };

        // Blocks
        record.blocks = new List<BlockRecord>();
        foreach (var block in Object.FindObjectsByType<Block>(FindObjectsSortMode.None))
        {
            record.blocks.Add(new BlockRecord
            {
                gridPos = block.gridPosition,
                levelIndex = block.levelIndex,
                isInHole = block.IsInHole(),
                color = block.data.blockColor,
                containedColors = new List<BlockData.BlockColor>(block.containedPrimaryColors)
            });
        }

        record.currentLevelIndex = lm.currentLevelIndex;

        return record;
    }

    // -------------------------
    // RESTORE RECORD
    // -------------------------

    public static void RestoreMoveRecord(MoveRecord record)
    {
        LevelManager lm = LevelManager.Instance;

        // Destroy all existing blocks
        foreach (var block in Object.FindObjectsByType<Block>(FindObjectsSortMode.None))
            Object.Destroy(block.gameObject);

        // Re-spawn blocks
        foreach (var b in record.blocks)
        {
            Vector3 pos = new Vector3(
                b.gridPos.x,
                b.levelIndex * lm.verticalSpacing + 1f,
                b.gridPos.y
            );

            GameObject obj = Object.Instantiate(lm.blockPrefab, pos, Quaternion.identity);
            Block block = obj.GetComponent<Block>();

            block.data = new BlockData
            {
                blockColor = b.color,
                containedColors = new List<BlockData.BlockColor>(b.containedColors)
            };

            block.levelIndex = b.levelIndex;

            if (b.isInHole)
                block.PushToThenPlaceInHole(b.gridPos);
        }

        // Restore player
        lm.currentLevelIndex = record.currentLevelIndex;
        //lm._playerScript.gridPosition = record.player.gridPos;

        lm._playerInstance.transform.position = new Vector3(
            record.player.gridPos.x,
            record.currentLevelIndex * lm.verticalSpacing + 1f,
            record.player.gridPos.y
        );

        // Update tile grid
        lm.UpdateGroundTilesForCurrentLevel();
    }
}