using UnityEngine;

namespace VoxelSystem {
    public abstract class VoxelData {
    }
    [System.Serializable]
    public class DensityVoxelData : VoxelData {
        public float density;
    }
    [System.Serializable]
    public class MeshCacheVoxelData : VoxelData {
        [System.NonSerialized]
        public bool isTransparent;
        [System.NonSerialized]
        public Vector2Int textureCoord;
        // neighbors?
    }
}