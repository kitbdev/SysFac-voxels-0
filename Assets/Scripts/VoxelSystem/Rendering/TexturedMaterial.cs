using UnityEngine;

namespace VoxelSystem {
    [System.Serializable]
    public class TexturedMaterial : VoxelMaterial {
        // todo change so only needed data is here, must be set from a loader?
        //? multiple atlas management?
        public bool isInvisible;// dont mesh
        public bool isTransparent;
        // [Kutil.CustomDropDown(nameof(textureOverrides) + "." + nameof(TextureOverrides.choices), includeNullChoice: true)]
        // public string texname;
        [Kutil.ReadOnly]
        public Vector2 textureCoord;
        // public TextureOverrides textureOverrides;
        // public Color tint = Color.white;
        public override void OnValidate(VoxelMaterialSetSO voxelMaterialSet) {
            base.OnValidate(voxelMaterialSet);
            // textureCoord = voxelMaterialSet.GetTexCoordForName(texname);
            // textureOverrides.Initialize(voxelMaterialSet, textureCoord);
        }
        public override void Initialize(VoxelMaterialSetSO voxelMaterialSet, int index = 0) {
            base.Initialize(voxelMaterialSet);
            // textureCoord = voxelMaterialSet.GetTexCoordForName(texname);
            // textureOverrides.Initialize(voxelMaterialSet, textureCoord);
            // todo dont assume palette like this
            // textureCoord = voxelMaterialSet.textureScale * new Vector2(index, 0);
            // Debug.Log($"initing texturemat {index} matindex:{materialIndex} tc:{textureCoord}");
        }
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
            // choices = voxelMaterialSet.textureAtlas.allTextureNames;
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
}