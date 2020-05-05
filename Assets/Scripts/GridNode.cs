
using System;
using UnityEngine;

public class GridNode : MonoBehaviour
{
    public GridNode BackNeighbour { get; private set; }
    public GridNode FrontNeighbour { get; private set; }
    public GridNode LeftNeighbour { get; private set; }
    public GridNode RightNeighbour { get; private set; }
    public GridNode DownNeighbour { get; set; }

    public PushableObject occupant = default;

    public bool hasSnow { get; private set; } = true;
    public bool isOutOfBounds;
    public int outOfBoundsDepth;

    private MeshRenderer mRenderer;
    private MeshFilter mFilter;

    public bool TryDepleteSnow()
    {
        if (hasSnow)
        {
            hasSnow = false;
            RecalculateSnowOnAll();
            return true;
        }

        return false;
    }

    private void RecalculateSnowOnAll()
    {
        RecalculateSnow();
        if(LeftNeighbour != null) LeftNeighbour.RecalculateSnow();
        if (BackNeighbour != null)
        {
            BackNeighbour.RecalculateSnow();
            if(BackNeighbour.LeftNeighbour != null)
                BackNeighbour.LeftNeighbour.RecalculateSnow();
        }
    }

    public void RecalculateSnow()
    {
        int snowConfig = GetSnowConfiguration();

        mFilter.sharedMesh = LevelGrid.Instance.snowMeshes[snowConfig];        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = hasSnow ? Color.red : Color.white;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    private void Awake()
    {
        hasSnow = true;
        mRenderer = gameObject.AddComponent<MeshRenderer>();
        mFilter = gameObject.AddComponent<MeshFilter>();
    }

    private void Start()
    {
        RecalculateSnow();

        mRenderer.sharedMaterial = LevelGrid.Instance.snowMaterial;
        mRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }


    static public void SetNeighbourBackFront(GridNode back, GridNode front)
    {
        back.FrontNeighbour = front;
        front.BackNeighbour = back;
    }

    static public void SetNeighbourLeftRight(GridNode left, GridNode right)
    {
        left.RightNeighbour = right;
        right.LeftNeighbour = left;
    }

    static bool NeighbourHasSnow(GridNode neighbour)
    {
        if (neighbour == null) return true;
        return neighbour.hasSnow;
    }

    byte GetSnowConfiguration()
    {
        GridNode topRight = null;
        if (RightNeighbour != null)
        {
            topRight = RightNeighbour.FrontNeighbour;
        }
        else if(FrontNeighbour != null)
        {
            topRight = FrontNeighbour.RightNeighbour;
        }

        GridNode topLeft = FrontNeighbour;
        GridNode bottomRight = RightNeighbour;
        GridNode bottomLeft = this;

        byte retval = 0; ;
        retval += NeighbourHasSnow(topLeft)     ? (byte)8 : (byte)0;
        retval += NeighbourHasSnow(topRight)    ? (byte)4 : (byte)0;
        retval += NeighbourHasSnow(bottomRight) ? (byte)2 : (byte)0;
        retval += NeighbourHasSnow(bottomLeft)  ? (byte)1 : (byte)0;

        return retval;
    }
}
