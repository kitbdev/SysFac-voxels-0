using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;

public class BlockManager : Singleton<BlockManager> {
    [System.Serializable]
    class AllBlocks {
        public List<BlockType> blockTypes;
    }

    private const string blocksfilename = "defblocks";

    [SerializeField] TextAsset blocksjson;

    [SerializeField]
    List<BlockType> _blockTypes = new List<BlockType>();
    Dictionary<string, BlockType> _blockTypeDict = new Dictionary<string, BlockType>();

    List<BlockType> blockTypes { get => _blockTypes; set => _blockTypes = value; }
    Dictionary<string, BlockType> blockTypeDict { get => _blockTypeDict; set => _blockTypeDict = value; }

    private void OnEnable() {
        LoadData();
        // blockTypeDict = blockTypes.ToDictionary((b) => b.idname);
    }

    public BlockType GetBlockType(string id) {
        if (blockTypeDict.ContainsKey(id)) {
            return blockTypeDict[id];
        } else {
            Debug.LogWarning($"Block type {id} does not exist!");
            return null;
        }
    }

    [ContextMenu("reload")]
    void LoadData() {
        AllBlocks blocks = JsonUtility.FromJson<AllBlocks>(blocksjson.text);
        blockTypes = blocks.blockTypes;
        blockTypeDict = blockTypes.ToDictionary((b) => b.idname);
    }
    [ContextMenu("save")]
    void SaveData() {
        AllBlocks allBlocks = new AllBlocks { blockTypes = blockTypes };
        // string content = JsonUtility.ToJson(allBlocks);
        // blocksjson = new TextAsset(content);
        // blocksjson.name = "Data/blocks.asset";
        SaveSystem.TrySaveLocal(blocksjson.name, allBlocks, true);
    }

}