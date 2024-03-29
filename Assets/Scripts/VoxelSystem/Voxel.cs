using UnityEngine;
using Kutil;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System;

namespace VoxelSystem {
    /// <summary>
    /// holds data for a single voxel
    /// </summary>
    [System.Serializable]
    public class Voxel {

        [SerializeField] VoxelMaterialId _voxelMaterialId;
        [SerializeReference]// todo make sure works with structs
        [SerializeField]
        VoxelData[] _voxelDatas;
        TypedObjectPool<VoxelData> voxelDataPool;

        public VoxelMaterialId voxelMaterialId { get => _voxelMaterialId; protected set => _voxelMaterialId = value; }
        public VoxelData[] voxelDatas { get => _voxelDatas; protected set => _voxelDatas = value; }

        protected Voxel() { }
        public Voxel(VoxelMaterialId voxelMaterialId, VoxelData[] voxelDatas) {
            this.voxelMaterialId = voxelMaterialId;
            this.voxelDatas = voxelDatas;
        }
        public Voxel(Voxel voxel) {
            voxelMaterialId = voxel.voxelMaterialId;
            voxelDatas = voxel.voxelDatas.ToArray();// theyre structs, so this makes a copy
        }
        // public static Voxel CreateVoxel(VoxelWorld world) {
        //     VoxelMaterialId voxelMaterialId = world.materialSet.GetDefaultId();
        //     return CreateVoxel(voxelMaterialId, world.neededData);
        // }
        public static Voxel CreateVoxel(VoxelMaterialId voxelMaterialId, VoxelData[] voxelDatas) {
            return new Voxel(voxelMaterialId, voxelDatas);
        }
        // public static Voxel CreateVoxel(VoxelMaterialId voxelMaterialId, TypeChoice<VoxelData>[] neededData) {
        //     // List<VoxelData> voxelDataList = (neededData.Select(nvd => nvd.CreateInstance())).ToList();
        //     // voxelDataList.Sort(VoxelDataSortComparer());
        //     // Voxel voxel = new Voxel(voxelMaterialId, voxelDataList.ToArray());
        //     // neededData.Select(nvd => nvd.CreateInstance()).ToArray()
        //     VoxelData[] voxelDatas = new VoxelData[neededData.Length];
        //     for (int i = 0; i < voxelDatas.Length; i++) {
        //         voxelDatas[i] = neededData[i].CreateInstance();
        //     }
        //     Voxel voxel = new Voxel(voxelMaterialId, voxelDatas);
        //     // Debug.Log($"Adding {neededData.Count} vdatas {voxel} {voxelDataList.Aggregate("", (s, vd) => s + vd + ",")}");
        //     // voxelDataList.Clear();
        //     return voxel;
        // }

        public void Initialize(VoxelChunk chunk, Vector3Int localVoxelPos) {
            foreach (var voxelData in voxelDatas) {
                voxelData.Initialize(this, chunk, localVoxelPos);
            }
            // NotifyVoxelDataMaterialUpdate(GetVoxelMaterial(chunk.world.materialSet));
        }
        public void OnDeserialized(VoxelChunk chunk, Vector3Int localVoxelPos) {
            foreach (var voxelData in voxelDatas) {
                voxelData.OnDeserialized(this, chunk, localVoxelPos);
            }
        }

