using UnityEngine;

public class ImmovableBlock : Block
{
    protected override void Awake()
    {
        base.Awake();
        canBePlacedInHole = false;
    }

    protected override void Start()
    {
        base.Start();
    }    

    public new void PushTo(Vector2Int targetPos)
    {
    }

    public new void PushToThenPlaceInHole(Vector2Int targetPos)
    {
    }

    public new bool CanCombineWith(Block otherBlock)
    {
        return false;
    }

    public new void CombineWith(Block otherBlock)
    {
    }
}

