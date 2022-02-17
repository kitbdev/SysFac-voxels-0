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
        public virtual void Initialize(VoxelMaterialSetSO voxelMaterialSet) { }
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
    public class TextureOverrides {

        [System.NonSerialized]
        public string[] choices = null;

        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameUp = null;
        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameDown = null;
        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameFront = null;
        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameBack = null;
        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameRight = null;
        [Kutil.CustomDropDown(nameof(choices), includeNullChoice: true)]
        public string texnameLeft = null;
        [Kutil.ReadOnly] public Vector2Int texcoordUp;
        [Kutil.ReadOnly] public Vector2Int texcoordDown;
        [Kutil.ReadOnly] public Vector2Int texcoordFront;
        [Kutil.ReadOnly] public Vector2Int texcoordBack;
        [Kutil.ReadOnly] public Vector2Int texcoordRight;
        [Kutil.ReadOnly] public Vector2Int texcoordLeft;
        public string[] textureNames => new string[6]{
            texnameRight,// in order of Voxel.dirs
            texnameFront,
            texnameUp,
            texnameLeft,
            texnameBack,
            texnameDown,
        };
        public Vector2Int[] textureCoords => new Vector2Int[6]{
            texcoordRight,// in order of Voxel.dirs
            texcoordFront,
            texcoordUp,
            texcoordLeft,
            texcoordBack,
            texcoordDown,
        };
        public void Initialize(VoxelMaterialSetSO voxelMaterialSet, Vector2Int defTexCoord) {
            // Debug.Log("defTexCoord " + defTexCoord + " :" + texnameUp);
            choices = voxelMaterialSet.textureAtlas.allTextureNames;
            texcoordUp = GetTexCoord(texnameUp, voxelMaterialSet, defTexCoord);
            texcoordDown = GetTexCoord(texnameDown, voxelMaterialSet, defTexCoord);
            texcoordFront = GetTexCoord(texnameFront, voxelMaterialSet, defTexCoord);
            texcoordBack = GetTexCoord(texnameBack, voxelMaterialSet, defTexCoord);
            texcoordRight = GetTexCoord(texnameRight, voxelMaterialSet, defTexCoord);
            texcoordLeft = GetTexCoord(texnameLeft, voxelMaterialSet, defTexCoord);

            Vector2Int GetTexCoord(string texname, VoxelMaterialSetSO voxelMaterialSet, Vector2Int defTexCoord) {
                return (texname != null && texname != "") ? 
                    voxelMaterialSet.GetTexCoordForName(texname) : defTexCoord;
            }
            // dont want to set names to base name because that will update the SO
        }

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
    public class BasicMaterial : VoxelMaterial {
        public bool isInvisible;// dont mesh
        public bool isTransparent;
        [Kutil.CustomDropDown(nameof(textureOverrides) + "." + nameof(TextureOverrides.choices), includeNullChoice: true)]
        public string texname;
        [Kutil.ReadOnly]
        public Vector2Int textureCoord;
        public TextureOverrides textureOverrides;
        // public Color tint = Color.white;
        public override void Initialize(VoxelMaterialSetSO voxelMaterialSet) {
            base.Initialize(voxelMaterialSet);
            textureCoord = voxelMaterialSet.GetTexCoordForName(texname);
            textureOverrides.Initialize(voxelMaterialSet, textureCoord);
        }
    }
    [System.Serializable]
    public class AnimatedMaterial : BasicMaterial {
        public float animDuration;
        public Vector2Int[] frameCoords;// ? auto set using texture?
    }
}