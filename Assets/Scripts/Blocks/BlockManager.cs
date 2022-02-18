using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;
using VoxelSystem;

[DefaultExecutionOrder(-5)]
public class BlockManager : Singleton<BlockManager> {

    // [System.Serializable]
    // class AllBlockTypes {
    //     public List<BlockType> blockTypes;
    // }

    // private const string blocksfilename = "defblocks";
    // [SerializeField] TextAsset blocksjson;

    [SerializeField] VoxelMaterialSetSO voxelMaterialSet;
    [SerializeField] BlockTypesHolderSO blockTypesHolder;

    [SerializeField]
    // List<BlockType> _blockTypes = new List<BlockType>();
    Dictionary<string, BlockType> _blockTypeDict = new Dictionary<string, BlockType>();

    public List<BlockType> blockTypes { get => blockTypesHolder?.blockTypes?.ToList(); }
    public Dictionary<string, BlockType> blockTypeDict { get => _blockTypeDict; private set => _blockTypeDict = value; }

    private void OnEnable() {
        // LoadData();
        blockTypeDict = blockTypes?.ToDictionary((b) => b.idname);
    }

    public BlockType GetBlockTypeAtIndex(int index) {
        if (index >= 0 && index < blockTypes.Count) {
            return blockTypes[index];
        } else {
            Debug.LogWarning($"Block index {index} does not exist!");
            return null;
        }
    }
    public BlockType GetBlockType(string idname) {
        if (blockTypeDict.ContainsKey(idname)) {
            return blockTypeDict[idname];
        } else {
            Debug.LogWarning($"Block type {idname} does not exist!");
            return null;
        }
    }
    public static void SetBlockType(Voxel voxel, BlockTypeRef newBlockType) {
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        var oldType = blockTypeVoxelData.blockTypeRef;
        // Debug.Log($"Setting |{voxel.ToStringFull()}| to |{newBlockType}|"); 
        blockTypeVoxelData.blockTypeRef = newBlockType;
        voxel.SetOrAddVoxelDataFor<BlockTypeVoxelData>(blockTypeVoxelData, true, false, false);
        UpdateBlockType(voxel, newBlockType, oldType);
        // Debug.Log($"Setdone |{voxel.ToStringFull()}| to |{newBlockType}|");
    }
    public static void UpdateBlockType(Voxel voxel, BlockTypeRef newBlockType, BlockTypeRef? oldBlockType = null) {
        // note chunk refresh should be done elsewhere
        if (!newBlockType.IsValid()) {
            Debug.LogWarning("invalid UpdateBlockType");
            return;
        }
        BlockType blockType = newBlockType.GetBlockType();
        voxel.SetVoxelMaterialId(blockType.voxelMaterialId);
        // Debug.Log($"Set '{voxel}' mat to '{blockType}'");
        // ! all block types do not have the same data

        // todo modify voxel
        // todo use blockmanager? to know what kind of vdatas are needed to add
        // defVoxelData.voxel.AddVoxelDataFor()
    }

    // void AddBlockTypes(params BlockType[] newBlockTypes) {
    //     blockTypesHolder.AddBlockTypes(newBlockTypes);
    //     blockTypeDict = newBlockTypes.ToDictionary((b) => b.idname);
    // }


    // [ContextMenu("reload")]
    // void LoadData() {
    //     AllBlocks blocks = JsonUtility.FromJson<AllBlocks>(blocksjson.text);
    //     blockTypesHolder.blockTypes = blocks.blockTypes.ToArray();
    //     blockTypeDict = blockTypes.ToDictionary((b) => b.idname);
    // }
    // [ContextMenu("save")]
    // void SaveData() {
    //     for (int i = 0; i < blockTypes.Count; i++) {
    //         blockTypes[i].id = i;
    //     }
    //     AllBlocks allBlocks = new AllBlocks { blockTypes = blockTypes };
    //     // string content = JsonUtility.ToJson(allBlocks);
    //     // blocksjson = new TextAsset(content);
    //     // blocksjson.name = "Data/blocks.asset";
    //     SaveSystem.StartSave().AsJSON().InLocalDataPath(blocksjson.name).Content(allBlocks).CanOverwrite().TrySave();
    //     // SaveSystem.TrySaveLocal(blocksjson.name, allBlocks, true);
    // }

}