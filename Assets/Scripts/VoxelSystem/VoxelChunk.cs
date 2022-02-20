using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;
using UnityEngine.Pool;

namespace VoxelSystem {
    /// <summary>
    /// handles a single chunk of voxels
    /// </summary>
    public class VoxelChunk : MonoBehaviour {

        [SerializeField, ReadOnly] VoxelWorld _world;
        [SerializeField, ReadOnly] Vector3Int _chunkPos;
        int _resolution;
        [SerializeField, ReadOnly] Voxel[] _voxels;

        private VoxelRenderer visuals;

        public bool showDebug = false;

        public Vector3Int chunkPos { get => _chunkPos; private set => _chunkPos = value; }
        public int resolution { get => _resolution; private set => _resolution = value; }
        public Voxel[] voxels { get => _voxels; private set => _voxels = value; }
        public VoxelWorld world { get => _world; private set => _world = value; }

        public int floorArea => resolution * resolution;
        public int volume => resolution * resolution * resolution;

        public void Initialize(VoxelWorld world, Vector3Int chunkPos, int resolution, bool populate = true) {
            this.world = world;
            this.chunkPos = chunkPos;
            this.resolution = resolution;
            visuals = GetComponent<VoxelRenderer>();
            if (!visuals) {
                visuals = gameObject.AddComponent<VoxelRenderer>();
            }
            visuals.Initialize(this);

            if (populate) {
                PopulateVoxels();
                InitVoxels();
            }
            if (world.enableCollision) { // todo: only colliders on some chunks
                AddColliders();
            }
        }
        public void OverrideVoxels(Voxel[] voxels) {
            this.voxels = voxels;
            //? initialize
            InitVoxels();
        }

        internal void PopulateVoxels() {
            PopulateVoxels(world.materialSet.GetDefaultId());
        }

        public void PopulateVoxels(VoxelMaterialId voxelMaterialId) {
            voxels = new Voxel[volume];
            TypeChoice<VoxelData>[] neededData = world.neededData;
            // Debug.Log($"populating voxels. needs {neededData.Count} {neededData.Aggregate("", (s, tcvd) => s + tcvd.selectedType + ",")}");
            for (int i = 0; i < volume; i++) {
                // y,z,x
                Vector3Int position = GetLocalPos(i);
                Voxel voxel = Voxel.CreateVoxel(voxelMaterialId, neededData);
                voxels[i] = voxel;
            }
        }
        public void PopulateVoxels(VoxelMaterialId[] voxelMaterialIds, TypeChoice<VoxelData>[] neededData = null) {
            if (voxelMaterialIds.Length != volume) return;
            voxels = new Voxel[volume];
            neededData ??= world.neededData;
            // Debug.Log($"populating voxels. needs {neededData.Count} {neededData.Aggregate("", (s, tcvd) => s + tcvd.selectedType + ",")}");
            for (int i = 0; i < volume; i++) {
                // y,z,x
                Vector3Int position = GetLocalPos(i);
                // todo instead of making the voxel create all of its datas, fill a pool of the needed types here and give to the voxel
                // ObjectPool<VoxelData> voxelDataPool = new UnityEngine.Pool.ObjectPool<VoxelData>();
                // voxelDataPool.Get()
                
                Voxel voxel = Voxel.CreateVoxel(voxelMaterialIds[i], neededData);
                voxels[i] = voxel;
            }
        }
        public void InitVoxels() {
            for (int i = 0; i < voxels.Length; i++) {
                // y,z,x
                Vector3Int position = GetLocalPos(i);
                voxels[i].Initialize(this, position);
            }
        }
        public bool IsPopulated() => voxels != null && voxels.Length > 0;

        private void OnEnable() {
            if (voxels != null) {
                for (int i = 0; i < voxels.Length; i++) {
                    // y,z,x
                    Vector3Int position = GetLocalPos(i);
                    voxels[i]?.OnDeserialized(this, position);
                }
            }
        }
        // public void RepopulateVoxels(){
        //     Clear();
        //     PopulateVoxels();
        // }
        public void Clear() {
            voxels = null;
        }

        [ContextMenu("ResetValue")]
        public void ResetValues() {
            for (int i = 0; i < volume; i++) {
                voxels[i].ResetToDefaults();
            }
            // Refresh(true);
        }
        public void Refresh(bool andNeighbors = false) {
            visuals.UpdateMesh();
            UpdateColliders();
            if (andNeighbors) {
                // updates the 7 neighbors behind, below, and left (otherwise recursion?)
                for (int i = 1; i < Voxel.cubePositions.Length; i++) {
                    Vector3Int dir = -Voxel.cubePositions[i];
                    VoxelChunk neighbor = world.GetNeighbor(this, dir);
                    if (neighbor) {
                        neighbor.Refresh(false);
                    }
                }
            }
        }
        public void LocalRefresh(Vector3Int pos, int size) {
            visuals.UpdateMeshAt(pos);
        }

