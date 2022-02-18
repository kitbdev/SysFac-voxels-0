using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VoxelSystem {
    public abstract class VoxelData {
        /// <summary>lower is earlier</summary>
        public virtual int sortOrder => 0;
        public virtual void CopyValuesFrom(VoxelData from) { }
        public virtual void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) { }
        public virtual void OnMaterialChange(Voxel voxel, VoxelMaterial voxelMaterial) { }
        public virtual void OnRemove(Voxel voxel) { }
        // public virtual void OnTick(){}
        public override string ToString() {
            return $"{this.GetType().Name} ";
        }
    }
    [System.Serializable]
    public class DensityVoxelData : VoxelData {
        public float density;
        // override int sortOrder => 0;
        public override void CopyValuesFrom(VoxelData from) {
            if (from is DensityVoxelData vd) {
                this.density = vd.density;
            } else {
                base.CopyValuesFrom(from);
            }
        }
    }
    [System.Serializable]
    public class DefaultVoxelData : VoxelData {
        public override int sortOrder => -1000;
        [SerializeReference]
        public Voxel voxel;
        public Vector3Int localVoxelPos;
        [SerializeReference]
        public VoxelChunk chunk;

        public override void CopyValuesFrom(VoxelData from) {
            if (from is DefaultVoxelData vd) {
                // voxel.CopyValuesFrom(vd.voxel);
                voxel = vd.voxel;
                localVoxelPos = vd.localVoxelPos;
                chunk = vd.chunk;
            } else {
                base.CopyValuesFrom(from);
            }
        }
        public override void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
            base.Initialize(voxel, chunk, localVoxelPos);
            // Debug.Log("DefaultVoxelData init");
            // voxel.CopyValuesFrom(vd.voxel);// todo copy?
            this.voxel = voxel;// it was never set!
            this.chunk = chunk;
            this.localVoxelPos = localVoxelPos;
        }
        public override string ToString() {
            return $"{base.ToString()} v:{voxel != null} lvp:{localVoxelPos} c:{chunk != null}";
        }
    }
    [System.Serializable]
    public class ExtraVoxelData : VoxelData {
        public Voxel[] neighbors;
        public override void CopyValuesFrom(VoxelData from) {
            if (from is ExtraVoxelData vd) {

            } else {
                base.CopyValuesFrom(from);
            }
        }
    }
    [System.Serializable]
    public class UndoRedoVoxelData : VoxelData {
        public Queue<Voxel> history;
        public override void CopyValuesFrom(VoxelData from) {
            if (from is UndoRedoVoxelData vd) {
                history = vd.history;
            } else {
                base.CopyValuesFrom(from);
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