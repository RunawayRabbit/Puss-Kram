using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelGrid : Singleton<LevelGrid>
{
    protected LevelGrid() { }

    private Scene gridScene;
    
    [SerializeField, Range(0.1f, 0.5f)] public float snowDepth = 0.1f;
    [SerializeField] public Material snowMaterial = default;

    [SerializeField] float maxDropHeight = 4.0f;
    [SerializeField, Range(0.0f, 0.5f)] float maxSlope = 0.2f;

    [SerializeField] LayerMask ignoredLayers = default;
    [SerializeField] public LayerMask gridLayer = default; // used by PushableObject
    private LayerMask nonIgnoredLayers;

    [SerializeField] Vector3 gridSize = Vector3.one;
    public Vector3 halfGridSize { get; private set; }

    [HideInInspector] public Vector2Int gridDim;
    private Vector3 minExtents;
    private Vector3 maxExtents;
    
    [HideInInspector] public Mesh[] snowMeshes;
    [SerializeField] private SnowTile emptySnowTile = default; // 0
    [SerializeField] private SnowTile cornerSnowTile = default; // 1, 2, 4, 8
    [SerializeField] private SnowTile halfSnowTile = default; // 3, 6, 9, 12
    [SerializeField] private SnowTile threeCornerSnowTile = default; // 7, 11, 13, 14,
    [SerializeField] private SnowTile oppositeCornerSnowTile = default; // 5, 10
    [SerializeField] private SnowTile fullSnowTile = default; // 15

    private void GenerateTiles()
    {
        snowMeshes[0]  = GenerateTile(emptySnowTile, 0);
        snowMeshes[15] = GenerateTile(fullSnowTile, 15);

        //@NOTE: Cycling these rather than doing data entry again.
        SnowTile config1 = cornerSnowTile;
        SnowTile config2 = config1.Rotated();
        SnowTile config4 = config2.Rotated();
        SnowTile config8 = config4.Rotated();
        snowMeshes[1] = GenerateTile(config1, 1);
        snowMeshes[2] = GenerateTile(config2, 2);
        snowMeshes[4] = GenerateTile(config4, 4);
        snowMeshes[8] = GenerateTile(config8, 8);

        snowMeshes[14] = GenerateTile(threeCornerSnowTile, 14);
        SnowTile config13 = threeCornerSnowTile.Rotated();
        snowMeshes[13] = GenerateTile(config13, 13);
        SnowTile config11 = config13.Rotated();
        snowMeshes[11] = GenerateTile(config11, 11);
        SnowTile config7 = config11.Rotated();
        snowMeshes[7]  = GenerateTile(config7, 7);

        snowMeshes[3] = GenerateTile(halfSnowTile, 3);
        SnowTile config6 = halfSnowTile.Rotated();
        snowMeshes[6] = GenerateTile(config6, 6);
        SnowTile config12 = config6.Rotated();
        snowMeshes[12] = GenerateTile(config12, 12);
        SnowTile config9 = config12.Rotated();
        snowMeshes[9] = GenerateTile(config9, 9);

        snowMeshes[10] = GenerateTile(oppositeCornerSnowTile, 10);
        SnowTile config5 = oppositeCornerSnowTile.Rotated();
        snowMeshes[5] = GenerateTile(config5, 5);

    }

    private Mesh GenerateTile(SnowTile snowTile, int config)
    {
        var mesh = new Mesh();
        mesh.name = "SnowTile " + config;

        int quadsPerSide = SnowTile.Width - 1;

        var vertices = new Vector3[(quadsPerSide + 1) * (quadsPerSide + 1)];
        var triangles = new int[quadsPerSide * quadsPerSide * 6];

        float step = 1.0f / (quadsPerSide);

        for (int z = 0, i = 0; z <= quadsPerSide; z++)
        {
            for (int x = 0; x <= quadsPerSide; x++, i++)
            {
                vertices[i] = new Vector3(
                    gridSize.x * x * step,
                    snowTile[i] * snowDepth,
                    gridSize.z * z * step);
            }
        }

        for (int z = 0, vert = 0, tris = 0;
            z < quadsPerSide;
            z++, vert++)
        {
            for (int x = 0;
                x < quadsPerSide;
                x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + quadsPerSide + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + quadsPerSide + 1;
                triangles[tris + 5] = vert + quadsPerSide + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }


    private void OnValidate()
    {
        if (gridSize.x < 1.0f) gridSize.x = 1.0f;
        if (gridSize.y < 1.0f) gridSize.y = 1.0f;
        if (gridSize.z < 1.0f) gridSize.z = 1.0f;

        halfGridSize = gridSize * 0.5f;

        nonIgnoredLayers = (Physics.AllLayers ^ ignoredLayers);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 horizontalSpan = Vector3.left * gridSize.x * (gridDim.x);
        Vector3 verticalSpan = Vector3.back * gridSize.z * (gridDim.y);

        Vector3 flattenedHalfGridSize = new Vector3(halfGridSize.x, 0.0f, halfGridSize.z);
        
        DrawGridLines(transform.position - minExtents + flattenedHalfGridSize,
            horizontalSpan, gridDim.x, gridSize.z * Vector3.back);
        DrawGridLines(transform.position - minExtents + flattenedHalfGridSize,
            verticalSpan, gridDim.y, gridSize.x * Vector3.left);

    }

    private void DrawGridLines(Vector3 start, Vector3 vector, int count, Vector3 step)
    {
        Vector3 end = start + vector;
        for (int i = 0; i < count; i++)
        {
            Gizmos.DrawLine(
                start + step*i,
                end + step*i);
        }
    }


    private void Awake()
    {
        snowMeshes = new Mesh[16];
        GenerateTiles();

        gridScene = SceneManager.CreateScene("Grid Scene");

        gridDim = Vector2Int.one;
        // Extents are relative to transform.position
        maxExtents = minExtents = Vector3.zero;

        float offsetToGround = GetDistanceToGround(transform.position);
        if (offsetToGround > gridSize.y * maxSlope)
            Debug.Log("Looks like the Grid Root is too far from the ground!");
        else
            FloodFill(transform.position + (Vector3.down * offsetToGround));
    }

    private GridNode PlaceTileAt(Vector3 location, string name = "GridNode")
    {
        float minX = minExtents.x;
        float maxX = maxExtents.x;
        float minY = minExtents.z;
        float maxY = maxExtents.z;

        Vector3 gridSpaceLocation = transform.position - location;

        if (gridSpaceLocation.x < minExtents.x - halfGridSize.x)
        {
            minExtents.x = gridSpaceLocation.x;
            gridDim.x++;
        }
        if (gridSpaceLocation.x > maxExtents.x + halfGridSize.x)
        {
            maxExtents.x = gridSpaceLocation.x;
            gridDim.x++;
        }
        if (gridSpaceLocation.z < minExtents.z - halfGridSize.z)
        {
            minExtents.z = gridSpaceLocation.z;
            gridDim.y++;
        }
        if (gridSpaceLocation.z > maxExtents.z + halfGridSize.z)
        {
            maxExtents.z = gridSpaceLocation.z;
            gridDim.y++;
        }


        var newNode = new GameObject(name + location.ToString());
        int gridLayerIndex = Utils.GetLayerNumberFromMask(gridLayer);
        Debug.Assert(gridLayerIndex != -1, "LevelGrid does not have it's layer set ");
        newNode.layer = gridLayerIndex;

        newNode.transform.position = location;

        var gridNode = newNode.AddComponent<GridNode>();
        var col = newNode.AddComponent<BoxCollider>();

        col.size = gridSize;
        col.center = new Vector3(col.center.x, halfGridSize.y, col.center.z);
        col.isTrigger = true;

        SceneManager.MoveGameObjectToScene(newNode, gridScene);

        //Utils.DrawDebugBox(gridNode.transform.position + Vector3.up * halfGridSize.y, halfGridSize, Color.magenta, 60.0f);

        return gridNode;
    }

    private GridNode FloodFill(Vector3 location, bool isOutOfBounds = false)
    {
        GridNode node;
        if (isOutOfBounds)
        {
            node = PlaceTileAt(location, "OOBNode");
            node.isOutOfBounds = true;
        }
        else
        {
            node = PlaceTileAt(location);
            node.isOutOfBounds = false;
        }

        if (node.BackNeighbour == null)
        {
            GridNode backNeighbour = ExploreDirection(node, Vector3.back);
            if (backNeighbour != null)
            {
                GridNode.SetNeighbourBackFront(backNeighbour, node);
            }
        }
        if (node.FrontNeighbour == null)
        {
            GridNode frontNeighbour = ExploreDirection(node, Vector3.forward);
            if (frontNeighbour != null)
            {
                GridNode.SetNeighbourBackFront(node, frontNeighbour);
            }
        }
        if (node.LeftNeighbour == null)
        {
            GridNode leftNeighbour = ExploreDirection(node, Vector3.left);
            if (leftNeighbour != null)
            {
                GridNode.SetNeighbourLeftRight(leftNeighbour, node);
            }
        }
        if (node.RightNeighbour == null)
        {
            GridNode rightNeighbour = ExploreDirection(node, Vector3.right);
            if (rightNeighbour != null)
            {
                GridNode.SetNeighbourLeftRight(node, rightNeighbour);
            }
        }

        return node;
    }

    private GridNode BoxCastToFindNeighbour(GridNode origin, Vector3 direction, float distance, out bool weHitGeometry)
    {
        Vector3 centerOfOriginNode =
                   origin.gameObject.transform.position + (halfGridSize.y * Vector3.up);

        Vector3 cubeExtents = halfGridSize * (1.0f - maxSlope);

        //Utils.DrawDebugBox(centerOfOriginNode, cubeExtents, Color.magenta, 60.0f);
        //Utils.DrawDebugBox(direction * distance + centerOfOriginNode, cubeExtents, Color.red, 60.0f);

        RaycastHit[] hits = Physics.BoxCastAll(
            center: centerOfOriginNode,
            halfExtents: cubeExtents,
            direction: direction,
            orientation: Quaternion.identity,
            maxDistance: distance,
            layerMask: nonIgnoredLayers,
            queryTriggerInteraction: QueryTriggerInteraction.Collide);

        weHitGeometry = false;
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out GridNode neighbour))
            {
                // Yes, we apparently can hit outselves.
                if (neighbour != origin)
                {
                    // We hit an existing neighbour.
                    return neighbour;
                }
            }
            else
            {
                weHitGeometry = true;
            }
        }
        return null;
    }


    private GridNode ExploreDirection(GridNode origin, Vector3 direction)
    {
        // This avoids a branch.
        float distance = Mathf.Abs(
            (direction.x * gridSize.x) +
            (direction.y * gridSize.y) +
            (direction.z * gridSize.z));

        GridNode neighbour = BoxCastToFindNeighbour(origin, direction, distance, out bool weHitGeometry);
        if (neighbour != null)
            return neighbour;

        if (!weHitGeometry)
        {
            Vector3 candidatePosition =
                origin.gameObject.transform.position +
                direction * distance;

            float distanceToGround = GetDistanceToGround(candidatePosition);
            
            if(distanceToGround < halfGridSize.y * maxSlope)
            {
                return FloodFill(candidatePosition + (Vector3.down * distanceToGround));
            }
            else if(distanceToGround <= maxDropHeight)
            {
                GridNode dropTile = PlaceDropTileAt(candidatePosition);
                dropTile.DownNeighbour = FloodFill(candidatePosition + (Vector3.down * distanceToGround));
                return dropTile;                
            }
            else
            {
                // Drop is too high.
                return null;
            }
        }
        else
        {
            return FloodFill(origin.transform.position + Vector3.Scale(direction, gridSize), true);
        }
    }

    private GridNode PlaceDropTileAt(Vector3 location)
    {
        return PlaceTileAt(location, "Drop");
    }

    private float GetDistanceToGround(Vector3 position)
    {
        // It could be a little bit above us, or below us.
        //float halfFloorSearchBounds = halfGridSize.y - (1 - halfGridSize.y * maxSlope);
        float halfFloorSearchBounds = halfGridSize.y * maxSlope;
        Vector3 rayStartPoint = position + Vector3.up * halfFloorSearchBounds;

        /*Debug.DrawLine(rayStartPoint,
            rayStartPoint + Vector3.down * halfFloorSearchBounds,
            Color.yellow, 60.0f);*/

        RaycastHit[] findingTheFloor =
            Physics.RaycastAll(
            origin: rayStartPoint,
            direction: Vector3.down,
            maxDistance: maxDropHeight);

        float closestHit = float.PositiveInfinity;
        foreach (var hit in findingTheFloor)
        {
            closestHit = (hit.distance - halfFloorSearchBounds);
        }
        return closestHit;
    }
}


