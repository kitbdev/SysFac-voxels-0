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
        public Material material;
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
        public string textureNameFront;
        public string textureNameBack;
        public string textureNameLeft;
        public string textureNameRight;
        public string textureNameUp;
        public string textureNameDown;
        public Vector3 blockFrom;
        public Vector3 blockTo;
        public Vector2 uvFrom;
        public Vector2 uvTo;
        [System.Serializable]
        public class BlockRotation {
            public Vector3 origin;
            public enum Axis {
                X, Y, Z
            }
            public Axis axis;
            public float angle;
        }
        public BlockRotation rotation;
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
        public bool isInvisible;// dont mesh
        public bool isTransparent;
        public string texname;
        public Vector2Int textureCoord;
        public Color tint = Color.white;
    }
    [System.Serializable]
    public class AnimatedMaterial : BasicMaterial {
        public float animDuration;
        public Vector2Int[] frameCoords;// ? auto set using texture?
    }
}