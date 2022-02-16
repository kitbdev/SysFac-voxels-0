using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class BlockManager : Singleton<BlockManager> {
    [System.Serializable]
    class AllBlocks {
        public List<BlockType> blockTypes;
    }

    private const string blocksfilename = "defblocks";

    [SerializeField] TextAsset blocksjson;
    public TextureAtlasPacker blockTextureAtlas;
    public Material defBlockMat;

    [SerializeField]
    List<BlockType> _blockTypes = new List<BlockType>();
    Dictionary<string, BlockType> _blockTypeDict = new Dictionary<string, BlockType>();

    public List<BlockType> blockTypes { get => _blockTypes; private set => _blockTypes = value; }
    public Dictionary<string, BlockType> blockTypeDict { get => _blockTypeDict; private set => _blockTypeDict = value; }

    private void OnValidate() {
        for (int i = 0; i < blockTypes.Count; i++) {
            blockTypes[i].id = i;
            if (blockTypes[i].displayName == "") {
                blockTypes[i].displayName = blockTypes[i].idname;
            }
        }
    }
    private void OnEnable() {
        LoadData();
        UpdateMat();
        // blockTypeDict = blockTypes.ToDictionary((b) => b.idname);
    }

    [ContextMenu("Update mat")]
    private void UpdateMat() {
        defBlockMat.mainTexture = blockTextureAtlas.atlas;
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
    public Vector2Int GetBlockTexCoord(BlockType blockType) {
        var texname = blockType.textureNameOverride == "" ? blockType.idname : blockType.textureNameOverride;
        if (blockType.id == 0 || blockType.textureNameOverride == "none") {
            // air has no texture
            return Vector2Int.zero;
        } else if (blockTextureAtlas.packDict.ContainsKey(texname)) {
            Vector2 coord = blockTextureAtlas.packDict[texname];
            // Debug.Log($"found {blockType} coord {coord}"); 
            return Vector2Int.FloorToInt(coord);
        } else {
            Debug.LogWarning($"Texture for block {blockType}({blockType.textureNameOverride},{blockType.idname}) not found!");
            return Vector2Int.zero;
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
        for (int i = 0; i < blockTypes.Count; i++) {
            blockTypes[i].id = i;
        }
        AllBlocks allBlocks = new AllBlocks { blockTypes = blockTypes };
        // string content = JsonUtility.ToJson(allBlocks);
        // blocksjson = new TextAsset(content);
        // blocksjson.name = "Data/blocks.asset";
        SaveSystem.TrySaveLocal(blocksjson.name, allBlocks, true);
    }

}