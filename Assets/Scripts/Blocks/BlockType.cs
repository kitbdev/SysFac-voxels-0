using Kutil;
using UnityEngine;

[System.Serializable]
public class BlockType {
    public int id;
    public string idname;
    public string displayName;
    public string textureNameOverride;
    public VoxelSystem.Voxel.VoxelShape shape;
    public bool isTransparent = false;
    [Min(0)]
    public int maxStack;
    public int itemid;

    public override string ToString() {
        return $"{id}-{displayName}";
    }
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