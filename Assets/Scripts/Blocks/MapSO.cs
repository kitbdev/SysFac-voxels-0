using UnityEngine;
using VoxelSystem.Importer;
using System.Collections.Generic;
using VoxelSystem;
using Kutil;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

// [CustomEditor(typeof(MapSO))]
// public class MapSOEditor : Editor {
//     // public override void OnInspectorGUI() {
//     //     base.OnInspectorGUI();

//     // }
// }
#endif

[System.Serializable]
public class MapData {
    // todo compress? its a lot of voxels in a lot of chunks
    public Vector3Int playerSpawn;
    public int chunkResolution;
    public Vector3[] otherStuff;
    public SerializableDictionary<Vector3Int, VoxelWorld.ChunkSaveData> chunks;
}
[CreateAssetMenu(fileName = "MapData", menuName = "SysFac/Map", order = 0)]
public class MapSO : ScriptableObject {

    public enum SpecialBlockType {
        NONE,
        UNKNOWN,
        MAP_BORDER,
        PLAYER_SPAWN,
        LOOT_SPAWN,
        ENEMY_SPAWN,
    }
    [System.Serializable]
    public struct SpecialBlocksMap {
        public int matid;
        public SpecialBlockType specialType;
        public BlockTypeRef newblockType;
    }

    // [System.Serializable]
    // public struct BlockLoadConverter {
    //     public int importMatId;
    //     // public int importMatIdEnd;
    //     public SpecialBlocks specialType;
    //     public BlockTypeRef blockType;
    // }
    // [System.Serializable]
    // public class AllBlockTypeLoadData {
    //     public BlockLoadConverter[] blockTypeConverter;
    // }
    // Dictionary<int, BlockLoadConverter> blockTypeLoadDict;


    [HideInInspector]
    public MapData mapData;
    [SerializeReference]
    public ImportedVoxelData importedVoxelData;
    [ContextMenuItem("Reset", nameof(ResetNeededData))]
    public TypeChoice<VoxelData>[] neededVoxelDatas = new TypeChoice<VoxelData>[] { new TypeChoice<VoxelData>(typeof(DefaultVoxelData)) };

    public BlockTypeRef unkownBlockDefault;
    [SerializeField]
    public SpecialBlocksMap[] specialBlocksMap;
    // public AllBlockTypeLoadData allBlocksConverter;

    void ResetNeededData() {
        neededVoxelDatas = new TypeChoice<VoxelData>[] { new TypeChoice<VoxelData>(typeof(DefaultVoxelData)) };
    }

