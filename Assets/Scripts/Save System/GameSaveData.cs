using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int currentLevelIndex;
    public Vector2IntSerializable playerPosition;
    public List<BlockSaveData> allBlocks;
    public List<ElevatorSaveData> elevators;
    public List<int> loadedLevelIndices;
    public string saveTimestamp;

    public GameSaveData()
    {
        allBlocks = new List<BlockSaveData>();
        elevators = new List<ElevatorSaveData>();
        loadedLevelIndices = new List<int>();
        saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

[System.Serializable]
public class BlockSaveData
{
    public Vector2IntSerializable gridPosition;
    public BlockData.BlockColor blockColor;
    public List<BlockData.BlockColor> containedColors;
    public int levelIndex;
    public int originLevelIndex;
    public bool isAtOriginLevel;
    public bool isInHole;
    
    public string blockDataName;
}

[System.Serializable]
public class ElevatorSaveData
{
    public Vector2IntSerializable position;
    public int originLevelIndex;
    public bool isAtOriginLevel;
}


[System.Serializable]
public struct Vector2IntSerializable
{
    public int x;
    public int y;

    public Vector2IntSerializable(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2IntSerializable(Vector2Int vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x, y);
    }

    public static implicit operator Vector2Int(Vector2IntSerializable v) => v.ToVector2Int();
    public static implicit operator Vector2IntSerializable(Vector2Int v) => new Vector2IntSerializable(v);

    public override bool Equals(object obj)
    {
        if (obj is Vector2IntSerializable other)
        {
            return x == other.x && y == other.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (x, y).GetHashCode();
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}