        public bool TryGetVoxelDataType<T>(out T voxelData)
            where T : VoxelData {
            if (HasVoxelDataFor<T>()) {
                voxelData = GetVoxelDataFor<T>();
                return true;
            }
            voxelData = default;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasVoxelDataFor<T>() where T : VoxelData {
            return HasVoxelDataFor(typeof(T));
            // return voxelDatas.Any(vd => vd.GetType() == typeof(T));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasVoxelDataFor(System.Type type) {
            for (int i = 0; i < voxelDatas.Length; i++) {
                if (voxelDatas[i].GetType() == type) {
                    return true;
                }
            }
            return false;
            // return voxelDatas.Any(vd => vd.GetType() == type);
        }
        public T GetVoxelDataFor<T>() where T : VoxelData {
            System.Type t = typeof(T);
            for (int i = 0; i < voxelDatas.Length; i++) {
                if (voxelDatas[i].GetType() == t) {
                    return (T)voxelDatas[i];
                }
            }
            return default;
            // return (T)voxelDatas.FirstOrDefault(vd => vd.GetType() == typeof(T));
        }
        // public TypeSelector<VoxelData> GetVoxelDataFor(TypeChoice<VoxelData> type) {
        //     return new TypeSelector<VoxelData>(voxelDatas.FirstOrDefault(vd => vd.GetType() == type.selectedType));
        // }
        /// <summary>
        /// Set the voxel data without checking
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RawSetVoxelDataFor<T>(T data) where T : VoxelData {
            // int v = voxelDatas.ToList().FindIndex(vd => vd.GetType() == typeof(T));
            System.Type t = typeof(T);
            // todo dict for Type, int voxeldata index? eh there shouldnt be too many voxeldatas
            for (int i = 0; i < voxelDatas.Length; i++) {
                if (voxelDatas[i].GetType() == t) {
                    voxelDatas[i] = data;
                    return;
                }
            }
        }
        // void RawSetVoxelDataFor(TypeSelector<VoxelData> data) {
        //     int v = voxelDatas.ToList().FindIndex(vd => vd.GetType() == data.type.selectedType);
        //     voxelDatas[v] = data.objvalue;
        // }
        // public 
        // void SetVoxelDataMany(params TypeSelector<VoxelData>[] datas) {
        //     DefaultVoxelData defaultVoxelData = GetVoxelDataFor<DefaultVoxelData>();
        //     foreach (var data in datas) {
        //         if (HasVoxelDataFor(data.type.selectedType)) {
        //             RawSetVoxelDataFor(data);
        //             data.objvalue.Initialize(this, defaultVoxelData.chunk, defaultVoxelData.localVoxelPos);
        //         }
        //     }
        // }
        // public 
        // void AddVoxelDataMany(bool orSet = false, params TypeSelector<VoxelData>[] datas) {
        //     // hopefully this should work
        //     DefaultVoxelData defaultVoxelData = GetVoxelDataFor<DefaultVoxelData>();
        //     List<VoxelData> newVoxelDatas = voxelDatas.ToList();
        //     foreach (var data in datas) {
        //         if (!HasVoxelDataFor(data.type.selectedType)) {
        //             newVoxelDatas.Add(data.objvalue);
        //         } else {
        //             RawSetVoxelDataFor(data);
        //         }
        //         data.objvalue.Initialize(this, defaultVoxelData.chunk, defaultVoxelData.localVoxelPos);
        //     }
        //     voxelDatas = newVoxelDatas.ToArray();
        // }
        public void SetOrAddVoxelDataFor(VoxelData[] data, bool set = true, bool add = true, bool reinit = false) {
            foreach (var d in data) {
                SetOrAddVoxelDataFor(d, set, add, false);
            }
            if (reinit) {
                foreach (var d in data) {
                    DefaultVoxelData defaultVoxelData = GetVoxelDataFor<DefaultVoxelData>();
                    d.Initialize(this, defaultVoxelData.chunk, defaultVoxelData.localVoxelPos);
                }
            }
        }
        public void SetOrAddVoxelDataFor(VoxelData data, bool set = true, bool add = true, bool reinit = false) {
            if (HasVoxelDataFor(data.GetType())) {
                if (set) {
                    RawSetVoxelDataFor(data);
                } else {
                    // ignoring
                    // Debug.LogWarning($"Voxel {this} already has {data.GetType()} data, cannot add {data}");
                    return;
                }
            } else {
                if (add) {
                    // List<VoxelData> newVoxelDatas = voxelDatas.ToList();
                    // newVoxelDatas.Add(data);
                    // voxelDatas = newVoxelDatas.ToArray();
                    // voxelDataList.Sort(VoxelDataSortComparer());// tolist is slow
                    voxelDatas = voxelDatas.Append(data).ToArray();// todo sort?
                }
            }
            if (reinit) {
                DefaultVoxelData defaultVoxelData = GetVoxelDataFor<DefaultVoxelData>();
                data.Initialize(this, defaultVoxelData.chunk, defaultVoxelData.localVoxelPos);
            }
        }
        // public void SetOrAddVoxelDataFor(TypeSelector<VoxelData> data, bool set = true, bool add = true) {
        //     // if (data.type.CanBeAssignedTo(typeof(DefaultVoxelData))) {
        //     //     Debug.LogError($"AddVoxelDataFor Voxel {this} cannot add {data.type.selectedType} data here");
        //     //     return;
        //     // }
        //     if (HasVoxelDataFor(data.type.selectedType)) {
        //         if (set) {
        //             RawSetVoxelDataFor(data);
        //         } else {
        //             Debug.LogWarning($"Voxel {this} alrady has {data.type.selectedType} data, cannot add {data}");
        //             return;
        //         }
        //     } else {
        //         if (add) {
        //             List<VoxelData> newVoxelDatas = voxelDatas.ToList();
        //             newVoxelDatas.Add(data.objvalue);
        //             voxelDatas = newVoxelDatas.ToArray();
        //         }
        //     }
        //     DefaultVoxelData defaultVoxelData = GetVoxelDataFor<DefaultVoxelData>();
        //     data.objvalue.Initialize(this, defaultVoxelData.chunk, defaultVoxelData.localVoxelPos);
        // }
        public void RemoveVoxelDataFor<T>() where T : VoxelData {
            // GetVoxelDataFor<T>().OnRemove(this); //todo would this even work? its a struct
            voxelDatas = voxelDatas.ToList().Where(vd => vd.GetType() != typeof(T)).ToArray();
        }
        // public void RemoveVoxelDataFor(System.Type type) {
        //     GetVoxelDataFor(type).objvalue.OnRemove(this);
        //     voxelDatas = voxelDatas.ToList().Where(vd => vd.GetType() != type).ToArray();
        // }
        public bool TryRemoveVoxelDataFor<T>() where T : VoxelData {
            if (HasVoxelDataFor<T>()) {
                RemoveVoxelDataFor<T>();
                return true;
            }
            return false;
        }
        // public bool TryRemoveVoxelDataFor(System.Type type) {
        //     if (HasVoxelDataFor(type)) {
        //         RemoveVoxelDataFor(type);
        //         return true;
        //     }
        //     return false;
        // }

        public VoxelMaterial GetVoxelMaterial(VoxelMaterialSetSO voxmatset) {
            return voxmatset.GetVoxelMaterial(voxelMaterialId);
        }
        public T GetVoxelMaterial<T>(VoxelMaterialSetSO voxmatset) where T : VoxelMaterial {
            return voxmatset.GetVoxelMaterial<T>(voxelMaterialId);
        }
        // public void SetVoxelMaterialId(VoxelMaterialSetSO voxmatset, VoxelMaterialId newVoxelMaterialId) {
        public void SetVoxelMaterialId(VoxelMaterialId newVoxelMaterialId) {
            voxelMaterialId = newVoxelMaterialId;
            // note notifies all data in case they need to update 
            // todo really only meshdatacache updates, have that caching somewhere else? 
            // VoxelMaterial voxelMaterial = GetVoxelMaterial(voxmatset);
            // NotifyVoxelDataMaterialUpdate(voxelMaterial);
        }

        // private void NotifyVoxelDataMaterialUpdate(VoxelMaterial voxelMaterial) {
        //     foreach (var vdata in voxelDatas) {
        //         vdata.OnMaterialChange(this, voxelMaterial);
        //     }
        // }

        public void ResetToDefaults() {
            voxelMaterialId = 0;
            // voxelDatas
            // todo what are the default voxel datas?
        }
        public void CopyValuesFrom(Voxel voxel) {
            voxelMaterialId = voxel.voxelMaterialId;
            voxelDatas = voxel.voxelDatas.ToArray();// theyre structs, so this makes a copy
                                                    // List<VoxelData> datas = new List<VoxelData>();
                                                    // foreach (var voxelData in voxel.voxelDatas) {
                                                    // needs to be typechoice to instantiate child stuff
                                                    // VoxelData nvoxl = ((TypeChoice<VoxelData>)voxelData.GetType()).CreateInstance();
                                                    // nvoxl.CopyValuesFrom(voxelData);
                                                    // datas.Add(nvoxl);
                                                    // }
        }

        public string ToStringFull() {
            string str = $"Voxel mat:{voxelMaterialId} datas({voxelDatas.Length}):[";
            string preDelim = voxelDatas.Length > 2 ? "\n" : " ";
            foreach (var vd in voxelDatas) {
                str += $"{preDelim}{vd.ToString()},";
            }
            str += "]";
            return str;
        }
        public override string ToString() {
            string str = $"Voxel mat:{voxelMaterialId} datas({voxelDatas.Length})";
            return str;
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