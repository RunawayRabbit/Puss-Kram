using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[ExecuteAlways]
[SelectionBase]
[RequireComponent(typeof(BoxCollider))]
public class Fence : MonoBehaviour
{
    [SerializeField] Fence nextFence = default;
    [SerializeField, Range(0.01f, 1.0f)] float pieceDistance = 0.177f;
    [SerializeField, Range(1.5f, 4.0f)] float postDistance = 2.371f;
    [SerializeField, Range(0.0f, 1.0f)] float clearanceDist = 0.011f;
    
    [SerializeField] Vector2 fencePostOffset = new Vector2(-0.1f, -0.1f);
    [SerializeField] Vector3 fencePieceOffset = new Vector3(-0.06f, 0.16f, 0.07f);
    [SerializeField] float fencePieceRotationAngle = 90.0f;

    [SerializeField, Range(0.1f, 1.0f)] float crossbeamHeight = 0.71f;
    [SerializeField, Range(0.1f, 1.0f)] float crossbeamSpacing = 0.413f;
    [SerializeField, Range(-0.3f, 0.3f)] float crossbeamDepth = 0.01f;
    [SerializeField] Material crossbeamMaterial = default;
    [SerializeField] Vector2 crossbeamDim = new Vector2(0.07f, 0.05f);

    [SerializeField] float collisionHeight = 4.0f;
    [SerializeField] float collisionDepth = 0.58f;
    [SerializeField] Vector3 collisionOffset = new Vector3(-0.12f, 0.0f, 0.27f);

    [SerializeField] GameObject fencePiecePrefab = default;
    [SerializeField] GameObject fencePostPrefab = default;

    [SerializeField] private List<GameObject> _fencePieces;

    // @TODO: Generalize this to have n-way fence connections?
    private Fence prevFence = default;
    private BoxCollider col;

    public void SetPreviousFence(Fence newPrevFence)
    {
        if(prevFence == null)
        {
            prevFence = newPrevFence;
        }
    }

    public void YourTargetMoved()
    {
        ResetFence();
        GenerateFence();
    }

    private void Awake()
    {
        if (_fencePieces == null)
        {
            HardResetFence();
            _fencePieces = new List<GameObject>();
        }
        else
            ResetFence();

        if (!TryGetComponent<BoxCollider>(out col))
            col = gameObject.AddComponent<BoxCollider>();
        
        if (nextFence != null) nextFence.SetPreviousFence(this);
    }

    private void Start()
    {
        GenerateFence();
    }

