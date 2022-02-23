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
    // public SerializedType[] baseSavedTypes;
    public VoxelWorld.ChunkSaveData[] chunks;
    // public SerializableDictionary<Vector3Int, VoxelWorld.ChunkSaveData> chunks;
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
        public override string ToString() {
            return $"matid:{matid} newType:{newblockType} special:{specialType}";
        }
    }

    [SerializeReference]
    public ImportedVoxelData importedVoxelData;
    public string filename = "mapdata";
    public bool overwrite = true;
    public bool autoIncrement = false;
    [ContextMenuItem("Reset needed voxel data", nameof(ResetNeededData))]
    public TypeChoice<VoxelData>[] neededVoxelDatas = new TypeChoice<VoxelData>[] { new TypeChoice<VoxelData>(typeof(DefaultVoxelData)) };

    public BlockTypeRef unkownBlockDefault;
    [SerializeField]
    public SpecialBlocksMap[] specialBlocksMap;

    void ResetNeededData() {
        neededVoxelDatas = new TypeChoice<VoxelData>[] { new TypeChoice<VoxelData>(typeof(DefaultVoxelData)) };
    }

    // [ContextMenu("Clear map data")]
    // public void ClearMapData() {
    //     // #if UNITY_EDITOR
    //     //         UnityEditor.Undo.RecordObject(this, "clear map");
    //     // #endif
    //     // mapData = null;
    //     Debug.Log($"map data cleared");
    // }

    [ContextMenu("Preprocess")]
    public void ProcessImportData() {
        Debug.Log($"prepocessing map...");
        // blockTypeLoadDict = allBlocksConverter.blockTypeConverter
        //             .ToDictionary((bid => bid.importMatId));
        // #if UNITY_EDITOR
        //         // note no undo because it is too much
        //         // UnityEditor.Undo.RecordObject(this, "preprocess map");
        //         EditorUtility.SetDirty(this);
        // #endif
        var mp = PreProcessMap(importedVoxelData.fullVoxelImportData, neededVoxelDatas.ToHashSet().ToArray());
        // todo? make sure chunk resolutions are the same
        if (mp != null) {
            // mapData = mp;
            SaveMapData(mp, filename, false);
            Debug.Log($"Finished processing import data {importedVoxelData.name} to map");
        } else {
            Debug.LogError($"Failed to process import data {importedVoxelData?.name} to map");
        }
    }
    public MapData GetMapData() {
        MapData mapData = LoadMapData(filename, false);
        // add unsaved needed data
        // todo? do this in world? but then it would happen every time we add a chunk individually
        VoxelData[] toAddVoxelDatas = neededVoxelDatas.Select(tc => tc.CreateInstance())
                                                        .Where(vd => !vd.shouldSave)
                                                        .ToArray();

        for (int c = 0; c < mapData.chunks.Length; c++) {
            VoxelWorld.ChunkSaveData chunkSaveData = mapData.chunks[c];
            int chunkVolume = mapData.chunkResolution * mapData.chunkResolution * mapData.chunkResolution;
            for (int v = 0; v < chunkVolume; v++) {
                Voxel voxel = chunkSaveData.voxels[v];
                voxel.SetOrAddVoxelDataFor(toAddVoxelDatas);
            }
        }
        return mapData;
    }
    private static void SaveMapData(MapData mapData, string filename, bool toPersistentOverLocal = false, bool overwrite = true, bool increment = false) {
        SaveSystem.SaveBuilder saveBuilder = SaveSystem.StartSave();
        if (toPersistentOverLocal) {
            saveBuilder.InPersistentDataPath(filename);
        } else {
            saveBuilder.InLocalDataPath(filename);
        }
        saveBuilder.CustomExtension("map");
        saveBuilder.CreateDirIfDoesntExist();
        if (overwrite) {
            saveBuilder.CanOverwrite();
        }
        if (increment) {
            saveBuilder.AutoIncrement();
        }
        // todo test
        saveBuilder.Content(mapData);
        saveBuilder.AsJSON().Zip().Save();
    }
    private static MapData LoadMapData(string filename, bool toPersistentOverLocal = false) {
        SaveSystem.SaveBuilder saveBuilder = SaveSystem.StartLoad();
        if (toPersistentOverLocal) {
            saveBuilder.InPersistentDataPath(filename);
        } else {
            saveBuilder.InLocalDataPath(filename);
        }
        saveBuilder.CustomExtension("map");
        saveBuilder.AsJSON().Zip().TryLoad<MapData>(out var mapData);
        return mapData;
    }
    private MapData PreProcessMap(FullVoxelImportData fullImportData, TypeChoice<VoxelData>[] neededVoxelDatas) {
        if (fullImportData == null) return null;
        // find player spawn pos
        List<VoxelWorld.ChunkSaveData> chunkSaveDatas = new List<VoxelWorld.ChunkSaveData>();
        int chunkRes = fullImportData.chunkResolution;
        VoxelData[] createVoxelDatas = neededVoxelDatas.Select(tc => tc.CreateInstance())
                                                        .Where(vd => vd.shouldSave)
                                                        .ToArray();

        // todo enemies, fine tune things, vd configuration

        MapData mapData = new MapData();
        mapData.chunkResolution = chunkRes;
        var chunkDict = new SerializableDictionary<Vector3Int, VoxelWorld.ChunkSaveData>();
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
                if (chunkDict.ContainsKey(chunkImportCPos)) {
                    // add intersection
                    for (int v = 0; v < chunkVol; v++) {
                        Voxel existingVoxel = chunkDict[chunkImportCPos].voxels[v];
                        if (existingVoxel.voxelMaterialId == 0 && chunkImportData.voxels[v].materialId != 0) {
                            chunkDict[chunkImportCPos].voxels[v] = new Voxel(chunkImportData.voxels[v].materialId, createVoxelDatas.ToArray());
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
                chunkDict.Add(chunkSaveData.chunkPos, chunkSaveData);
            }
        }
        mapData.chunks = chunkDict.Values.ToArray();
        int maxBlockType = 256;
        Dictionary<int, SpecialBlocksMap> specialBlocksDict = specialBlocksMap.ToDictionary(sb => sb.matid);
        // assign block types
        for (int c = 0; c < mapData.chunks.Length; c++) {
            VoxelWorld.ChunkSaveData chunkSaveData = mapData.chunks[c];
            for (int v = 0; v < chunkVol; v++) {
                Voxel voxel = chunkSaveData.voxels[v];
                // DefaultVoxelData defaultVoxelData = voxel.GetVoxelDataFor<DefaultVoxelData>();
                // defaultVoxelData.localVoxelPos = VoxelChunk.GetLocalPos(v, chunkRes);
                // defaultVoxelData.blockPos = defaultVoxelData.localVoxelPos + chunkSaveData.chunkPos * chunkRes;
                BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
                if (voxel.voxelMaterialId < maxBlockType) {
                    // if (voxel.voxelMaterialId == 0) {
                    int blockTypeId = voxel.voxelMaterialId;
                    if (specialBlocksDict.ContainsKey(voxel.voxelMaterialId)) {
                        SpecialBlocksMap specialBlock = specialBlocksDict[voxel.voxelMaterialId];
                        Debug.Log($"found special {specialBlock} !");
                        if (specialBlock.newblockType.IsValid()) {
                            if (specialBlocksDict[voxel.voxelMaterialId].newblockType.IsValid()) {
                                blockTypeId = specialBlocksDict[voxel.voxelMaterialId].newblockType.blockid;
                            }
                        }
                        SpecialBlockType specialBlockType = specialBlocksDict[voxel.voxelMaterialId].specialType;
                        if (specialBlockType == SpecialBlockType.PLAYER_SPAWN) {
                            mapData.playerSpawn = VoxelChunk.GetLocalPos(v, chunkRes) + chunkSaveData.chunkPos * chunkRes;
                        }
                    }
                    // if (blockTypeId != voxel.voxelMaterialId) {
                    //     Debug.Log($"assigning {voxel.voxelMaterialId} to {blockTypeId}");
                    // }
                    voxel.RawSetVoxelDataFor<BlockTypeVoxelData>(new BlockTypeVoxelData() {
                        blockTypeRef = new BlockTypeRef().SetBlockId(blockTypeId)
                    });
                    BlockTypeVoxelData blockTypeVoxelData1 = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
                    // Debug.Log($"set {blockTypeVoxelData} id to {btypel.importMatId} now {blockTypeVoxelData1}");
                } else {
                    Debug.Log($"set {blockTypeVoxelData} mat:{voxel.voxelMaterialId} block id to unknown type");
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