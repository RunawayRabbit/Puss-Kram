using UnityEngine;

[RequireComponent(typeof(SnowballMover))]
[SelectionBase]
class Snowball : PushableObject
{
    public readonly float[] SnowballScales = { 1.0f, 1.2f, 1.4f, 1.8f, 2.0f, 2.2f };
    public int size { get; private set; } = 0;
    public float baseScale { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        baseScale = transform.localScale.x;
    }

    public float GetSizeInMeters()
    {
        return SnowballScales[size];
    }

    protected override bool TryInitiateMoveTo(GridNode targetGridNode)
    {
        if (base.TryInitiateMoveTo(targetGridNode))
        {
            if (targetGridNode.hasSnow)
            {
                var snowballMover = mover as SnowballMover;
                if (snowballMover == null) Debug.LogError("This Snowball has a mover that isn't a SnowballMover!", this);
                bool pickedUpSnow = TryIncrementSize();
                if (pickedUpSnow) targetGridNode.TryDepleteSnow();
            }
            return true;
        }
        else
            return false;
    }

    public bool TryIncrementSize()
    {
        if (size >= SnowballScales.Length - 1)
        {
            return false;
        }
        else
        {
            size++;
            return true;
        }
    }

    protected override bool SnapToGrid()
    {
        bool ret = base.SnapToGrid();
        if(ret)
        {
            currentGridNode.TryDepleteSnow();
        }

        return ret;
    }
}
