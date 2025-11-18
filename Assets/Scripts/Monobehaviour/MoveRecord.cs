using UnityEngine;

public struct MoveRecord
{
    public Vector2Int playerFrom;
    public Vector2Int playerTo;

    public bool pushedBlock;
    public Block block;
    public Vector2Int blockFrom;
    public Vector2Int blockTo;

    // Player move
    public MoveRecord(Vector2Int pFrom, Vector2Int pTo)
    {
        playerFrom = pFrom;
        playerTo = pTo;

        pushedBlock = false;
        block = null;
        blockFrom = Vector2Int.zero;
        blockTo = Vector2Int.zero;
    }

    // player move with block push
    public MoveRecord(Vector2Int pFrom, Vector2Int pTo, Block block, Vector2Int bFrom, Vector2Int bTo)
    {
        playerFrom = pFrom;
        playerTo = pTo;

        pushedBlock = true;
        this.block = block;
        blockFrom = bFrom;
        blockTo = bTo;
    }
}   

