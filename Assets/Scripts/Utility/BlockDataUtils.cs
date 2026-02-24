using UnityEngine;

public static class BlockDataUtils
{
    public static string NormalizeBlockDataName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "";

        return name.Replace("(Clone)", "").Trim();
    }

    public static BlockData FindBlockDataByName(string blockDataName)
    {
        return FindBlockDataByName(LevelManager.Instance, blockDataName);
    }

    public static BlockData FindBlockDataByName(LevelManager levelManager, string blockDataName)
    {
        if (levelManager == null || levelManager.levelDatas == null)
            return null;

        foreach (LevelData levelData in levelManager.levelDatas)
        {
            if (levelData == null || levelData.blocks == null)
                continue;

            foreach (BlockData blockData in levelData.blocks)
            {
                if (blockData != null && blockData.name == blockDataName)
                    return blockData;
            }
        }

        return null;
    }

    public static Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
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
}
