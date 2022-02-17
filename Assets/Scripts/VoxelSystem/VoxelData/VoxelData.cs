using UnityEngine;

namespace VoxelSystem {
    public abstract class VoxelData {
        public virtual void CopyValuesFrom(VoxelData from) { }
        public virtual void OnSetup() { }
        public virtual void OnMaterialChange(Voxel voxel, VoxelMaterial voxelMaterial) { }
        // public virtual void OnTick(){}
    }
    [System.Serializable]
    public class DensityVoxelData : VoxelData {
        public float density;
        public override void CopyValuesFrom(VoxelData from) {
            if (from is DensityVoxelData vd) {
                this.density = vd.density;
            } else {
                base.CopyValuesFrom(from);
            }
        }
    }
    [System.Serializable]
    public class MeshCacheVoxelData : VoxelData {
        [System.NonSerialized]
        public bool isTransparent;
        [System.NonSerialized]
        public Vector2Int textureCoord;
        // neighbors?

        public override void CopyValuesFrom(VoxelData from) {
            if (from is MeshCacheVoxelData vd) {
                this.isTransparent = vd.isTransparent;
                this.textureCoord = vd.textureCoord;
            } else {
                base.CopyValuesFrom(from);
            }
        }
        public override void OnMaterialChange(Voxel voxel, VoxelMaterial voxelMaterial) {
            if (voxelMaterial is BasicMaterial bmat){
                isTransparent = bmat.isTransparent;
                textureCoord = bmat.textureCoord;
            }
            // if (voxel.TryGetVoxelDataType<MeshCacheVoxelData>(out var mcvd)) {
            // }
        }
    }
}