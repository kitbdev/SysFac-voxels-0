using UnityEngine;
using Kutil;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;

namespace VoxelSystem {
    /// <summary>
    /// holds data for a single voxel
    /// </summary>
    [System.Serializable]
    public class Voxel {

        [SerializeField] VoxelMaterialId _voxelMaterialId;
        [SerializeReference] [SerializeField] VoxelData[] _voxelDatas;

        public VoxelMaterialId voxelMaterialId { get => _voxelMaterialId; protected set => _voxelMaterialId = value; }
        public VoxelData[] voxelDatas { get => _voxelDatas; protected set => _voxelDatas = value; }

        protected Voxel() { }
        protected Voxel(VoxelMaterialId voxelMaterialId, VoxelData[] voxelDatas) {
            this.voxelMaterialId = voxelMaterialId;
            this.voxelDatas = voxelDatas;
        }
        public static Voxel CreateVoxel(VoxelWorld world) {
            VoxelMaterialId voxelMaterialId = world.materialSet.GetDefaultId();
            return CreateVoxel(voxelMaterialId, world.neededData);
        }
        public static Voxel CreateVoxel(VoxelMaterialId voxelMaterialId, List<TypeChoice<VoxelData>> neededData) {
            List<VoxelData> voxelDataList = new List<VoxelData>();
            // todo test voxeldata still have child data
            // neededData.ForEach((nvd) => {
            //     voxelDataList.Add(nvd.CreateInstance());
            // }); 
            Voxel voxel = new Voxel(voxelMaterialId, voxelDataList.ToArray());
            voxelDataList.Clear();// native array?
            return voxel;
        }
        public void Initialize(VoxelChunk chunk) {
            // NotifyVoxelDataMaterialUpdate(GetVoxelMaterial(chunk.world.materialSet));
        }

        public bool TryGetVoxelDataType<T>(out T voxelData)
            where T : VoxelData {
            if (HasVoxelDataFor<T>()) {
                voxelData = GetVoxelDataFor<T>();
                return true;
            }
            voxelData = null;
            return false;
        }
        public bool HasVoxelDataFor<T>() where T : VoxelData {
            return voxelDatas.Any(vd => vd.GetType() == typeof(T));
        }
        public bool HasVoxelDataFor(System.Type type) {
            return voxelDatas.Any(vd => vd.GetType() == type);
        }
        public T GetVoxelDataFor<T>() where T : VoxelData {
            return (T)voxelDatas.FirstOrDefault(vd => vd.GetType() == typeof(T));
        }
        public void SetVoxelDataFor<T>(T data) where T : VoxelData {
            int v = voxelDatas.ToList().FindIndex(vd => vd.GetType() == typeof(T));
            voxelDatas[v] = data;
        }
        public void SetVoxelDataFor(TypeSelector<VoxelData> data) {
            int v = voxelDatas.ToList().FindIndex(vd => vd.GetType() == data.type.selectedType);
            voxelDatas[v] = data.objvalue;
        }
        public void SetVoxelDataMany(params TypeSelector<VoxelData>[] datas) {
            foreach (var data in datas) {
                if (HasVoxelDataFor(data.type.selectedType)) {
                    SetVoxelDataFor(data);
                }
            }
        }
        public void AddVoxelDataMany(bool orSet = false, params TypeSelector<VoxelData>[] datas) {
            // hopefully this should work
            List<VoxelData> newVoxelDatas = voxelDatas.ToList();
            foreach (var data in datas) {
                if (!HasVoxelDataFor(data.type.selectedType)) {
                    newVoxelDatas.Add(data.objvalue);
                } else {
                    SetVoxelDataFor(data);
                }
            }
            voxelDatas = newVoxelDatas.ToArray();
        }
        public void AddVoxelDataFor<T>(T data, bool orSet = false) where T : VoxelData {
            if (HasVoxelDataFor<T>()) {
                if (orSet) {
                    SetVoxelDataFor<T>(data);
                } else {
                    Debug.LogWarning($"Voxel {this} alrady has {typeof(T).Name} data, cannot add {data}");
                }
            } else {
                List<VoxelData> newVoxelDatas = voxelDatas.ToList();
                newVoxelDatas.Add(data);
                voxelDatas = newVoxelDatas.ToArray();
            }
        }
        public void SetOrAddVoxelDataFor<T>(T data) where T : VoxelData {
            AddVoxelDataFor<T>(data, true);
        }

