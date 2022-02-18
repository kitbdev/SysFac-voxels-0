using UnityEngine;
using VoxelSystem;

[System.Serializable]
public struct BlockTypeVoxelData : VoxelData {

    public BlockTypeRef blockTypeRef;
    // [System.NonSerialized]
    // [SerializeField]
    public DefaultVoxelData defVoxelData;// cant not serialize, oh well

    public void CopyValuesFrom(VoxelData from) {
        if (from is BlockTypeVoxelData vd) {
            blockTypeRef = new BlockTypeRef(vd.blockTypeRef);
        }
    }
    public void OnDeserialized(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        // Debug.Log($"btvd OnDeserialized {defVoxelData.voxel != null}");
        // why?
        defVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
    }
    public void Initialize(Voxel voxel, VoxelChunk chunk, Vector3Int localVoxelPos) {
        // defVoxelData = new DefaultVoxelData();
        // defVoxelData.CopyValuesFrom(voxel.GetVoxelDataFor<DefaultVoxelData>());
        defVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
        // voxel.voxelMaterialId
        // Debug.Log($"BlockTypeVoxelData Initialize voxel as '{blockTypeRef}' +{this}");
        UpdateBlockType(defVoxelData.voxel, blockTypeRef);
    }
    public void SetBlockType(BlockTypeRef newBlockType) {
        var oldType = blockTypeRef;
        // blockTypeRef = new BlockTypeRef(newBlockType);// its a class ref, so make new
        blockTypeRef = newBlockType;
        UpdateBlockType(defVoxelData.voxel, blockTypeRef, oldType);
    }

    // todo move to block manager
    public static void SetBlockType(Voxel voxel, BlockTypeRef newBlockType) {
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        var oldType = blockTypeVoxelData.blockTypeRef;
        // Debug.Log($"Setting |{voxel.ToStringFull()}| to |{newBlockType}|"); 
        blockTypeVoxelData.blockTypeRef = newBlockType;
        voxel.SetOrAddVoxelDataFor<BlockTypeVoxelData>(blockTypeVoxelData, true, false, false);
        UpdateBlockType(voxel, newBlockType, oldType);
        // Debug.Log($"Setdone |{voxel.ToStringFull()}| to |{newBlockType}|");
    }
    static void UpdateBlockType(Voxel voxel, BlockTypeRef newBlockType, BlockTypeRef? oldBlockType = null) {
        if (!newBlockType.IsValid()) {
            Debug.LogWarning("invalid UpdateBlockType");
            return;
        }
        BlockType blockType = newBlockType.GetBlockType();
        voxel.SetVoxelMaterialId(blockType.voxelMaterialId);
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