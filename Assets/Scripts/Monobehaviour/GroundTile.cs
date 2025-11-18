using UnityEngine;

public class GroundTile : MonoBehaviour
{
    public TileData data;
    public bool isWalkable = true;
    public Vector2Int gridPosition;
    public Block occupant;  
    public bool isOccupied = false;

    public void Initialize(TileData tileData, Vector2Int gridPos)
    {
        data = tileData;
        gridPosition = gridPos;
        isOccupied = false;
        name = $"Tile_({gridPosition.x},{gridPosition.y})";
        
    }
    
    
}