    private void HardResetFence()
    {
        // @NOTE: This is the emergency reset button, for when all else fails!
        int children = transform.childCount;
        for (int i = children - 1; i > 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void ResetFence()
    {        
        for (int i = 0; i < _fencePieces.Count; i++)
        {
            DestroyImmediate(_fencePieces[i]);
        }
        _fencePieces.Clear();        
    }


    private void Update()
    {
        if (!Application.isPlaying && transform.hasChanged)
        {
            if (prevFence) prevFence.YourTargetMoved();
            ResetFence();
            GenerateFence();
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            HardResetFence();
        }
    }

    private void GenerateFence()
    {
        if (!nextFence)
        {
            InstantiateFenceComponent(fencePostPrefab, transform.localPosition);
        }
        else
        { 
            Vector3 toTarget = (nextFence.transform.position - transform.position);
            float totalLength = toTarget.magnitude;
            Vector3 dir = toTarget.normalized;
            Quaternion rot = Quaternion.LookRotation(dir);
            Quaternion fencePieceRotation = rot * Quaternion.Euler(0.0f, fencePieceRotationAngle, 0.0f);

            #region Set BoxCollider Size and Center
            Vector3 collisionSize = rot * new Vector3(collisionDepth, collisionHeight, toTarget.magnitude + (collisionDepth *0.5f));
            col.center = (collisionSize * 0.5f) + rot*collisionOffset;

            collisionSize.x = Mathf.Abs(collisionSize.x);
            collisionSize.y = Mathf.Abs(collisionSize.y);
            collisionSize.z = Mathf.Abs(collisionSize.z);
            col.size = collisionSize;
            #endregion

            MakeCrossBeam(transform.position + Vector3.up * (crossbeamHeight + crossbeamSpacing), totalLength, rot);
            MakeCrossBeam(transform.position + Vector3.up * (crossbeamHeight - crossbeamSpacing), totalLength, rot);
                        
            int postCount = Mathf.FloorToInt(totalLength / postDistance);
            Vector3 postStep = dir * (totalLength / postCount);
            Vector3 postAccumulator = transform.position;
            for (int i = 0; i < postCount; i++, postAccumulator += postStep)
            {
                Vector3 fencePostPos = postAccumulator - new Vector3(fencePostOffset.x, 0.0f, fencePostOffset.y);
                InstantiateFenceComponent(fencePostPrefab, fencePostPos); 

                Vector3 firstPiece = postAccumulator + (dir * clearanceDist);
                Vector3 lastPiece = postAccumulator + postStep - (dir * clearanceDist);

                float sectionLength = (lastPiece - firstPiece).magnitude;

                int numPosts = Mathf.FloorToInt(sectionLength / pieceDistance);

                Vector3 pieceStep = dir * (sectionLength / numPosts);
                Vector3 pieceAccumulator = firstPiece + pieceStep;

                for (int j = 0;
                    j < numPosts-1;
                    j++, pieceAccumulator += pieceStep)
                {
                    Vector3 fencePiecePos = pieceAccumulator - (rot * fencePieceOffset);
                    InstantiateFenceComponent(fencePiecePrefab, fencePiecePos, fencePieceRotation);
                }
            }
        }
    }

    private void MakeCrossBeam(Vector3 position, float length, Quaternion rotation)
    {
        GameObject topBeam = new GameObject("Crossbeam");
        topBeam.transform.position = position;
        topBeam.transform.rotation = rotation;
        topBeam.transform.parent = transform;
        topBeam.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;

        var mesh = new Mesh();

        mesh.Clear();
        
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        
        vertices.Add(new Vector3(crossbeamDim.x + crossbeamDepth, 0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f));

        vertices.Add(new Vector3(crossbeamDim.x + crossbeamDepth, 0.0f, length));
        uvs.Add(new Vector2(0.0f, 1.0f));

        vertices.Add(new Vector3(crossbeamDim.x + crossbeamDepth, crossbeamDim.y, 0.0f));
        uvs.Add(new Vector2(0.5f, 0.0f));

        vertices.Add(new Vector3(crossbeamDim.x + crossbeamDepth, crossbeamDim.y, length));
        uvs.Add(new Vector2(0.5f, 1.0f));

        vertices.Add(new Vector3(0.0f + crossbeamDepth, crossbeamDim.y, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));

        vertices.Add(new Vector3(0.0f + crossbeamDepth, crossbeamDim.y, length));
        uvs.Add(new Vector2(1.0f, 1.0f));

        vertices.Add(new Vector3(0.0f + crossbeamDepth, 0.0f, 0.0f));
        uvs.Add(new Vector2(0.5f, 0.0f));

        vertices.Add(new Vector3(0.0f + crossbeamDepth, 0.0f, length));
        uvs.Add(new Vector2(0.5f, 1.0f));

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        
        List<int> triangleIndices = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            int rootIndex = 2 * i;

            int RootOuter = rootIndex;
            int RootInner = (rootIndex + 1);
            int NextOuter = (rootIndex + 2) % vertices.Count;
            int NextInner = (rootIndex + 3) % vertices.Count;

            triangleIndices.Add(RootOuter);
            triangleIndices.Add(NextOuter);
            triangleIndices.Add(NextInner);

            triangleIndices.Add(RootOuter);
            triangleIndices.Add(NextInner);
            triangleIndices.Add(RootInner);
        }

        mesh.SetTriangles(triangleIndices, 0);
        mesh.RecalculateNormals();

        topBeam.AddComponent<MeshRenderer>().sharedMaterial = crossbeamMaterial;
        topBeam.AddComponent<MeshFilter>().sharedMesh = mesh;

        _fencePieces.Add(topBeam);
    }

    private void InstantiateFenceComponent(GameObject prefab, Vector3 position, Quaternion? rotation = null)
    {
        if (rotation == null) rotation = transform.rotation;

        GameObject fenceComponent = Instantiate(prefab);
        fenceComponent.isStatic = true;
        fenceComponent.transform.position = position;
        fenceComponent.transform.rotation = (Quaternion) rotation;
        fenceComponent.transform.parent = transform;
        fenceComponent.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

        _fencePieces.Add(fenceComponent);
    }
}
