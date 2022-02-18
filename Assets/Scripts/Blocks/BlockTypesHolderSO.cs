using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;
using VoxelSystem;

[CreateAssetMenu(fileName = "BlockTypesHolder", menuName = "SysFac/BlockTypesHolder", order = 0)]
public class BlockTypesHolderSO : ScriptableObject {

    [SerializeField] VoxelMaterialSetSO voxelMaterialSet;
    public BlockType[] blockTypes;

    private void OnValidate() {
        for (int i = 0; i < blockTypes.Length; i++) {
            blockTypes[i].id = i;
            // if (blockTypes[i].displayName == "") {
            //     blockTypes[i].displayName = blockTypes[i].idname;
            // }
        }
        blockTypeToAdd.vmat.objvalue.OnValidate(voxelMaterialSet);
    }

    public BlockType GetBlockTypeAtIndex(int index) {
        if (index >= 0 && index < blockTypes.Length) {
            return blockTypes[index];
        } else {
            Debug.LogWarning($"Block index {index} does not exist!");
            return null;
        }
    }
    // public BlockType GetBlockType(string idname) {
    //     if (blockTypeDict.ContainsKey(idname)) {
    //         return blockTypeDict[idname];
    //     } else {
    //         Debug.LogWarning($"Block type {idname} does not exist!");
    //         return null;
    //     }
    // }
    public void AddBlockTypes(params BlockType[] newBlockTypes) {
        List<BlockType> btlist = blockTypes.ToList();
        btlist.AddRange(newBlockTypes);
        blockTypes = btlist.ToArray();
        // blockTypeDict = newBlockTypes.ToDictionary((b) => b.idname);
    }

    // todo move to editor window
    [System.Serializable]
    public class BlockTypeConsParam {
        public string displayName = "";
        public TypeSelector<VoxelMaterial> vmat = new TypeSelector<VoxelMaterial>(typeof(BasicMaterial));
    }
    [Header("Block editor")]
    [ContextMenuItem("add type", nameof(AddNewBlockType))]
    [ContextMenuItem("clear all", nameof(ClearAllBlockTypesAndMats))]
    public BlockTypeConsParam blockTypeToAdd;

    void AddNewBlockType() {
        CreateBlockTypeAndMat(blockTypeToAdd);
        blockTypeToAdd = new BlockTypeConsParam();
    }

    public void ClearAllBlockTypesAndMats() {
        if (voxelMaterialSet) {
            voxelMaterialSet.ClearVoxelMats();
        }
        blockTypes = new BlockType[0];
    }
    public void CreateBlockTypeAndMat(BlockTypeConsParam data) {
        if (voxelMaterialSet == null || data == null || data.displayName == "") {
            return;
        }
        BlockType blockType = new BlockType();
        blockType.displayName = data.displayName;
        blockType.idname = ToIdName(data.displayName);
        if (data.vmat.objvalue is BasicMaterial bvm) {
            if (bvm.texname == "") {
                bvm.texname = blockType.idname;
            }
        }
        VoxelMaterialId voxelMaterialId = voxelMaterialSet.AddVoxelMaterial(data.vmat.objvalue);
        blockType.voxelMaterialId = voxelMaterialId;
        AddBlockTypes(blockType);
    }

    static string ToIdName(string displayName) {
        return displayName.Replace(" ", "").ToLower();
    }
}