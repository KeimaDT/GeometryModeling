using UnityEngine;

public class saisons : MonoBehaviour
{
    public ComputeShader computeShader;

    private Mesh terrainMesh;
    public Mesh grassMesh;
    public Mesh snowMesh;
    public Mesh leavesMesh; // Mesh pour les feuilles
    public Mesh dryGrassMesh; // Mesh pour l'herbe séchée

    public Material grassMaterial;
    public Material snowMaterial;
    public Material leavesMaterial; // Matériau pour les feuilles
    public Material dryGrassMaterial; // Matériau pour l'herbe séchée

    public float scale = 0.1f;
    public Vector2 minMaxBladeHeight = new Vector2(0.5f, 1.5f);

    private GraphicsBuffer terrainTriangleBuffer;
    private GraphicsBuffer terrainVertexBuffer;

    private GraphicsBuffer transformMatrixBuffer;

    private GraphicsBuffer grassTriangleBuffer;
    private GraphicsBuffer grassVertexBuffer;
    private GraphicsBuffer grassUVBuffer;

    private GraphicsBuffer snowTriangleBuffer;
    private GraphicsBuffer snowVertexBuffer;
    private GraphicsBuffer snowUVBuffer;

    private GraphicsBuffer leavesTriangleBuffer; // Buffer pour les triangles des feuilles
    private GraphicsBuffer leavesVertexBuffer; // Buffer pour les vertices des feuilles
    private GraphicsBuffer leavesUVBuffer; // Buffer pour les UVs des feuilles

    private GraphicsBuffer dryGrassTriangleBuffer; // Buffer pour les triangles de l'herbe séchée
    private GraphicsBuffer dryGrassVertexBuffer; // Buffer pour les vertices de l'herbe séchée
    private GraphicsBuffer dryGrassUVBuffer; // Buffer pour les UVs de l'herbe séchée

    private Bounds bounds;

    private int kernel;
    private uint threadGroupSize;
    private int terrainTriangleCount = 0;

    private float timeElapsed = 0f;
    private int state = 0;
    private const int numStates = 4; // Nombre total d'états

    private void Start()
    {
        kernel = computeShader.FindKernel("TerrainOffsets");

        terrainMesh = GetComponent<MeshFilter>().sharedMesh;

        // Terrain data for the compute shader.
        Vector3[] terrainVertices = terrainMesh.vertices;
        terrainVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainVertices.Length, sizeof(float) * 3);
        terrainVertexBuffer.SetData(terrainVertices);
        computeShader.SetBuffer(kernel, "_TerrainPositions", terrainVertexBuffer);

