using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem {
    public class VoxelMaterialManager {
        Dictionary<VoxelMaterialId, VoxelMaterial> vmats;
        Material[] materialPerSubmesh;
        public float textureResolution;
    }
    [System.Serializable]
    public class VoxelMaterial {
        public Rect textureRect;
    }
    [System.Serializable]
    public class CuboidMaterial : VoxelMaterial {

    }
    [System.Serializable]
    public struct CuboidData {
        public Vector3 center;
        public Vector3 extents;
        public Quaternion rotation;
        public Rect[] textureRects;
    }
    [System.Serializable]
    public struct SimpleCuboidData {
        public Vector3Int center;
        public Vector3Int size;
        public float angleRotation;
        public Vector3 axisRotation;
        public Rect[] textureRects;
    }
    [System.Serializable]
    public class BasicMaterial : VoxelMaterial {

    }
    // public class Material : VoxelMaterial {

    // }
    [System.Serializable]
    public struct VoxelMaterialId {
        public int id;
        public static implicit operator int(VoxelMaterialId vmid) => vmid.id;
        public static implicit operator VoxelMaterialId(int nid) => new VoxelMaterialId() { id = nid };
    }
}