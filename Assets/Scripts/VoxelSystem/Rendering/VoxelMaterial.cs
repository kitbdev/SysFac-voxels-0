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
        // public Rect textureRect;
        public int materialIndex;
        public bool hasCollision;
        public Material material;//todo? dont serialize this
        // public TextureAtlasPacker
        // public Texture2D
        public virtual void OnValidate(VoxelMaterialSetSO voxelMaterialSet) { }
        public virtual void Initialize(VoxelMaterialSetSO voxelMaterialSet, int index=0) { }
    }
    [System.Serializable]
    public class CuboidMaterial : VoxelMaterial {
        public CuboidData[] cuboids;
    }
    [System.Serializable]
    public struct CuboidData {
        public Vector3 center;
        public Vector3 extents;
        public Quaternion rotation;
        public Rect[] textureRects;
    }
    [System.Serializable]
    public class BlockShape {
        public Vector3 blockFrom;
        public Vector3 blockTo;
        public Vector2 uvFrom;
        public Vector2 uvTo;
    }
    [System.Serializable]
    public struct SimpleCuboidData {
        public Vector3Int center;
        public Vector3Int size;
        public BlockRotation rotation;
        public BlockRotation rotation2;
        [System.Serializable]
        public class BlockRotation {
            public Vector3 origin = Vector3.zero;
            public Axis axis;
            public float angle;
            public enum Axis {
                X, Y, Z
            }
        }
    }
    [System.Serializable]
    public class AnimatedMaterial : TexturedMaterial {
        public float animDuration;
        public Vector2Int[] frameCoords;// ? auto set using texture?

        public override void OnValidate(VoxelMaterialSetSO voxelMaterialSet) {
            base.OnValidate(voxelMaterialSet);
            // textureCoord = voxelMaterialSet.GetTexCoordForName(texname);
            // textureOverrides.Initialize(voxelMaterialSet, textureCoord);
        }
        public override void Initialize(VoxelMaterialSetSO voxelMaterialSet, int index=0) {
            base.Initialize(voxelMaterialSet, index);
            // textureCoord = voxelMaterialSet.GetTexCoordForName(texname);
            // textureOverrides.Initialize(voxelMaterialSet, textureCoord);
        }
    }
}