        int[] terrainTriangles = terrainMesh.triangles;
        terrainTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangles.Length, sizeof(int));
        terrainTriangleBuffer.SetData(terrainTriangles);
        computeShader.SetBuffer(kernel, "_TerrainTriangles", terrainTriangleBuffer);

        terrainTriangleCount = terrainTriangles.Length / 3;

        // Grass data for RenderPrimitives.
        SetupGrassBuffers();

        // Snow data for RenderPrimitives.
        SetupSnowBuffers();

        // Leaves data for RenderPrimitives.
        SetupLeavesBuffers();

        // Dry grass data for RenderPrimitives.
        SetupDryGrassBuffers();

        transformMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangleCount, sizeof(float) * 16);
        computeShader.SetBuffer(kernel, "_TransformMatrices", transformMatrixBuffer);

        // Set bounds.
        bounds = terrainMesh.bounds;
        bounds.center += transform.position;
        bounds.Expand(minMaxBladeHeight.y);

        RunComputeShader();
    }

    private void SetupGrassBuffers()
    {
        Vector3[] grassVertices = grassMesh.vertices;
        grassVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassVertices.Length, sizeof(float) * 3);
        grassVertexBuffer.SetData(grassVertices);

        int[] grassTriangles = grassMesh.triangles;
        grassTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassTriangles.Length, sizeof(int));
        grassTriangleBuffer.SetData(grassTriangles);

        Vector2[] grassUVs = grassMesh.uv;
        grassUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassUVs.Length, sizeof(float) * 2);
        grassUVBuffer.SetData(grassUVs);
    }

    private void SetupSnowBuffers()
    {
        Vector3[] snowVertices = snowMesh.vertices;
        snowVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, snowVertices.Length, sizeof(float) * 3);
        snowVertexBuffer.SetData(snowVertices);

        int[] snowTriangles = snowMesh.triangles;
        snowTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, snowTriangles.Length, sizeof(int));
        snowTriangleBuffer.SetData(snowTriangles);

        Vector2[] snowUVs = snowMesh.uv;
        snowUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, snowUVs.Length, sizeof(float) * 2);
        snowUVBuffer.SetData(snowUVs);
    }

    private void SetupLeavesBuffers()
    {
        Vector3[] leavesVertices = leavesMesh.vertices;
        leavesVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leavesVertices.Length, sizeof(float) * 3);
        leavesVertexBuffer.SetData(leavesVertices);

        int[] leavesTriangles = leavesMesh.triangles;
        leavesTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leavesTriangles.Length, sizeof(int));
        leavesTriangleBuffer.SetData(leavesTriangles);

        Vector2[] leavesUVs = leavesMesh.uv;
        leavesUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leavesUVs.Length, sizeof(float) * 2);
        leavesUVBuffer.SetData(leavesUVs);
    }

    private void SetupDryGrassBuffers()
    {
        Vector3[] dryGrassVertices = dryGrassMesh.vertices;
        dryGrassVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dryGrassVertices.Length, sizeof(float) * 3);
        dryGrassVertexBuffer.SetData(dryGrassVertices);

        int[] dryGrassTriangles = dryGrassMesh.triangles;
        dryGrassTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dryGrassTriangles.Length, sizeof(int));
        dryGrassTriangleBuffer.SetData(dryGrassTriangles);

        Vector2[] dryGrassUVs = dryGrassMesh.uv;
        dryGrassUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dryGrassUVs.Length, sizeof(float) * 2);
        dryGrassUVBuffer.SetData(dryGrassUVs);
    }

    private void RunComputeShader()
    {
        computeShader.SetMatrix("_TerrainObjectToWorld", transform.localToWorldMatrix);
        computeShader.SetInt("_TerrainTriangleCount", terrainTriangleCount);
        computeShader.SetVector("_MinMaxBladeHeight", minMaxBladeHeight);
        computeShader.SetFloat("_Scale", scale);

        computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(terrainTriangleCount / threadGroupSize);
        computeShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= 5f)
        {
            state = (state + 1) % numStates;
            timeElapsed = 0f;
        }

        RenderParams rp = new RenderParams(GetMaterialForState());
        rp.worldBounds = bounds;
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_TransformMatrices", transformMatrixBuffer);

        switch (state)
        {
            case 0:
                rp.matProps.SetBuffer("_Positions", grassVertexBuffer);
                rp.matProps.SetBuffer("_UVs", grassUVBuffer);
                Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count, instanceCount: terrainTriangleCount);
                break;
            case 1:
                rp.matProps.SetBuffer("_Positions", dryGrassVertexBuffer);
                rp.matProps.SetBuffer("_UVs", dryGrassUVBuffer);
                Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, dryGrassTriangleBuffer, dryGrassTriangleBuffer.count, instanceCount: terrainTriangleCount);
                break;
            case 2:
                rp.matProps.SetBuffer("_Positions", leavesVertexBuffer);
                rp.matProps.SetBuffer("_UVs", leavesUVBuffer);
                Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, leavesTriangleBuffer, leavesTriangleBuffer.count, instanceCount: terrainTriangleCount);
                break;
            case 3:
                rp.matProps.SetBuffer("_Positions", snowVertexBuffer);
                rp.matProps.SetBuffer("_UVs", snowUVBuffer);
                Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, snowTriangleBuffer, snowTriangleBuffer.count, instanceCount: terrainTriangleCount);
                break;
        }
    }

    private Material GetMaterialForState()
    {
        switch (state)
        {
            case 0:
                return grassMaterial;
            case 1:
                return dryGrassMaterial;
            case 2:
                return leavesMaterial;
            case 3:
                return snowMaterial;
            default:
                return grassMaterial;
        }
    }

    private void OnDestroy()
    {
        terrainTriangleBuffer.Dispose();
        terrainVertexBuffer.Dispose();
        transformMatrixBuffer.Dispose();

        grassTriangleBuffer.Dispose();
        grassVertexBuffer.Dispose();
        grassUVBuffer.Dispose();

        dryGrassTriangleBuffer.Dispose();
        dryGrassVertexBuffer.Dispose();
        dryGrassUVBuffer.Dispose();

        leavesTriangleBuffer.Dispose();
        leavesVertexBuffer.Dispose();
        leavesUVBuffer.Dispose();
        
        snowTriangleBuffer.Dispose();
        snowVertexBuffer.Dispose();
        snowUVBuffer.Dispose();
    }
}
