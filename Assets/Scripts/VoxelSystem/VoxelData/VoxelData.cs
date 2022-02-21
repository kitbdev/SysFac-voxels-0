using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VoxelSystem {
    public interface VoxelData {
        /// <summary>lower is earlier</summary>
        // int sortOrder => 0;
        void CopyValuesFrom(VoxelData from);
        void OnDeserialized(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) { }
        void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) { }
        // void OnMaterialChange(Voxel voxel, VoxelMaterial voxelMaterial) { }
        void OnRemove(Voxel voxel) { }
        // public virtual void OnTick(){}
        // public override string ToString() {
        //     return $"{this.GetType().Name} ";
        // }
    }
    [System.Serializable]
    public struct DensityVoxelData : VoxelData {
        public float density;
        // int sortOrder => 0;
        public void CopyValuesFrom(VoxelData from) {
            if (from is DensityVoxelData vd) {
                this.density = vd.density;
            }
        }
    }
    [System.Serializable]
    public struct DefaultVoxelData : VoxelData {
        // [SerializeReference]
        // public Voxel voxel;
        public Vector3Int localVoxelPos;
        public Vector3Int blockPos;
        [SerializeReference]
        public VoxelChunk chunk;

        // public int sortOrder => -1000;

        public void CopyValuesFrom(VoxelData from) {
            if (from is DefaultVoxelData vd) {
                // voxel.CopyValuesFrom(vd.voxel);
                // voxel = vd.voxel;
                localVoxelPos = vd.localVoxelPos;
                blockPos = vd.blockPos;
                chunk = vd.chunk;
            }
        }
        // public void OnDeserialized(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        //     Debug.Log($"DefaultVoxelData des {this.voxel != null}");
        //     // voxel.CopyValuesFrom(vd.voxel);// todo copy?
        //     // this.voxel = voxel;// it was never set!
        //     // this.chunk = chunk;
        //     // this.localVoxelPos = localVoxelPos;
        // }
        public void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
            // Debug.Log("DefaultVoxelData init");
            // voxel.CopyValuesFrom(vd.voxel);// todo copy?
            // this.voxel = voxel;// it was never set!
            this.chunk = chunk;
            this.blockPos = chunk.chunkPos * chunk.resolution + localVoxelPos;
            this.localVoxelPos = localVoxelPos;
        }
        public override string ToString() {
            return $"{{{this.GetType().Name} bp:{blockPos} lvp:{localVoxelPos} c:{chunk != null}}}";
        }
    }
    [System.Serializable]
    public struct ExtraVoxelData : VoxelData {
        public Voxel[] neighbors;
        public void CopyValuesFrom(VoxelData from) {
            if (from is ExtraVoxelData vd) {

            }
        }
    }
    [System.Serializable]
    public struct UndoRedoVoxelData : VoxelData {
        public Queue<Voxel> history;
        public void CopyValuesFrom(VoxelData from) {
            if (from is UndoRedoVoxelData vd) {
                history = vd.history;
            }
        }
    }
    // [System.Serializable]
    // public class MeshCacheVoxelData : VoxelData {
    //     [System.NonSerialized]
    //     public bool isTransparent;
    //     [System.NonSerialized]
    //     public Vector2Int textureCoord;
    //     // neighbors?

    //     public override void CopyValuesFrom(VoxelData from) {
    //         if (from is MeshCacheVoxelData vd) {
    //             this.isTransparent = vd.isTransparent;
    //             this.textureCoord = vd.textureCoord;
    //         } else {
    //             base.CopyValuesFrom(from);
    //         }
    //     }
    //     public override void OnMaterialChange(Voxel voxel, VoxelMaterial voxelMaterial) {
    //         if (voxelMaterial is BasicMaterial bmat){
    //             isTransparent = bmat.isTransparent;
    //             textureCoord = bmat.textureCoord;
    //         }
    //         // if (voxel.TryGetVoxelDataType<MeshCacheVoxelData>(out var mcvd)) {
    //         // }
    //     }
    // }
}