using UnityEngine;
using VoxelSystem;

[System.Serializable]
public struct BlockTypeVoxelData : VoxelData {

    public BlockTypeRef blockTypeRef;
    // [SerializeReference, SerializeField]
    [System.NonSerialized]
    DefaultVoxelData defVoxelData;// todo dont serialize? 

    public void CopyValuesFrom(VoxelData from) {
        if (from is BlockTypeVoxelData vd) {
            blockTypeRef = new BlockTypeRef(vd.blockTypeRef);
        }
    }
    public void OnDeserialized(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        defVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
    }
    public void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
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
        if (blockTypeRef == null || !blockTypeRef.IsValid()) {
            return;
        }
        BlockType blockType = blockTypeRef.GetBlockType();
        defVoxelData.voxel.SetVoxelMaterialId(blockType.voxelMaterialId);
        // note chunk refresh should be done elsewhere
        // ! all block types do not have the same data

        // todo modify voxel
        // todo use blockmanager? to know what kind of vdatas are needed to add
        // defVoxelData.voxel.AddVoxelDataFor()
    }
    public override string ToString() {
        return $"{base.ToString()}:{blockTypeRef}";
    }
}