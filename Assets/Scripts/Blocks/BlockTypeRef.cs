using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;


#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(BlockTypeRef))]
public class BlockTypeRefDrawer : Kutil.ShowAsChildPropertyDrawer {
    public override string childName => nameof(BlockTypeRef.blockid);
}
#endif
// just refers to a blocktype
[System.Serializable]
public struct BlockTypeRef {

    // todo the reason this was a string is to maintain refs between changes to the blocktypelist
    // [CustomDropDown(nameof(choices), noElementsText: "No Types! check BlockManager")]
    // public string idname = "";

    [CustomDropDown(nameof(choicesData))]
    public int blockid;

    public CustomDropDownData choicesData => CustomDropDownData.Create<int>(choicesint, choices,
        preFormatValueFunc: v => {
            if (v < 0) return "n\\a";
            BlockType b = BlockManager.Instance?.GetBlockTypeAtIndex(v);
            if (b == null) return "missing";
            return $"{b.idname}({b.id})";
        },
        noElementsText: "No Types! check BlockManager");
    public string[] choices {
        get {
            List<string> btypelist = BlockManager.Instance?.blockTypes.Select((b) => $"{b.idname}({b.id})").ToList();
            btypelist?.Insert(0, "n\\a");
            return btypelist?.ToArray() ?? null;
        }
    }

    public IEnumerable<int> choicesint => Enumerable.Range(-1, choices.Length);

    public BlockTypeRef(BlockTypeRef other) {
        this.blockid = other.blockid;
        // blockIdInt = blockid;
    }
    public BlockTypeRef SetBlockName(string idname) {
        this.blockid = BlockManager.Instance?.GetBlockType(idname)?.id ?? -1;
        return this;
    }
    public BlockTypeRef SetBlockId(int blockid) {
        this.blockid = blockid;
        // int maxBlockId = BlockManager.Instance.blockTypes.Count;
        // if (this.blockid >= maxBlockId) {
        //     Debug.LogWarning($"BlockTypeRef {blockid} is larger than {maxBlockId} setting to default 0");
        //     this.blockid = Default.blockid;
        // }
        return this;
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

    public static BlockTypeRef Default => new BlockTypeRef().SetBlockId(0);
    public static BlockTypeRef Invalid => new BlockTypeRef().SetBlockId(-1);
}