using UnityEngine;

[System.Serializable]
public class BlockType {
    public int id;
    public string idname;
    public string displayName;
    public Voxel.VoxelShape shape;
    public int textureId;
    [Min(0)]
    public int maxStack;
    public int itemid;

    public override string ToString() {
        return $"{id}-{displayName}";
    }
}