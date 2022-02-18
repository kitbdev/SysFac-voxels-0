using System.Linq;
using Kutil;
using UnityEngine;

// just refers to a blocktype
[System.Serializable]
public struct BlockTypeRef {

    // todo the reason this was a string is to maintain refs between changes to the blocktypelist
    // [CustomDropDown(nameof(choices), noElementsText: "No Types! check BlockManager")]
    // public string idname = "";

    [CustomDropDown(nameof(choices), nameof(selChoiceName), noElementsText: "No Types! check BlockManager")]
    public int blockid;

    public string selChoiceName => BlockManager.Instance?.GetBlockTypeAtIndex(blockid).idname;
    public string[] choices => BlockManager.Instance?.blockTypes.Select((b) => b.idname).ToArray() ?? null;

    // public BlockTypeRef() { }
    // // public BlockTypeRef(string idname) {
    //     // idname = BlockManager.Instance.GetBlockTypeAtIndex(id).idname;
    //     // this.idname = idname;
    //     // todo cant call here
    //     // this.blockid = BlockManager.Instance?.GetBlockType(idname).id ?? -1;
    // // }
    // public BlockTypeRef(int blockid) {
    //     // idname = BlockManager.Instance.GetBlockTypeAtIndex(id).idname;
    //     this.blockid = blockid;
    // }
    public BlockTypeRef(BlockTypeRef other) {
        this.blockid = other.blockid;
    }
    public BlockTypeRef SetBlockName(string idname){
        this.blockid = BlockManager.Instance?.GetBlockType(idname)?.id ?? -1;
        return this;
    }
    public void SetBlockId(int blockid) {
        this.blockid = blockid;
    }

    public bool IsValid() {
        return blockid >= 0;
    }

    public BlockType GetBlockType() {
        return BlockManager.Instance?.GetBlockTypeAtIndex(blockid);
        // return BlockManager.Instance.GetBlockType(idname);
    }
    public override string ToString() {
        return $"BlockType {blockid} {GetBlockType()?.displayName ?? ""}";
    }
}