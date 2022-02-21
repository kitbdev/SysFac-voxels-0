using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;
using VoxelSystem.Importer;
using System;
using System.Linq;

public class MapManager : Singleton<MapManager> {

    [SerializeField] Transform player;
    [SerializeField] VoxelWorld world;
    [SerializeField] ImportedVoxelData ImportedVoxelData;

    public enum SpecialBlocks {
        NONE,
        UNKNOWN,
        MAP_BORDER,
        PLAYER_SPAWN,
        LOOT_SPAWN,
        ENEMY_SPAWN,
    }
    public BlockTypeRef unkownBlockDefault;

    [System.Serializable]
    public struct BlockImportData {
        public int importMatId;
        public SpecialBlocks specialType;
        public BlockTypeRef blockType;
    }
    [System.Serializable]
    class AllBlockImportData {
        public BlockImportData[] blockImport;
        public Dictionary<int, BlockImportData> blockImportDict;
    }
    [SerializeField]
    AllBlockImportData blockImport;

    private void Reset() {
        world = GameManager.Instance?._mainWorld;
    }
    protected override void Awake() {
        base.Awake();
        world = GameManager.Instance?._mainWorld;
    }
    int maxBlockId;
    private void OnEnable() {
        maxBlockId = BlockManager.Instance.blockTypes.Length - 1;
        blockImport.blockImportDict = blockImport.blockImport.ToDictionary((bid => bid.importMatId));
        world.loadImportPopulateEvent += PopulateBlockType;// todo tell world to load instead
    }
    private void OnDisable() {
        if (world) world.loadImportPopulateEvent -= PopulateBlockType;
    }
    private void Start() {
        // preprocess map
        PreLoadMap(ImportedVoxelData.fullVoxelImportData);
        StartLoad();
    }
    void StartLoad() {
        // load to world

    }
    void LoadChunks() {

    }

    private void PopulateBlockType(ImportedVoxel importedVoxel, Voxel voxel) {
        // should be one block per palette row
        BlockTypeRef blockTypeRef = unkownBlockDefault;
        if (blockImport.blockImportDict.ContainsKey(importedVoxel.materialId)) {
            BlockImportData blockImportData = blockImport.blockImportDict[importedVoxel.materialId];
            blockTypeRef = new BlockTypeRef().SetBlockId(blockImportData.importMatId);
        }
        // blockid = Unity.Mathematics.math.clamp(blockid, 0, maxBlockId);
        BlockTypeVoxelData btvdata = new BlockTypeVoxelData() {
            blockTypeRef = blockTypeRef
        };
        voxel.RawSetVoxelDataFor<BlockTypeVoxelData>(btvdata);//, true, false, false
    }
    private FullVoxelImportData PreLoadMap(FullVoxelImportData fullImportData) {
        // find player spawn pos
        List<VoxelWorld.ChunkSaveData> chunkSaveDatas = new List<VoxelWorld.ChunkSaveData>();
        TypeChoice<VoxelData>[] neededData = world.neededData;
        int chunkRes = world.chunkResolution;
        VoxelData[] createVoxelDatas = neededData.Select(tc => tc.CreateInstance()).ToArray();

        // todo do we need to convert everything into voxels at the start?
        // ! actually, do this in editor
        // yes, so I can add things in editor like enemies and fine tune things, configuration
        VoxelWorld.ChunkSaveData chunkSaveData = new VoxelWorld.ChunkSaveData();
        chunkSaveData.voxels = new Voxel[chunkRes * chunkRes * chunkRes];
        // chunkSaveData.chunkPos = ;
        for (int i = 0; i < chunkRes * chunkRes * chunkRes; i++) {
            chunkSaveData.voxels[i] = new Voxel(0, createVoxelDatas.ToArray());
        }
        // flood outer map with stone
        // ? organize for better loading
        //  to a single model of chunks and help with reloading?
        return fullImportData;
    }
}