        private void UpdateColliders() {
            RemoveBoxColliders();
            if (world.enableCollision) {
                if (world.useBoxColliders) {
                    AddBoxColliders();
                }
            }
        }
        // private void LocalUpdateColliders(Vector3Int pos) {
        //     RemoveBoxColliders();
        //     if (world.enableCollision) {
        //         if (world.useBoxColliders) {
        //             AddBoxColliders();
        //         }
        //     }
        // }
        public void AddColliders() {
            // todo? cache this useBoxColliders
            // if (!world.enableCollision) return;
            if (world.useBoxColliders) {
                RemoveMeshCollider();
                AddBoxColliders();
            } else {
                RemoveBoxColliders();
                if (!gameObject.TryGetComponent<MeshCollider>(out _)) {
                    gameObject.AddComponent<MeshCollider>();
                }
            }
        }
        public void RemoveColliders() {
            RemoveBoxColliders();
            RemoveMeshCollider();
        }
        private void RemoveMeshCollider() {
            if (gameObject.TryGetComponent<MeshCollider>(out var mcol)) {
                if (Application.isPlaying) {
                    Destroy(mcol);
                } else {
                    DestroyImmediate(mcol);
                }
            }
        }
        private void AddBoxColliders() {
            var collgo = new GameObject($"chunk {chunkPos} col");
            collgo.transform.parent = transform;
            collgo.transform.localPosition = Vector3.zero;
            List<Bounds> surfaceVoxels = new List<Bounds>();
            for (int i = 0; i < volume; i++) {
                Vector3Int vpos = GetLocalPos(i);
                Voxel voxel = GetLocalVoxelAt(i);
                var vmat = voxel.GetVoxelMaterial<BasicMaterial>(world.materialSet);
                if (vmat.isInvisible) {// todo? seperate collider data?
                    continue;
                }
                bool hidden = IsVoxelHidden(vpos);
                if (!hidden) {
                    surfaceVoxels.Add(new Bounds(((Vector3)vpos) * world.voxelSize, Vector3.one * world.voxelSize));
                }
            }
            foreach (var survox in surfaceVoxels) {
                BoxCollider boxCollider = collgo.AddComponent<BoxCollider>();
                boxCollider.center = survox.center;
                boxCollider.size = survox.size;
            }
        }
        private void RemoveBoxColliders() {
            if (transform.childCount > 0) {
                if (Application.isPlaying) {
                    Destroy(transform.GetChild(0).gameObject);
                } else {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
        }


        // public void SetVoxelMaterial(int index, VoxelMaterialId voxelMaterialId) {
        //     voxels[index].SetVoxelMaterialId(world.materialSet, voxelMaterialId);
        // }
        // public void SetVoxelDataTake<T>(int index, T data) where T : VoxelData {
        //     voxels[index].SetVoxelDataFor<T>(data);
        // }
        // /// <summary>
        // /// Sets the voxel data for that datatype. Copys values
        // /// </summary>
        // public void SetVoxelData<T>(int index, T data) where T : VoxelData {
        //     voxels[index].GetVoxelDataFor<T>().CopyValuesFrom(data);
        // }
        public void SetOrAddVoxelDatas<T>(T[] data) where T : VoxelData {
            if (data.Length != volume) {
                Debug.LogWarning($"Error in Chunk SetData size {volume} vs {data.Length}", this);
                return;
            }
            // voxels[volume - 1].SetOrAddVoxelDataFor<T>(data[volume - 1]);
            // Debug.Log($"added volume-1 {voxels[volume - 1]}");
            // todo jobs?
            for (int i = 0; i < volume; i++) {
                voxels[i].SetOrAddVoxelDataFor<T>(data[i], true, true);
            }
        }
        public void SetVoxelDatas<T>(T[] data) where T : VoxelData {
            if (data.Length != volume) {
                Debug.LogWarning($"Error in Chunk SetData size {volume} vs {data.Length}", this);
                return;
            }
            // Debug.Log($"preadded 0 {voxels[0]}");
            // voxels[0].SetOrAddVoxelDataFor<T>(data[0]);
            // Debug.Log($"added 0 {voxels[0]}");
            for (int i = 0; i < volume; i++) {
                voxels[i].SetOrAddVoxelDataFor<T>(data[i], true, false);
            }
        }
        public void SetVoxelMaterials(VoxelMaterialId voxelMaterialId) {
            VoxelMaterialSetSO materialSet = world.materialSet;
            for (int i = 0; i < volume; i++) {
                voxels[i].SetVoxelMaterialId(voxelMaterialId);
            }
        }
        public void SetVoxelMaterials(VoxelMaterialId[] data) {
            if (data.Length != volume) {
                Debug.LogWarning($"Error in Chunk SetData size {volume} vs {data.Length}", this);
                return;
            }
            VoxelMaterialSetSO materialSet = world.materialSet;
            for (int i = 0; i < volume; i++) {
                // if (voxels[i] == null) Debug.LogWarning($"voxel {i} is null!");
                voxels[i].SetVoxelMaterialId(data[i]);
            }
        }

        /// <summary>
        /// Includes neighbors in other chunks
        /// </summary>
        /// <param name="pos">voxel position</param>
        /// <returns></returns>
        public Voxel GetVoxelN(Vector3Int localpos) {
            Voxel voxel = GetLocalVoxelAt(localpos);
            if (voxel != null) {
                return voxel;
            }
            Vector3Int cdir = new Vector3Int(
                localpos.x >= resolution ? 1 : (localpos.x < 0 ? -1 : 0),
                localpos.y >= resolution ? 1 : (localpos.y < 0 ? -1 : 0),
                localpos.z >= resolution ? 1 : (localpos.z < 0 ? -1 : 0)
            );
            localpos -= cdir * resolution;
            Voxel copy = world.GetChunkAt(chunkPos + cdir)?.GetVoxelN(localpos);
            return copy;
        }

        /// <summary>
        /// True if voxel has nontransparent voxels on all sides
        /// </summary>
        /// <param name="vpos"></param>
        /// <returns></returns>
        public bool IsVoxelHidden(Vector3Int vpos) {
            bool hidden = true;
            foreach (Vector3Int dir in Voxel.unitDirs) {
                Voxel voxel = GetVoxelN(vpos + dir);
                var vmat = voxel?.GetVoxelMaterial<BasicMaterial>(world.materialSet);
                if (voxel != null && vmat.isTransparent) {
                    hidden = false;
                    break;
                }
            }
            return hidden;
        }

        /// <summary>
        /// local position of voxel at index i
        /// </summary>
        /// <param name="i"></param>
        /// <returns>v3int position</returns>
        public Vector3Int GetLocalPos(int i) {
            Vector3Int pos = Vector3Int.zero;
            pos.x = i % resolution;
            pos.z = (i / resolution) % resolution;
            pos.y = i / floorArea;
            return pos;
        }
        public int IndexAt(Vector3Int localpos) => IndexAt(localpos.x, localpos.y, localpos.z);
        public int IndexAt(int x, int y, int z) {
            return IndexAt(x, y, z, resolution);
        }
        public static int IndexAt(Vector3Int localpos, int resolution) => IndexAt(localpos.x, localpos.y, localpos.z, resolution);
        public static int IndexAt(int x, int y, int z, int resolution) {
            if (x < 0 || x >= resolution || y < 0 || y >= resolution || z < 0 || z >= resolution)
                return -1;
            return x + y * resolution * resolution + z * resolution;
        }
        public Voxel GetLocalVoxelAt(Vector3Int localpos) => GetLocalVoxelAt(IndexAt(localpos));
        public Voxel GetLocalVoxelAt(int index) {
            if (index >= 0 && index < voxels.Length)
                return voxels[index];
            else
                return null;
        }
        private void OnDrawGizmos() {
            if (!showDebug || !world) {
                return;
            }
            Gizmos.color = new Color(0, 0.8f, 0.5f, 0.1f);
            Gizmos.DrawCube(world.ChunkposToWorldposCenter(chunkPos), world.chunkSize * Vector3.one);
            Gizmos.color = Color.white;
        }
        /*
        [System.Serializable]
    class RawVoxelRun {
        public int count;
        public RawVoxel data;
    }
    RawVoxelRun[] ToRawVoxelRuns() {
        // todo test these
        // should be a more compact format
        List<RawVoxelRun> runList = new List<RawVoxelRun>();
        for (int i = 0; i < voxels.Length; i++) {
            var rv = voxels[i].ToRawVoxel();
            if (runList.Count == 0 || runList[runList.Count - 1].data != rv) {
                runList.Add(new RawVoxelRun { count = 1, data = rv });
            } else {
                runList[runList.Count - 1].count++;
            }
        }
        return runList.ToArray();
    }
    void SetDataFromRawVoxelRuns(RawVoxelRun[] runs) {
        if (runs == null) {
            return;
        }
        int run = -1;
        int cnt = 0;
        for (int i = 0; i < volume; i++) {
            if (cnt <= 0) {
                run++;
                if (run > runs.Length || runs[run] == null) {
                    Debug.LogError("Invalid RawVoxelRun!");
                    return;
                }
                cnt = runs[run].count;
            }
            voxels[i].value = runs[run].data.value;
            voxels[i].colorId = runs[run].data.colorId;
            cnt--;
        }

    }
    */
    }
}