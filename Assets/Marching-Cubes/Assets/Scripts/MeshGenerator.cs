using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    const int threadGroupSize = 8;

    [Header ("General Settings")]
    public DensityGenerator densityGenerator;

    public bool fixedMapSize;
    [ConditionalHide (nameof (fixedMapSize), true)]
    public Vector3Int numChunks = Vector3Int.one;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public Transform viewer;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public float viewDistance = 30;

    [Space ()]
    public bool autoUpdateInEditor = true;
    public bool autoUpdateInGame = true;
    public ComputeShader shader;
    public Material mat;
    public bool generateColliders;

    [Header ("Voxel Settings")]
    public float isoLevel;
    public float boundsSize = 1;
    public Vector3 offset = Vector3.zero;

    [Range (2, 100)]
    public int numPointsPerAxis = 30;

    [Header("Mesh Settings")]
    public float normalDegrees = 180;

    [Header ("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;

    GameObject chunkHolder;
    public string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    bool settingsUpdated;
    [Header("Performance")]
    [Tooltip("If there are chunks to load, this will be the maximum fps.")]
    public float fpsBreakout = 60f;
    [Tooltip("Smaller chunks load faster when in blocking mode. Larger chunks work better in non-blocking mode, but can be limited by the collider update routine if colliders are needed.")]
    public bool blockingGpu = true;
    [Tooltip("Asynchronous garbage collection and PCG can cause lag spikes if not set right, so we call it in our timer every once in a while.")]
    public int collect_countdown = 10;

    void Awake () {
        if (Application.isPlaying && !fixedMapSize) {

            InitVariableChunkStructures();

            var oldChunks = FindObjectsOfType<Chunk> ();
            for (int i = oldChunks.Length - 1; i >= 0; i--) {
                Destroy (oldChunks[i].gameObject);
            }
        }
    }

    void Update () {
        // Update endless terrain
        if ((Application.isPlaying && !fixedMapSize)) {
            Run ();
        }

        if (settingsUpdated) {
            RequestMeshUpdate ();
            settingsUpdated = false;
        }
    }

    public void Run () {
        CreateBuffers ();

        if (fixedMapSize) {
            InitChunks ();
            UpdateAllChunks ();

        } else {
            if (Application.isPlaying) {
                InitVisibleChunks ();
            }
        }

        // Release buffers immediately in editor
        if (!Application.isPlaying) {
            ReleaseBuffers ();
        }

    }

    public void RequestMeshUpdate () {
        if ((Application.isPlaying && autoUpdateInGame) || (!Application.isPlaying && autoUpdateInEditor)) {
            Run ();
        }
    }

    void InitVariableChunkStructures () {
        recycleableChunks = new Queue<Chunk> (10192);
        chunks = new List<Chunk> (10192);
        existingChunks = new Dictionary<Vector3Int, Chunk> (10192);
    }


    


    void InitVisibleChunks () {
        if (chunks==null) {
            return;
        }
        CreateChunkHolder ();

        Vector3 p = viewer.position;
        Vector3 ps = p / boundsSize;
        Vector3Int viewerCoord = new Vector3Int (Mathf.RoundToInt (ps.x), Mathf.RoundToInt (ps.y), Mathf.RoundToInt (ps.z));

        int maxChunksInView = Mathf.CeilToInt (viewDistance / boundsSize);
        float sqrViewDistance = viewDistance * viewDistance;

        // Go through all existing chunks and flag for recyling if outside of max view dst
        float t0 = Time.realtimeSinceStartup;

        Chunk chunk;
        Vector3 centre;
        Vector3 viewerOffset;
        Vector3 o;
        float sqrDst;
        for (int i = chunks.Count - 1; i >= 0; i--) {
            if (Time.realtimeSinceStartup - t0 > (1.0 / fpsBreakout))
            {
                return;
            }

            chunk = chunks[i];
            centre = CentreFromCoord (chunk.coord);
            viewerOffset = p - centre;
            o = new Vector3 (Mathf.Abs (viewerOffset.x), Mathf.Abs (viewerOffset.y), Mathf.Abs (viewerOffset.z)) - Vector3.one * boundsSize / 2;
            sqrDst = new Vector3 (Mathf.Max (o.x, 0), Mathf.Max (o.y, 0), Mathf.Max (o.z, 0)).sqrMagnitude;
            if (sqrDst > sqrViewDistance) {
                existingChunks.Remove (chunk.coord);
                recycleableChunks.Enqueue (chunk);
                chunks.RemoveAt (i);
            }
        }

        collect_countdown -= 1;
        if (collect_countdown == 0)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false);
            collect_countdown = 10;
        }

        Vector3Int coord;
        foreach (Vector3Int spi in SpiralIterators.Cube(new Vector3Int(maxChunksInView, maxChunksInView, maxChunksInView)))
        {
            if (Time.realtimeSinceStartup - t0 > (1.0 / fpsBreakout))
            {
                return;
            }

            coord = spi + viewerCoord;

            if (existingChunks.ContainsKey(coord))
            {
                
                if(!blockingGpu && (((!existingChunks[coord].asyncOp0?.done) ?? true) ||
                    ((!existingChunks[coord].asyncOp1?.done) ?? true) ||
                    ((!existingChunks[coord].asyncOp2?.done) ?? true)))
                {
                    // wait for centermost chunks to finish
                    return;
                }
                else
                {
                    continue;
                }
            }

            centre = CentreFromCoord(coord);
            viewerOffset = p - centre;
            o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * boundsSize / 2;
            sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

            // Chunk is within view distance and should be created (if it doesn't already exist)
            if (sqrDst <= sqrViewDistance)
            {

                Bounds bounds = new Bounds(CentreFromCoord(coord), Vector3.one * boundsSize);
                if (IsVisibleFrom(bounds, Camera.main))
                {
                    if (recycleableChunks.Count > 0)
                    {
                        chunk = recycleableChunks.Dequeue();
                        chunk.coord = coord;
                        existingChunks.Add(coord, chunk);
                        chunks.Add(chunk);
                        UpdateChunkMesh(chunk, blockingGpu);
                    }
                    else
                    {
                        chunk = CreateChunk(coord);
                        chunk.coord = coord;
                        chunk.SetUp(mat, generateColliders);
                        existingChunks.Add(coord, chunk);
                        chunks.Add(chunk);
                        UpdateChunkMesh(chunk, blockingGpu);
                    }
                }
            }
            
        }
    }

    public bool IsVisibleFrom (Bounds bounds, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes (camera);
        return GeometryUtility.TestPlanesAABB (planes, bounds);
    }

    int maxTris=-1;
    Triangle[] tris;
    Vector3[] vertices;
    int[] meshTriangles;

    public void UpdateChunkMesh (Chunk chunk, bool? blocking=null) {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt (numVoxelsPerAxis / (float) threadGroupSize);
        float pointSpacing = boundsSize / (numPointsPerAxis - 1);

        bool blockGpu = blocking?? blockingGpu;

        Vector3Int coord = chunk.coord;
        Vector3 centre = CentreFromCoord (coord);

        Vector3 worldBounds = new Vector3 (numChunks.x, numChunks.y, numChunks.z) * boundsSize;

        densityGenerator.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);

        void SetupShader()
        {
            triangleBuffer.SetCounterValue(0);
            shader.SetBuffer(0, "points", pointsBuffer);
            shader.SetBuffer(0, "triangles", triangleBuffer);
            shader.SetInt("numPointsPerAxis", numPointsPerAxis);
            shader.SetFloat("isoLevel", isoLevel);

            shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        }

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

        void HandleReadBack()
        {
            int[] triCountArray = { 0 };
            triCountBuffer.GetData(triCountArray);

            int numTris = triCountArray[0];

            if (numTris > maxTris || tris==null || vertices==null || meshTriangles==null)
            {
                maxTris = numTris;
                tris = new Triangle[numTris];
                vertices = new Vector3[numTris * 3];
                meshTriangles = new int[numTris * 3];
            }

            // Get triangle data from shader
            triangleBuffer.GetData(tris, 0, 0, numTris);

            Mesh mesh = chunk.mesh;
            mesh.Clear();

            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j];
                }
            }
            mesh.vertices = vertices;
            mesh.triangles = meshTriangles;
            var scale = chunk.GetComponent<Transform>().localScale;
            mesh.SetUVs(0, UvCalculator.CalculateUVs(vertices, scale.magnitude));
            NormalSolver.RecalculateNormals(mesh, normalDegrees);

            chunk.UpdateColliders();
        }

        if (!blockGpu)
        {
            chunk.asyncOp0 = null;
            chunk.asyncOp1 = null;
            chunk.asyncOp2 = null;
            var async0 = AsyncGPUReadback.Request(pointsBuffer, delegate (AsyncGPUReadbackRequest a0)
            {
                SetupShader();
                var async1 = AsyncGPUReadback.Request(triangleBuffer, delegate (AsyncGPUReadbackRequest a1)
                {
                // Get number of triangles in the triangle buffer
                ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
                    var async2 = AsyncGPUReadback.Request(triCountBuffer, delegate (AsyncGPUReadbackRequest a2)
                    {
                        HandleReadBack();
                    });
                    chunk.asyncOp2 = async2;
                });
                chunk.asyncOp1 = async1;
            });
            chunk.asyncOp0 = async0;
        }
        else
        {
            SetupShader();
            ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            HandleReadBack();
        }
        

    }

    public void UpdateAllChunks () {

        // Create mesh for each chunk
        foreach (Chunk chunk in chunks) {
            UpdateChunkMesh (chunk);
        }

    }

    void OnDestroy () {
        if (Application.isPlaying) {
            ReleaseBuffers ();
        }
    }

    void CreateBuffers () {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || (pointsBuffer == null || numPoints != pointsBuffer.count)) {
            if (Application.isPlaying) {
                ReleaseBuffers ();
            }
            triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer (numPoints, sizeof (float) * 4);
            triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        }
    }

    void ReleaseBuffers () {
        if (triangleBuffer != null) {
            triangleBuffer.Release ();
            pointsBuffer.Release ();
            triCountBuffer.Release ();
        }
    }

    Vector3 CentreFromCoord (Vector3Int coord) {
        // Centre entire map at origin
        if (fixedMapSize) {
            Vector3 totalBounds = (Vector3) numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3) coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        return new Vector3 (coord.x, coord.y, coord.z) * boundsSize;
    }

    void CreateChunkHolder () {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find (chunkHolderName)) {
                chunkHolder = GameObject.Find (chunkHolderName);
            } else {
                chunkHolder = new GameObject(chunkHolderName);

                if (generateColliders)
                {
                    //add rigidbody so collisions are enforced
                    var rigidBody = chunkHolder.AddComponent<Rigidbody>();
                    rigidBody.useGravity = false;
                    rigidBody.isKinematic = true;
                    rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                    rigidBody.detectCollisions = true;
                }
                
            }
        }
    }

    // Create/get references to all chunks
    void InitChunks () {
        CreateChunkHolder ();
        chunks = new List<Chunk> ();
        List<Chunk> oldChunks = new List<Chunk> (FindObjectsOfType<Chunk> ());

        // Go through all coords and create a chunk there if one doesn't already exist
        for (int x = 0; x < numChunks.x; x++) {
            for (int y = 0; y < numChunks.y; y++) {
                for (int z = 0; z < numChunks.z; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z);
                    bool chunkAlreadyExists = false;

                    // If chunk already exists, add it to the chunks list, and remove from the old list.
                    for (int i = 0; i < oldChunks.Count; i++) {
                        if (oldChunks[i].coord == coord) {
                            chunks.Add (oldChunks[i]);
                            oldChunks.RemoveAt (i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    if (!chunkAlreadyExists) {
                        var newChunk = CreateChunk (coord);
                        chunks.Add (newChunk);
                    }

                    chunks[chunks.Count - 1].SetUp (mat, generateColliders);

                }
            }
        }

        // Delete all unused chunks
        for (int i = 0; i < oldChunks.Count; i++) {
            oldChunks[i].DestroyOrDisable ();
        }
    }

    Chunk CreateChunk (Vector3Int coord) {
        GameObject chunk = new GameObject ($"{chunkHolderName} Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk> ();
        newChunk.coord = coord;
        return newChunk;
    }

    void OnValidate() {
        settingsUpdated = true;
    }

    struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this [int i] {
            get {
                switch (i) {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    void OnDrawGizmos () {
        if (showBoundsGizmo) {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk> (FindObjectsOfType<Chunk> ()) : this.chunks;
            foreach (var chunk in chunks) {
                Bounds bounds = new Bounds (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
            }
        }
    }

}