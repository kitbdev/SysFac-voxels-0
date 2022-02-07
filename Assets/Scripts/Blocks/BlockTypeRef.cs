using Kutil;
using UnityEngine;

// just refers to a blocktype
[System.Serializable]
public class BlockTypeRef {
    public string idname = "air";

    public BlockTypeRef(string idname) {
        this.idname = idname;
    }
    public BlockTypeRef(int id) {
        idname = BlockManager.Instance.GetBlockTypeAtIndex(id).idname;
    }

    public BlockType ToBlockType() {
        return BlockManager.Instance.GetBlockType(idname);
    }
}