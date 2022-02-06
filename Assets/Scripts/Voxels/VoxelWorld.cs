using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using Unity.Mathematics;
using System.Linq;

/// <summary>
/// handles chunks. all chunks need to be in a world
/// </summary>
public class VoxelWorld : MonoBehaviour {

    public float voxelSize = 1;
    public int defaultChunkResolution = 16;
    public bool enableCollision = true;
    public bool useBoxColliders = true;

    [Space]
    [SerializeField] GameObject voxelChunkPrefab;
    [SerializeField] List<VoxelChunk> _activeChunks = new List<VoxelChunk>();
    [SerializeField] Dictionary<Vector3Int, VoxelChunk> activeChunksDict = new Dictionary<Vector3Int, VoxelChunk>();
    [SerializeField] bool genOnStart = false;

    UnityEngine.Pool.ObjectPool<GameObject> chunkPool;

    public event System.Action<Vector3Int> generateChunkEvent;

    public float chunkSize => voxelSize * defaultChunkResolution;

    public List<VoxelChunk> activeChunks { get => _activeChunks; private set => _activeChunks = value; }
    public List<Vector3Int> activeChunksPos => activeChunksDict.Keys.ToList();

    private void OnEnable() {
        chunkPool = new UnityEngine.Pool.ObjectPool<GameObject>(
            () => {
                return Instantiate(voxelChunkPrefab, transform);
            },
            (chunk) => { chunk.SetActive(true); },
            (chunk) => { chunk.SetActive(false); },
            (chunk) => {
                if (Application.isPlaying) {
                    Destroy(chunk);
                } else {
                    DestroyImmediate(chunk);
                }
            },
            true, 50, 1000);
    }
    private void OnDisable() {
        RemoveAllChunks();
        chunkPool.Dispose();
    }
    private void Start() {
        if (Application.isPlaying && genOnStart) {
            StartGeneration();
        }
    }
    public void SaveVoxels() {

    }
    public void LoadVoxels() {

    }

    [ContextMenu("Refresh")]
    public void RefreshAll() {
        foreach (var chunk in activeChunks) {
            chunk.Refresh();
        }
    }
    [ContextMenu("Clear")]
    public void Clear() {
        if (activeChunks == null) {
            activeChunks = new List<VoxelChunk>();
            activeChunksDict = new Dictionary<Vector3Int, VoxelChunk>();
            // chunksToPopulate = new List<int>();
        } else {
            for (int i = activeChunks.Count - 1; i >= 0; i--) {
                if (!activeChunks[i]) continue;
                VoxelChunk chunk = activeChunks[i];
                chunk.Clear();
                chunkPool.Release(chunk.gameObject);
            }
            activeChunks.Clear();
            activeChunksDict.Clear();
            // chunksToPopulate.Clear();
        }
    }
    [ContextMenu("ReGen")]
    public void StartGeneration() {
        Clear();
        LoadChunks(Vector3Int.zero);
        // AddChunksCube(0, 0, 0, startRes.x, startRes.y, startRes.z);
    }
    void GenEmptyChunk(Vector3Int chunkPos) {
        var chunk = CreateChunk(chunkPos);
        chunk.SetAll(new Voxel { blockId = 0 });
        chunk.Refresh();
    }

    VoxelChunk CreateChunk(Vector3Int chunkPos) {
        GameObject chunkgo = chunkPool.Get();
        chunkgo.transform.localPosition = (Vector3)chunkPos * chunkSize;
        chunkgo.name = $"chunk {chunkPos.x},{chunkPos.y},{chunkPos.z}";
        VoxelChunk chunk = chunkgo.GetComponent<VoxelChunk>();
        chunk.Initialize(this, chunkPos, defaultChunkResolution);
        activeChunks.Add(chunk);
        activeChunksDict.Add(chunkPos, chunk);
        return chunk;
    }

    void AddChunks(params Vector3Int[] chunkposs) {
        foreach (var cp in chunkposs) {
            if (HasChunkActiveAt(cp))
                continue;
            CreateChunk(cp);
        }
        // todo refresh neighbor chunks?
        // maybe just when loading?
    }
    void RemoveAllChunks() {
        RemoveChunks(activeChunksPos.ToArray());
    }
    void RemoveChunks(params Vector3Int[] chunkposs) {
        HashSet<Vector3Int> chunksToRefresh = new HashSet<Vector3Int>();
        foreach (var cp in chunkposs) {
            VoxelChunk chunk = GetChunkAt(cp);
            if (!chunk) continue;
            chunk.Clear();
            activeChunks.Remove(chunk);
            activeChunksDict.Remove(cp);
            chunkPool.Release(chunk.gameObject);
            Voxel.GetUnitNeighbors(cp).ToList().ForEach((cp) => chunksToRefresh.Add(cp));
        }
        foreach (var cpos in chunksToRefresh.Except(chunkposs)) {
            GetChunkAt(cpos)?.Refresh();
        }
    }

    public void LoadChunks(params Vector3Int[] chunkposs) {
        // todo multithread
        foreach (var cp in chunkposs) {
            AddChunks(cp);
            // todo restore if have data or generate
            generateChunkEvent?.Invoke(cp);
            // GetChunkAt(cp).Refresh();
        }
    }
    public void UnloadChunks(params Vector3Int[] chunkposs) {
        foreach (var cp in chunkposs) {
            // todo save each chunk - only if dirty
            RemoveChunks(cp);
        }
    }


    public bool HasChunkActiveAt(Vector3Int cpos) {
        // return world.Exists(c => c.chunkPos == pos);
        return activeChunksDict.ContainsKey(cpos);
    }
    public VoxelChunk GetChunkAt(Vector3Int cpos) {
        // return world.Find(c => c.chunkPos == pos);
        return HasChunkActiveAt(cpos) ? activeChunksDict[cpos] : null;
    }
    public VoxelChunk GetNeighbor(VoxelChunk start, Vector3Int dir) {
        Vector3Int npos = start.chunkPos + dir;
        return GetChunkAt(npos);
    }

    public Vector3 ChunkposToWorldpos(Vector3 cpos) {
        Vector3 wpos = cpos * chunkSize;
        wpos = transform.TransformPoint(wpos);
        return wpos;
    }
    public Vector3 ChunkposToWorldposCenter(Vector3 cpos) {
        var wpos = ChunkposToWorldpos(cpos) + (chunkSize - 1) / 2 * Vector3.one;
        return wpos;
    }
    public Vector3Int WorldposToChunkpos(Vector3 wpos) {
        wpos = transform.InverseTransformPoint(wpos);
        Vector3Int cpos = Vector3Int.FloorToInt(wpos / chunkSize);
        return cpos;
    }
}