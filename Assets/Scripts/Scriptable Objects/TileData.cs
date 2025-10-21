using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public string tileName = "Tile";
    public bool isWalkable = true;
    public bool isMovable = false;
}