    [ContextMenu("Clear map data")]
    public void ClearMapData() {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "clear map");
#endif
        mapData = null;
        Debug.Log($"map data cleared");
    }

    [ContextMenu("Preprocess")]
    public void ProcessImportData() {
        Debug.Log($"prepocessing map...");
        // blockTypeLoadDict = allBlocksConverter.blockTypeConverter
        //             .ToDictionary((bid => bid.importMatId));
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "preprocess map");
#endif
        var mp = PreProcessMap(importedVoxelData.fullVoxelImportData, neededVoxelDatas.ToHashSet().ToArray());
        // todo? make sure chunk resolutions are the same
        if (mp != null) {
            mapData = mp;
            Debug.Log($"Finished processing import data {importedVoxelData.name} to map");
        } else {
            Debug.LogError($"Failed to process import data {importedVoxelData.name} to map");
        }
    }
    private MapData PreProcessMap(FullVoxelImportData fullImportData, TypeChoice<VoxelData>[] neededVoxelDatas) {
        if (fullImportData == null) return null;
        // find player spawn pos
        List<VoxelWorld.ChunkSaveData> chunkSaveDatas = new List<VoxelWorld.ChunkSaveData>();
        int chunkRes = fullImportData.chunkResolution;
        VoxelData[] createVoxelDatas = neededVoxelDatas.Select(tc => tc.CreateInstance()).ToArray();

        // todo enemies, fine tune things, vd configuration

        // todo save to a file (json?) or something the mapdata size is currently too large to hold in the SO comfortably(causes editor lag) and will only get bigger
        // todo also look into compressing data

        MapData mapData = new MapData();
        mapData.chunkResolution = chunkRes;
        mapData.chunks = new SerializableDictionary<Vector3Int, VoxelWorld.ChunkSaveData>();
        int chunkVol = chunkRes * chunkRes * chunkRes;

        for (int m = 0; m < fullImportData.models.Length; m++) {
            // todo? dont chunk this import data, have in its own shape
            VoxelModelImportData voxelModelImportData = fullImportData.models[m];
            for (int c = 0; c < voxelModelImportData.chunks.Length; c++) {
                // // todo models may be non chunk aligned, need to properly convert to chunks on our grid
                // // todo alternatively, have multiple voxelworlds?
                // or align when importing
                ChunkImportData chunkImportData = voxelModelImportData.chunks[c];
                Vector3Int chunkImportCPos = chunkImportData.chunkPos;// + voxelModelImportData.position / chunkRes;
                if (mapData.chunks.ContainsKey(chunkImportCPos)) {
                    // add intersection
                    for (int v = 0; v < chunkVol; v++) {
                        Voxel existingVoxel = mapData.chunks[chunkImportCPos].voxels[v];
                        if (existingVoxel.voxelMaterialId == 0 && chunkImportData.voxels[v].materialId != 0) {
                            mapData.chunks[chunkImportCPos].voxels[v] = new Voxel(chunkImportData.voxels[v].materialId, createVoxelDatas.ToArray());
                        }
                    }
                    continue;
                }
                VoxelWorld.ChunkSaveData chunkSaveData = new VoxelWorld.ChunkSaveData();
                chunkSaveData.chunkPos = chunkImportCPos;
                chunkSaveData.voxels = new Voxel[chunkVol];
                for (int v = 0; v < chunkVol; v++) {
                    // todo add more vd based on type? or that will happen later?
                    chunkSaveData.voxels[v] = new Voxel(chunkImportData.voxels[v].materialId, createVoxelDatas.ToArray());
                }
                mapData.chunks.Add(chunkSaveData.chunkPos, chunkSaveData);
            }
        }
        int maxBlockType = 256;
        Dictionary<int, SpecialBlocksMap> specialBlocksDict = specialBlocksMap.ToDictionary(sb => sb.matid);
        // set block types
        for (int c = 0; c < mapData.chunks.Count; c++) {
            VoxelWorld.ChunkSaveData chunkSaveData = mapData.chunks[mapData.chunks.Keys.ToArray()[c]];
            for (int v = 0; v < chunkVol; v++) {
                Voxel voxel = chunkSaveData.voxels[v];
                BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
                if (voxel.voxelMaterialId < maxBlockType) {
                    // if (voxel.voxelMaterialId == 0) {
                    int blockTypeId = voxel.voxelMaterialId;
                    if (specialBlocksDict.ContainsKey(voxel.voxelMaterialId)){
                        if (specialBlocksDict[voxel.voxelMaterialId].newblockType.IsValid()){
                            blockTypeId = specialBlocksDict[voxel.voxelMaterialId].newblockType.blockid;
                        }
                        SpecialBlockType specialBlockType = specialBlocksDict[voxel.voxelMaterialId].specialType;
                        if (specialBlockType == SpecialBlockType.PLAYER_SPAWN) {
                            mapData.playerSpawn = VoxelChunk.GetLocalPos(v, chunkRes) + chunkSaveData.chunkPos * chunkRes;
                        }
                    }

                    voxel.RawSetVoxelDataFor<BlockTypeVoxelData>(new BlockTypeVoxelData() {
                        blockTypeRef = new BlockTypeRef().SetBlockId(blockTypeId)
                    });
                        BlockTypeVoxelData blockTypeVoxelData1 = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
                    // Debug.Log($"set {blockTypeVoxelData} id to {btypel.importMatId} now {blockTypeVoxelData1}");
                } else {
                    voxel.RawSetVoxelDataFor<BlockTypeVoxelData>(new BlockTypeVoxelData() {
                        blockTypeRef = unkownBlockDefault
                    });
                }
            }
        }
        // flood outer map with stone
        // ? organize for better loading
        return mapData;
    }
}