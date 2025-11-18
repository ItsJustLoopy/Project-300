using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BlockData", menuName = "Scriptable Objects/BlockData")]
public class BlockData : ScriptableObject
{
    public string blockName = "Block";
    public GameObject blockPrefab;
    public Vector3 BlockPosition;

    public enum BlockColor
    {
        Red,
        Yellow,
        Blue,
        Purple,
        Orange,
        Green,
        Black
    }

    public BlockColor blockColor = BlockColor.Red;
    public List<BlockColor> containedColors = new List<BlockColor>();
}
