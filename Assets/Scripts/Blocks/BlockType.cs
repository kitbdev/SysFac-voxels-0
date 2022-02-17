using Kutil;
using UnityEngine;
using VoxelSystem;

[System.Serializable]
public class BlockType {

    public int id;
    public string idname;
    public string displayName;
    [ShowAsChild(nameof(VoxelMaterialId.id))]
    public VoxelMaterialId voxelMaterialId;
    [Min(0)]
    public int maxStack;
    public int itemid;

    public override string ToString() {
        return $"{id}-{displayName}";
    }
}