using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Scriptable Objects/BlockData")]
public class BlockData : ScriptableObject
{
    public string blockName = "Block";
    public GameObject blockPrefab;
    public Vector3 BlockPosition;

    public enum BlockType
    {
        Red,
        Blue,
        Yellow
    }

    
}