        public VoxelMaterial GetVoxelMaterial(VoxelMaterialSetSO voxmatset) {
            return voxmatset.GetVoxelMaterial(voxelMaterialId);
        }
        public T GetVoxelMaterial<T>(VoxelMaterialSetSO voxmatset) where T : VoxelMaterial {
            return voxmatset.GetVoxelMaterial<T>(voxelMaterialId);
        }
        public void SetVoxelMaterialId(VoxelMaterialSetSO voxmatset, VoxelMaterialId newVoxelMaterialId) {
            voxelMaterialId = newVoxelMaterialId;
            // note notifies all data in case they need to update 
            // todo really only meshdatacache updates, have that caching somewhere else? 
            // VoxelMaterial voxelMaterial = GetVoxelMaterial(voxmatset);
            // NotifyVoxelDataMaterialUpdate(voxelMaterial);
        }

        private void NotifyVoxelDataMaterialUpdate(VoxelMaterial voxelMaterial) {
            foreach (var vdata in voxelDatas) {
                vdata.OnMaterialChange(this, voxelMaterial);
            }
        }

        public void ResetToDefaults() {
            voxelMaterialId = 0;
            // voxelDatas
            // todo what are the default voxel datas?
        }
        public void CopyValuesFrom(Voxel voxel) {
            voxelMaterialId = voxel.voxelMaterialId;
            List<VoxelData> datas = new List<VoxelData>();
            foreach (var voxelData in voxel.voxelDatas) {
                VoxelData nvoxl = ((TypeChoice<VoxelData>)voxelData.GetType()).CreateInstance();
                nvoxl.CopyValuesFrom(voxelData);
                datas.Add(nvoxl);
            }
            voxelDatas = datas.ToArray();
        }
        public override string ToString() {
            return $"Voxel {voxelMaterialId} datas:{voxelDatas.Length}";
        }

        // static stuff

        public static Vector3Int[] GetUnitNeighbors(Vector3Int pos, bool includeSelf = false) {
            var neighbors = unitDirs.Select((v) => { return v + pos; });
            if (!includeSelf) {
                neighbors = neighbors.Skip(1);
            }
            return neighbors.ToArray();
        }

        public static Vector3Int[] unitDirs = new Vector3Int[6] {
        new Vector3Int(1,0,0),// right
        new Vector3Int(0,0,1),// forward
        new Vector3Int(0,1,0),// up
        new Vector3Int(-1,0,0),// left
        new Vector3Int(0,0,-1),// back
        new Vector3Int(0,-1,0),// down
        };
        public static Vector3Int[] cubePositions = {
        new Vector3Int(0,0,0),//0
        new Vector3Int(1,0,0),//1
        new Vector3Int(1,0,1),//2
        new Vector3Int(0,0,1),//3
        new Vector3Int(0,1,0),//4
        new Vector3Int(1,1,0),//5
        new Vector3Int(1,1,1),//6
        new Vector3Int(0,1,1),//7
        };
        public readonly static Vector3Int[] dirTangents = new Vector3Int[6] {
        new Vector3Int(0,0,1),
        new Vector3Int(-1,0,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(1,0,0),
        };
        public readonly static Vector3[] vOffsets = new Vector3[6] {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        };

    }
    [System.Serializable]
    public class OldVoxel {
        public enum VoxelShape {
            none,
            cube,
            xfull,
            xsmall,
            customcubey,
            customcubexyz,
            custom,
        }

        public int blockId;
        public VoxelShape shape;
        public bool isTransparent;
        public Vector2Int textureCoord;
        // public Color tint;
        // todo lighting data?
        // todo anim data

        // public Voxel() {
        //     ResetToDefaults();
        // }
        // public Voxel(BlockType blockType) {
        //     blockId = blockType.id;
        //     shape = blockType.shape;
        //     isTransparent = blockType.isTransparent;
        //     textureCoord = BlockManager.Instance.GetBlockTexCoord(blockType);
        // }

        public void ResetToDefaults() {
            shape = VoxelShape.cube;
            blockId = 0;
            isTransparent = false;
            textureCoord = Vector2Int.zero;
        }
        // public void CopyValues(Voxel voxel) {
        // shape = voxel.shape;
        // blockId = voxel.blockId;
        // isTransparent = voxel.isTransparent;
        // textureCoord = voxel.textureCoord;
        // }
        public override string ToString() {
            return $"Voxel {shape.ToString()} id:{blockId}";
        }

    }

    public enum VoxelDirection {
        RIGHT = 0,
        FORWARD = 1,
        UP = 2,
        LEFT = 3,
        BACK = 4,
        DOWN = 5,
    }
    // [System.Serializable]
    // public class FatVoxel {
    //     public int index;
    //     public Vector3Int position;
    //     [SerializeField]
    //     public VoxelChunk chunk;

    //     public Voxel.VoxelShape shape;
    //     public int textureId;

    //     public override string ToString() {
    //         return $"Voxel {index} {position} c{chunk.chunkPos}";
    //     }
    // }
}