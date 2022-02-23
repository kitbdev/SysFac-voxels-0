using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;
using VoxelSystem;

/*
todo rework blocks, voxel materials, and adding them
load from file?
editor to add new types
dont store in voxmatset? set it?
? start with editor


idea:
material editor
set coords for uv
alt uv
material to use
etc

and block editor
add block
- actually rather load from file?

*/

[CreateAssetMenu(fileName = "BlockTypesHolder", menuName = "SysFac/BlockTypesHolder", order = 0)]
public class BlockTypesHolderSO : ScriptableObject {

    // to help find blocktypes
    [SerializeField]
    string findType = "";
    [SerializeField]
    int findTypeId;
    [SerializeField]
    BlockTypeRef findTypeRef;
    [SerializeField, ReadOnly]
    BlockType foundType;

    // [SerializeField] VoxelMaterialSetSO voxelMaterialSet;
    public BlockType[] blockTypes;

    private void OnValidate() {
        if (findType != "") {
            foundType = blockTypes.FirstOrDefault(bt => bt.idname == findType);
        } else if (findTypeId > 0) {
            foundType = blockTypes.FirstOrDefault(bt => bt.id == findTypeId);
        } else if (findTypeRef.IsValid()) {
            foundType = blockTypes.FirstOrDefault(bt => bt.id == findTypeRef.blockid);
        }
        //     for (int i = 0; i < blockTypes.Length; i++) {
        //         blockTypes[i].id = i;
        //         // if (blockTypes[i].displayName == "") {
        //         //     blockTypes[i].displayName = blockTypes[i].idname;
        //         // }
        //     }
        //     // blockTypeToAdd.vmat.objvalue.OnValidate(voxelMaterialSet);
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
        for (int i = 0; i < newBlockTypes.Length; i++) {
            BlockType bt = newBlockTypes[i];
            bt.id = btlist.Count + i;
        }
        btlist.AddRange(newBlockTypes);
        blockTypes = btlist.ToArray();
        // blockTypeDict = newBlockTypes.ToDictionary((b) => b.idname);
    }
    public void ClearAllBlockTypes() {
        blockTypes = new BlockType[0];
    }

}