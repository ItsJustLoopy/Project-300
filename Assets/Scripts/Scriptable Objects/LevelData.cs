using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Level Settings")]
    public int levelNumber;
    public int levelWidth;
    public int levelHeight;
    public Vector2Int holePosition = new Vector2Int(4, 4);
    public TileData[] tiles;
}
