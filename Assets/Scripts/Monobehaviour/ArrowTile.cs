using UnityEngine;

public class ArrowTile : MonoBehaviour
{
    public ArrowDirection direction = ArrowDirection.Up;
    public BlockData.BlockColor arrowColor = BlockData.BlockColor.Red;
    public Renderer arrowRenderer;
    public float arrowYOffset = 0.02f;
    private GameObject _visualInstance;

    public Vector2Int DirectionVector
    {
        get
        {
            switch (direction)
            {
                case ArrowDirection.Up: return new Vector2Int(0, 1);
                case ArrowDirection.Right: return new Vector2Int(1, 0);
                case ArrowDirection.Down: return new Vector2Int(0, -1);
                case ArrowDirection.Left: return new Vector2Int(-1, 0);
                default: return Vector2Int.zero;
            }
        }
    }
    
    public void ApplyToBlock(Block block, Vector2Int pushDirection)
    {
        if (block == null)
        {
            return;
        }
        if (!block.IsPrimaryColor(arrowColor))
        {
            return;
        }

        Vector2Int arrowDir = DirectionVector;
        if (pushDirection == arrowDir)
        {
            block.AddPrimaryColor(arrowColor);
        }
        else if (pushDirection == -arrowDir)
        {
            block.RemovePrimaryColor(arrowColor);
        }
    }

    public void SyncVisuals()
    {
        if (arrowRenderer == null)
        {
            return;
        }
        arrowRenderer.material.color = BlockDataUtils.GetColorFromBlockColor(arrowColor);
    }

    public void Initialize(ArrowDirection newDirection, BlockData.BlockColor newColor, GameObject arrowPrefab, float yOffset)
    {
        direction = newDirection;
        arrowColor = newColor;
        arrowYOffset = yOffset;
        SpawnVisual(arrowPrefab);
        SyncVisuals();
    }

    private void OnValidate()
    {
        SyncVisuals();
    }

    private void SpawnVisual(GameObject arrowPrefab)
    {
        if (arrowPrefab == null)
        {
            return;
        }
        if (_visualInstance != null)
        {
            DestroyImmediate(_visualInstance);
        }
        _visualInstance = Instantiate(arrowPrefab, transform);
        _visualInstance.transform.localPosition = new Vector3(0f, arrowYOffset, 0f);
        _visualInstance.transform.localRotation = Quaternion.Euler(90f, GetYawForDirection(direction), 0f);
        arrowRenderer = _visualInstance.GetComponentInChildren<Renderer>();
    }

    private float GetYawForDirection(ArrowDirection arrowDirection)
    {
        switch (arrowDirection)
        {
            case ArrowDirection.Up: return 0f;
            case ArrowDirection.Right: return 90f;
            case ArrowDirection.Down: return 180f;
            case ArrowDirection.Left: return 270f;
            default: return 0f;
        }
    }

}
