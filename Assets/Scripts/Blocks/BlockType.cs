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
[System.Serializable]
public class BlockTypeVoxelData : VoxelData {

    public BlockTypeRef blockTypeRef;
    [SerializeReference, SerializeField]
    DefaultVoxelData defVoxelData;

    public override void CopyValuesFrom(VoxelData from) {
        if (from is BlockTypeVoxelData vd) {
            blockTypeRef = new BlockTypeRef(vd.blockTypeRef);
        } else {
            base.CopyValuesFrom(from);
        }
    }
    public override void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        base.Initialize(voxel, chunk, localVoxelPos);
        defVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
        // voxel.voxelMaterialId
        // Debug.Log($"BlockTypeVoxelData Initialize voxel as '{blockTypeRef}' +{this}");
        UpdateBlockType();
    }
    public void SetBlockType(BlockTypeRef newBlockType) {
        var oldType = blockTypeRef;
        blockTypeRef = new BlockTypeRef(newBlockType);// its a class ref, so make new
        UpdateBlockType(oldType);
    }
    void UpdateBlockType(BlockTypeRef oldType = null) {
        if (blockTypeRef == null || blockTypeRef.blockid == -1) {
            // todo || isInvalid
            return;
        }
        BlockType blockType = blockTypeRef.GetBlockType();
        defVoxelData.voxel.SetVoxelMaterialId(blockType.voxelMaterialId);
        // todo chunk mat update? handle elsewhere
        // defVoxelData.chunk.Refresh();
        // ! all block types do not have the same data
        // todo modify voxel
        // todo use blockmanager? to know what kind of vdatas are needed to add
        // defVoxelData.voxel.AddVoxelDataFor()
    }
    public override string ToString() {
        return $"{base.ToString()}:{blockTypeRef}{(defVoxelData == null ? "(nulldefd)" : "")}";
    }
}