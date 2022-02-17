using System.Linq;
using Kutil;
using UnityEngine;

// just refers to a blocktype
[System.Serializable]
public class BlockTypeRef {
    [CustomDropDown(nameof(choices), missingText:"No Types! check BlockManager")]
    public string idname = "air";

    public string[] choices => BlockManager.Instance?.blockTypes.Select((b) => b.idname).ToArray() ?? null;

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