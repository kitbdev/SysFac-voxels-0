using Kutil;
using UnityEngine;

// just refers to a blocktype
[System.Serializable]
public class BlockTypeRef {
    public string idname = "air";

    public BlockTypeRef(string idname) {
        this.idname = idname;
    }

    public BlockType ToBlockType() {
        return BlockManager.Instance.GetBlockType(idname);
    }
}