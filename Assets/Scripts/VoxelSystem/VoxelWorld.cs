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
    public class VoxelWorld : MonoBehaviour {// todo add new class that renders voxels without chunks

        [Header("Voxel settings")]
        public float voxelSize = 1;
        public int chunkResolution = 16;
        public TypeChoice<Mesher.VoxelMesher> mesher = typeof(Mesher.AdvMesher);
        public bool renderNullSides = true;
        public bool enableCollision = true;
        public bool useBoxColliders = true;
        public bool clearOnAwake = true;
        public bool clearOnDisable = true;
        public VoxelMaterialSetSO materialSet;

        public List<TypeChoice<VoxelData>> additionalData = new List<TypeChoice<VoxelData>>();

        [Space]
        // [SerializeField] GameObject voxelChunkPrefab;// todo? remove 
        [SerializeField] List<VoxelChunk> _activeChunks = new List<VoxelChunk>();
        SerializableDictionary<Vector3Int, VoxelChunk> activeChunksDict = new SerializableDictionary<Vector3Int, VoxelChunk>();

        UnityEngine.Pool.ObjectPool<GameObject> chunkPool;

        public event System.Action<Vector3Int> generateChunkEvent;
        public event System.Action<Importer.ImportedVoxel, Voxel> loadImportPopulateEvent;
        // public event System.Action<Vector3Int> onChunkLoadEvent;
        // public event System.Action<Vector3Int> onChunkUnLoadEvent;

        public float chunkSize => voxelSize * chunkResolution;

        public List<VoxelChunk> activeChunks { get => _activeChunks; private set => _activeChunks = value; }
        public List<Vector3Int> activeChunksPos => activeChunksDict.Keys.ToList();

        public TypeChoice<VoxelMaterial> materialType => mesher.CreateInstance().neededMaterial;
        private TypeChoice<VoxelData>[] _neededData;
        public TypeChoice<VoxelData>[] neededData { get => _neededData ?? UpdateNeededData(); }
        // [System.NonSerialized]
        // public List<int> testHash = new List<int>();

        private void OnValidate() {
            additionalData.ForEach(tc => tc.onlyIncludeConcreteTypes = true);
            UpdateNeededData();
        }
        private void Awake() {
            if (clearOnAwake) {
                ClearPool();
                Clear();
            }
        }
        private void OnEnable() {
            if (clearOnDisable || chunkPool == null) {
                ClearPool();
                Clear();
                Initialize();
            }
        }
        [ContextMenu("Init")]
        void Initialize() {
            chunkPool = new UnityEngine.Pool.ObjectPool<GameObject>(
                () => {
                    GameObject chunk;
                    // if (voxelChunkPrefab != null) {
                    //     chunk = Instantiate(voxelChunkPrefab, transform);
                    // } else {
                    chunk = new GameObject("Voxel chunk", typeof(VoxelChunk), typeof(VoxelRenderer));
                    chunk.transform.parent = transform;
                    // }
                    return chunk;
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
            // set active chunks dict
            FixDict();
            UpdateNeededData();
        }
        void FixDict(){
            activeChunksDict ??= (SerializableDictionary<Vector3Int, VoxelChunk>)(activeChunks?.ToDictionary(vc => vc.chunkPos));
        }
        private void OnDisable() {
            if (clearOnDisable) {
                Clear();
                ClearPool();
            }
        }
        private void OnDestroy() {
            RemoveAllChunks();
            ClearPool();
        }
        public TypeChoice<VoxelData>[] UpdateNeededData() {
            List<TypeChoice<VoxelData>> datas = new List<TypeChoice<VoxelData>>(additionalData);
            TypeChoice<VoxelData>[] mesherDatas = mesher.CreateInstance().neededDatas;
            datas.AddRange(mesherDatas);
            datas.Add(typeof(DefaultVoxelData));
            _neededData = datas.ToHashSet().ToArray();
            return neededData;
        }

        [ContextMenu("Refresh all chunks")]
        public void RefreshAll() {
            foreach (var chunk in activeChunks) {
                chunk.Refresh();
            }
        }
        public void RefreshChunks(params Vector3Int[] chunkspos) {
            foreach (var chunkpos in chunkspos) {
                VoxelChunk voxelChunk = GetChunkAt(chunkpos);
                voxelChunk?.Refresh();
            }
        }
        [ContextMenu("Clear")]
        public void Clear() {
            if (activeChunks == null) {
                activeChunks = new List<VoxelChunk>();
                activeChunksDict = null;
                // chunksToPopulate = new List<int>();
            } else {
                for (int i = activeChunks.Count - 1; i >= 0; i--) {
                    if (!activeChunks[i]) continue;
                    VoxelChunk chunk = activeChunks[i];
                    chunk.Clear();
                    chunkPool?.Release(chunk.gameObject);
                }
                activeChunks.Clear();
                activeChunksDict?.Clear();
                // chunksToPopulate.Clear();
            }
            if (chunkPool == null) {
                Initialize();
            }
        }
        [ContextMenu("Clear pool")]
        void ClearPool() {
            chunkPool?.Dispose();
            for (int i = transform.childCount - 1; i >= 0; i--) {
                GameObject cgo = transform.GetChild(i).gameObject;
                if (Application.isPlaying) {
                    Destroy(cgo);
                } else {
                    DestroyImmediate(cgo);
                }
            }
        }

        [System.Serializable]
        public struct WorldSaveData {
            public float voxelSize;
            public int defaultChunkResolution;
            public TypeChoice<Mesher.VoxelMesher> mesher;
            public bool enableCollision;
            public bool useBoxColliders;
            public bool renderNullSides;
            [SerializeReference]
            public VoxelMaterialSetSO materialSet;
            public ChunkSaveData[] chunks;
        }
        [System.Serializable]
        public struct ChunkSaveData {
            public Vector3Int chunkPos;
            public Voxel[] voxels;
        }
        public WorldSaveData GetWorldSaveData() {
            WorldSaveData worldSaveData = new WorldSaveData();
            worldSaveData.voxelSize = voxelSize;
            worldSaveData.defaultChunkResolution = chunkResolution;
            worldSaveData.mesher = mesher;
            worldSaveData.enableCollision = enableCollision;
            worldSaveData.useBoxColliders = useBoxColliders;
            worldSaveData.renderNullSides = renderNullSides;
            worldSaveData.materialSet = materialSet;
            for (int i = 0; i < activeChunks.Count; i++) {
                VoxelChunk voxelChunk = activeChunks[i];
                worldSaveData.chunks[i] = GetChunkSaveData(voxelChunk);
            }
            return worldSaveData;
        }
        ChunkSaveData GetChunkSaveData(VoxelChunk voxelChunk) {
            return new ChunkSaveData() {
                chunkPos = Vector3Int.FloorToInt(voxelChunk.chunkPos),
                voxels = voxelChunk.voxels,
            };
        }
        public void LoadWorldSaveData(WorldSaveData worldSaveData) {
            Debug.Log("Loading WorldSaveData...", this);
            Clear();
            voxelSize = worldSaveData.voxelSize;
            chunkResolution = worldSaveData.defaultChunkResolution;
            mesher = worldSaveData.mesher;
            enableCollision = worldSaveData.enableCollision;
            useBoxColliders = worldSaveData.useBoxColliders;
            renderNullSides = worldSaveData.renderNullSides;
            materialSet = worldSaveData.materialSet;
            ChunkSaveData[] chunks = worldSaveData.chunks;
            Clear();
            LoadChunksFromData(chunks);
        }

        public void LoadChunksFromData(ChunkSaveData[] chunks, bool overwrite = false) {
            Debug.Log($"Loading chunks from data {chunks.Length}");
            if (chunkPool == null) {
                Initialize();
            }
            List<Vector3Int> chunksToRefresh = new List<Vector3Int>();
            for (int i = 0; i < chunks.Length; i++) {
                ChunkSaveData chunkSaveData = chunks[i];
                if (HasChunkActiveAt(chunkSaveData.chunkPos)) {
                    if (overwrite) {
                        Debug.LogWarning($"Overwriting chunk {chunkSaveData.chunkPos}");
                        GetChunkAt(chunkSaveData.chunkPos).OverrideVoxels(chunkSaveData.voxels);
                        chunksToRefresh.Add(chunkSaveData.chunkPos);
                    }
                    // todo additive mode
                    continue;
                }
                VoxelChunk voxelChunk = CreateChunk(chunkSaveData.chunkPos, false);
                voxelChunk.OverrideVoxels(chunkSaveData.voxels);
                // voxelChunk.InitVoxels();// gets called in override
                chunksToRefresh.Add(chunkSaveData.chunkPos);
            }
            RefreshChunks(chunksToRefresh.ToArray());
        }
        public void LoadFullImportVoxelData(Importer.FullVoxelImportData fullVoxelImportData) {
            if (fullVoxelImportData == null) return;
            voxelSize = fullVoxelImportData.voxelSize;
            chunkResolution = fullVoxelImportData.chunkResolution;
            Debug.Log($"Loading from import data models:{fullVoxelImportData.models.Length} vsize:{voxelSize} cres:{chunkResolution}");
            LoadModelsImportData(fullVoxelImportData.models);
        }
        void LoadModelsImportData(Importer.VoxelModelImportData[] rooms) {
            if (rooms != null) {
                Clear();
                //? load each model seperately (in seperate worlds?)
                // if (rooms.Length > 0) {// todo fix loader multiple model offsets
                // var room = rooms[0];
                foreach (var model in rooms) {
                    // if (model.trMatrix != Matrix4x4.zero) {

                    // }
                    LoadChunksImportData(model.chunks, model.position);
                }
            }
        }
        void LoadModelImportData(Importer.VoxelModelImportData modelImportData) {

        }
        void LoadChunksImportData(Importer.ChunkImportData[] chunks, Vector3Int vOffset) {
            // Debug.Log($"Loading from import chunks:{chunks.Length}");
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                for (int i = 0; i < chunks.Length; i++) {
                    Importer.ChunkImportData chunk = chunks[i];
                    chunk.chunkPos += vOffset / chunkResolution;
                    // Debug.Log($"Loading from import chunk:{chunk.chunkPos}");
                    if (HasChunkActiveAt(chunk.chunkPos)) continue;
                    CreateChunk(chunk);
                    RefreshAll();
                }
            } else
#endif
            {
                StartCoroutine(LoadChunksImportDataCo(chunks, vOffset));
            }
        }
        IEnumerator LoadChunksImportDataCo(Importer.ChunkImportData[] chunks, Vector3Int vOffset) {
            for (int i = 0; i < chunks.Length; i++) {
                Importer.ChunkImportData chunk = chunks[i];
                chunk.chunkPos += vOffset / chunkResolution;
                Debug.Log($"Loading from import chunk:{chunk.chunkPos}");
                if (HasChunkActiveAt(chunk.chunkPos)) continue;
                VoxelChunk voxelChunk = CreateChunk(chunk);
                yield return null;
                voxelChunk.Refresh();
                yield return null;
            }
            // RefreshAll();
        }

        public void LoadChunksEmpty(params Vector3Int[] chunkposs) {
            foreach (var cp in chunkposs) {
                if (HasChunkActiveAt(cp))
                    continue;
                CreateChunk(cp);
            }
            // todo refresh neighbor chunks?
        }
        public void LoadChunksAndGen(params Vector3Int[] chunkposs) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                foreach (var cp in chunkposs) {
                    if (HasChunkActiveAt(cp))
                        continue;
                    CreateChunk(cp);
                    // todo restore if have data or generate
                    generateChunkEvent?.Invoke(cp);
                }
            } else
#endif
            {
                StartCoroutine(LoadChunksCo(chunkposs));
            }
        }
        IEnumerator LoadChunksCo(Vector3Int[] chunkposs) {
            foreach (var cp in chunkposs) {
                if (HasChunkActiveAt(cp))
                    continue;
                CreateChunk(cp);
                // todo restore if have data or generate
                generateChunkEvent?.Invoke(cp);
                // GetChunkAt(cp).Refresh();
                // HashSet<int> hashSet = testHash.ToHashSet();
                // hashSet.Add(ChunkPosToHash(cp));
                // testHash = hashSet.ToList();
                yield return null;
            }
            // todo refresh neighbor chunks?
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

        VoxelChunk CreateChunk(Importer.ChunkImportData chunkImportData) {
            // Debug.Log($"Creating import chunk {chunkImportData.chunkPos}");
            VoxelChunk voxelChunk = CreateChunk(chunkImportData.chunkPos, false);
            voxelChunk.PopulateVoxels(chunkImportData.voxels.Select(rv => (VoxelMaterialId)(rv.materialId)).ToArray(), null);
            if (loadImportPopulateEvent != null) {
                for (int i = 0; i < voxelChunk.voxels.Length; i++) {
                    Voxel v = voxelChunk.voxels[i];
                    loadImportPopulateEvent?.Invoke(chunkImportData.voxels[i], v);
                }
            }
            voxelChunk.InitVoxels();
            return voxelChunk;
        }
        VoxelChunk CreateChunk(Vector3Int chunkPos, bool populate = true) {
            // Debug.Log($"Creating chunk {chunkPos}");
            GameObject chunkgo = chunkPool.Get();
            chunkgo.transform.localPosition = (Vector3)chunkPos * chunkSize;
            chunkgo.name = $"chunk {chunkPos.x},{chunkPos.y},{chunkPos.z}{(populate ? "" : " (loaded)")}";
            VoxelChunk chunk = chunkgo.GetComponent<VoxelChunk>();
            chunk.Initialize(this, chunkPos, chunkResolution, populate);
            activeChunks.Add(chunk);
            activeChunksDict.Add(chunkPos, chunk);
            return chunk;
        }


        public bool HasChunkActiveAt(Vector3Int cpos) {
            FixDict();
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
        public Vector3Int ChunkPosWithBlock(Vector3Int blockpos) {
            return ChunkPosWithBlock(blockpos, chunkResolution);
        }
        public static Vector3Int ChunkPosWithBlock(Vector3Int blockpos, int chunkResolution) {
            // if (blockpos.x < 0) blockpos.x -= chunkResolution-1;
            // if (blockpos.y < 0) blockpos.y -= chunkResolution-1;
            // if (blockpos.z < 0) blockpos.z -= chunkResolution-1;
            Vector3Int cpos = Vector3Int.FloorToInt(((Vector3)blockpos) / chunkResolution);
            // handles negatives
            // int cshift = chunkResolution / 4;
            // Vector3Int cpos = new Vector3Int(
            //     blockpos.x >> cshift,
            //     blockpos.y >> cshift,
            //     blockpos.z >> cshift
            //     );
            return cpos;
        }
        public VoxelChunk GetChunkWithBlock(Vector3Int blockpos) {
            return GetChunkAt(ChunkPosWithBlock(blockpos));
        }
        Vector3Int BlockPosToLocalVoxelPos(Vector3Int blockpos, Vector3Int chunkpos) {
            return BlockPosToLocalVoxelPos(blockpos, chunkpos, chunkResolution);
        }
        public static Vector3Int BlockPosToLocalVoxelPos(Vector3Int blockpos, Vector3Int chunkpos, int chunkResolution) {
            return blockpos - chunkpos * chunkResolution;
        }
        public Voxel GetVoxelAt(Vector3Int blockpos) {
            VoxelChunk chunk = GetChunkWithBlock(blockpos);
            if (chunk) {
                Voxel voxel = chunk.GetLocalVoxelAt(BlockPosToLocalVoxelPos(blockpos, chunk.chunkPos));
                if (voxel == null) {
                    Debug.LogWarning($"Error getting block type cp{chunk.chunkPos} bp{blockpos} vp{BlockPosToLocalVoxelPos(blockpos, chunk.chunkPos)} v{voxel}");
                    return null;
                }
                return voxel;
            } else {
                return null;
            }
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
        public Vector3Int WorldposToBlockpos(Vector3 wpos) {
            wpos = transform.InverseTransformPoint(wpos);
            // wpos -= Vector3.one * voxelSize / 2f;
            Vector3Int bpos = Vector3Int.RoundToInt(wpos / voxelSize);
            return bpos;
        }
        public Vector3 BlockposToWorldPos(Vector3Int bpos) {
            return transform.TransformPoint(bpos) * voxelSize;
        }

        public static int ChunkPosToHash(Vector3Int chunkpos) {
            // using random primes
            chunkpos += Vector3Int.one * 1097;
            int hashCode = chunkpos.x;
            hashCode = hashCode * 5237 + chunkpos.y;
            hashCode = hashCode * 6043 + chunkpos.z;
            return hashCode;
        }
    }
}