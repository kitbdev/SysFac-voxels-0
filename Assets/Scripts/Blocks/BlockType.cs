using Kutil;
using UnityEngine;
using VoxelSystem;

[System.Serializable]
public class BlockType {

    public int id;
    public string idname;
    public string displayName;
    public VoxelMaterialId voxelMaterialId;
    public TypeChoice<VoxelData>[] customDatas;
    [Min(0)]
    public int maxStack;
    public int itemid;

    public override string ToString() {
        return $"Blocktype{id}-'{displayName}'";
    }
}
