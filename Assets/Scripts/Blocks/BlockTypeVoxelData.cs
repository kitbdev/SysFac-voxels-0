using UnityEngine;
using VoxelSystem;

[System.Serializable]
public struct BlockTypeVoxelData : VoxelData {

    public BlockTypeRef blockTypeRef;

    public void CopyValuesFrom(VoxelData from) {
        if (from is BlockTypeVoxelData vd) {
            blockTypeRef = new BlockTypeRef(vd.blockTypeRef);
        }
    }
    public void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        // var defVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
        // Debug.Log($"BlockTypeVoxelData Initialize voxel as '{blockTypeRef}' +{voxel.ToStringFull()}");
        // voxel.voxelMaterialId
        BlockManager.UpdateBlockType(voxel, blockTypeRef);
        // note: block type is now based on 
    }
    /*
    
    */
    // public void SetBlockType(BlockTypeRef newBlockType) {
    //     var oldType = blockTypeRef;
    //     // blockTypeRef = new BlockTypeRef(newBlockType);// its a class ref, so make new
    //     blockTypeRef = newBlockType;
    //     BlockManager.UpdateBlockType(defVoxelData.voxel, blockTypeRef, oldType);
    // }

    public override string ToString() {
        return $"{base.ToString()}:{blockTypeRef}";
    }

}