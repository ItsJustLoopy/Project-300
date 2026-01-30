using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class ArrowData
    {
        public Vector2Int position;
        public ArrowDirection direction = ArrowDirection.Up;
        public BlockData.BlockColor color = BlockData.BlockColor.Red;
    }

    [Header("Level Settings")]
    public int levelWidth;
    public int levelHeight;
    public Vector2Int playerSpawn;
    public Vector2Int holePosition = new Vector2Int(4, 4);
    public List<BlockData> blocks;
    public List<ArrowData> arrows = new List<ArrowData>();
    
}
