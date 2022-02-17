using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using Unity.Mathematics;
using System.Linq;

namespace VoxelSystem {
    /// <summary>
    /// handles chunks. all chunks need to be in a world
    /// </summary>
    [DefaultExecutionOrder(-2)]
    public class VoxelWorld : MonoBehaviour {

        public float voxelSize = 1;
        public int defaultChunkResolution = 16;
        public TypeChoice<Mesher.VoxelMesher> mesher = typeof(Mesher.AdvMesher);
        public bool enableCollision = true;
        public bool useBoxColliders = true;
        public VoxelMaterialSetSO materialSet;

        [Space]
        [SerializeField] GameObject voxelChunkPrefab;// todo remove
        [SerializeField] List<VoxelChunk> _activeChunks = new List<VoxelChunk>();
        // todo serialized dict
        [SerializeField] Dictionary<Vector3Int, VoxelChunk> activeChunksDict = new Dictionary<Vector3Int, VoxelChunk>();

        UnityEngine.Pool.ObjectPool<GameObject> chunkPool;

        public event System.Action<Vector3Int> generateChunkEvent;

        public float chunkSize => voxelSize * defaultChunkResolution;

        public List<VoxelChunk> activeChunks { get => _activeChunks; private set => _activeChunks = value; }
        public List<Vector3Int> activeChunksPos => activeChunksDict.Keys.ToList();

        public TypeChoice<VoxelMaterial> materialType => mesher.CreateInstance().neededMaterial;
        public List<TypeChoice<VoxelData>> neededData => mesher.CreateInstance().neededDatas.ToList();

        private void OnEnable() {
            Clear();
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
        void GenEmptyChunk(Vector3Int chunkPos) {
            var chunk = CreateChunk(chunkPos);
            // chunk.SetAll(new Voxel());
            // todo load from data
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
        public void UnloadAllChunks() {
            UnloadChunks(activeChunksPos.ToArray());
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
        public Vector3Int ChunkPosWithBlock(Vector3 blockpos) {
            return Vector3Int.FloorToInt(blockpos / chunkSize);
        }
        public VoxelChunk GetChunkWithBlock(Vector3Int blockpos) {
            return GetChunkAt(ChunkPosWithBlock(blockpos));
        }
        Vector3Int BlockPosToVoxelPos(Vector3Int blockpos, Vector3Int chunkpos) {
            return blockpos - chunkpos * defaultChunkResolution;
        }
        public BlockType GetBlockTypeAt(Vector3Int blockpos) {
            VoxelChunk chunk = GetChunkWithBlock(blockpos);
            if (chunk) {
                Voxel voxel = chunk.GetLocalVoxelAt(BlockPosToVoxelPos(blockpos, chunk.chunkPos));
                if (voxel == null) {
                    Debug.LogWarning($"Error getting block type cp{chunk.chunkPos} bp{blockpos} vp{BlockPosToVoxelPos(blockpos, chunk.chunkPos)} v{voxel}");
                    return null;
                }
                // todo remove all this block stuff
                return null;
                // return BlockManager.Instance?.GetBlockTypeAtIndex(voxel.blockId) ?? null;
            } else {
                return null;
            }
        }
        // public BlockType GetBlockAt(Vector3Int blockpos) {
        //     return default;
        // }

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
        public Vector3Int WorldposToBlockpos(Vector3 wpos) {
            wpos = transform.InverseTransformPoint(wpos);
            Vector3Int bpos = Vector3Int.FloorToInt(wpos / voxelSize);
            return bpos;
        }
    }
}