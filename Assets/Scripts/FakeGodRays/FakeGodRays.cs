using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FakeGodRays : MonoBehaviour
{
    [SerializeField, Range(32, 512)] int quality = 64;

    [SerializeField] Color colorA = default;
    [SerializeField] Color colorB = default;

    [SerializeField] float width = 1.0f;
    [SerializeField] float height = 1.25f;
    [SerializeField] float angle = 30.0f;
    [SerializeField] float distance = 6.0f;
    [SerializeField] float spread = 1.3f;

    [SerializeField] Vector2 perlinScale1 = new Vector2(20.0f, 20.0f);
    [SerializeField] Vector2 perlinScale2 = new Vector2(1.0f, 1.0f);
    [SerializeField] float perlinOffsetU = 0.0f;
    [SerializeField] float perlinOffsetV = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] float strength = 0.3f;
    [SerializeField, Range(0.1f, 20.0f)] float speed = 1.6f;

    public Renderer mRenderer;
    public Mesh mesh;

    private void Awake()
    {
        mRenderer = GetComponent<Renderer>();
        mesh = new Mesh();
        mesh.name = name;
        GetComponent<MeshFilter>().sharedMesh = mesh;

        mRenderer.receiveShadows = false;
        mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mRenderer.sharedMaterial.mainTexture = GenerateTexture();
        GenerateMesh();
    }

    private void Update()
    {
        mRenderer.sharedMaterial.SetFloat("_Strength", strength);
        mRenderer.sharedMaterial.SetFloat("_Speed", speed);

        mRenderer.sharedMaterial.mainTexture = GenerateTexture();
    }

    private void OnValidate()
    {
        if(mesh != null) GenerateMesh();
    }

    private void GenerateMesh()
    {
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        vertices.Add(new Vector3(halfWidth * spread, halfHeight * spread - Mathf.Sin(angle) * distance, distance));
        uvs.Add(new Vector2(0.0f, 0.0f));

        vertices.Add(new Vector3(halfWidth, halfHeight, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));
        
        vertices.Add(new Vector3(halfWidth * spread, -halfHeight * spread - Mathf.Sin(angle) * distance, distance));
        uvs.Add(new Vector2(0.5f, 0.0f));

        vertices.Add(new Vector3(halfWidth, -halfHeight, 0.0f));
        uvs.Add(new Vector2(0.5f, 1.0f));
        
        vertices.Add(new Vector3(-halfWidth * spread, -halfHeight * spread - Mathf.Sin(angle) * distance, distance));
        uvs.Add(new Vector2(1.0f, 0.0f));

        vertices.Add(new Vector3(-halfWidth, -halfHeight, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));

        vertices.Add(new Vector3(-halfWidth * spread, halfHeight * spread - Mathf.Sin(angle) * distance, distance));
        uvs.Add(new Vector2(0.5f, 0.0f));

        vertices.Add(new Vector3(-halfWidth, halfHeight, 0.0f));
        uvs.Add(new Vector2(0.5f, 1.0f));


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

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangleIndices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

    }

    private Texture2D GenerateTexture()
    {
        var tex = new Texture2D(quality, quality);

        for(int x = 0; x < quality; x++)
            for (int y = 0; y < quality; y++)
                tex.SetPixel(x, y, GetColour(x,y));

        tex.Apply();
        return tex;
    }

    private Color GetColour(float x, float y)
    {
        float u = x / (float)quality;
        float v = y / (float)quality;

        float perlinSample1 = Mathf.PerlinNoise(
            Mathf.Abs(1.0f - 2 * u * perlinScale1.x) - perlinOffsetU,
            Mathf.Abs(1.0f - 2 * v * perlinScale1.y) - perlinOffsetV);

        float perlinSample2 = Mathf.PerlinNoise(
            Mathf.Abs(1.0f - 2 * u * perlinScale2.x) - perlinOffsetU,
            Mathf.Abs(1.0f - 2 * v * perlinScale2.y) - perlinOffsetV);

        float perlinSample = (perlinSample1 + perlinSample2) * 0.5f;
        Color finalColor = Color.Lerp(colorA, colorB, perlinSample);
        finalColor.a = Mathf.Min(v, perlinSample1);

        return finalColor;
    }
}
