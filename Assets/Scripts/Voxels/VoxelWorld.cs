using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;

/// <summary>
/// handles chunks. all chunks need to be in a world
/// </summary>
public class VoxelWorld : MonoBehaviour {

    public float voxelSize = 1;
    public int defaultChunkResolution = 16;
    // [SerializeField] List<VoxelChunk> world = new List<VoxelChunk>();

    [Space]
    [SerializeField] GameObject voxelChunkPrefab;
    [SerializeField] List<VoxelChunk> allChunks = new List<VoxelChunk>();
    [SerializeField] bool genOnStart = false;

    UnityEngine.Pool.ObjectPool<GameObject> chunkPool;

    public float chunkSize => voxelSize * defaultChunkResolution;
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
            false, 20, 1000);
    }
    private void OnDisable() {
        chunkPool.Dispose();
    }
    private void Start() {
        if (Application.isPlaying && genOnStart) {
            StartGeneration();
        }
    }

    [ContextMenu("Clear")]
    public void Clear() {
        if (allChunks == null) {
            allChunks = new List<VoxelChunk>();
            // worlddict = new Dictionary<Vector3Int, VoxelChunk>();
            // chunksToPopulate = new List<int>();
        } else {
            for (int i = allChunks.Count - 1; i >= 0; i--) {
                if (!allChunks[i]) continue;
                VoxelChunk chunk = allChunks[i];
                chunk.Clear();
                chunkPool.Release(chunk.gameObject);
            }
            allChunks.Clear();
            // worlddict.Clear();
            // chunksToPopulate.Clear();
        }
    }
    [ContextMenu("ReGen")]
    public void StartGeneration() {
        Clear();
        GenChunk(Vector3Int.zero);
        // AddChunksCube(0, 0, 0, startRes.x, startRes.y, startRes.z);
    }
    public void GenChunk(Vector3Int chunkPos) {
        GameObject chunkgo = chunkPool.Get();
        chunkgo.transform.localPosition = (Vector3)chunkPos * chunkSize;
        chunkgo.name = $"chunk {chunkPos.x},{chunkPos.y},{chunkPos.z}";
        VoxelChunk chunk = chunkgo.GetComponent<VoxelChunk>();
        chunk.Initialize(this, chunkPos, defaultChunkResolution);
        allChunks.Add(chunk);
        // worlddict.Add(cp, chunk);
        chunk.Refresh();
    }
}