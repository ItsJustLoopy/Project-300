using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveRecord
{
    public PlayerRecord player;
    public List<BlockRecord> blocks;
    public int currentLevelIndex;
}

[Serializable]
public class PlayerRecord
{
    public Vector2Int gridPos;
}

[Serializable]
public class BlockRecord
{
    public Vector2Int gridPos;
    public int levelIndex;
    public bool isInHole;
    public BlockData.BlockColor color;
    public List<BlockData.BlockColor> containedColors;
}