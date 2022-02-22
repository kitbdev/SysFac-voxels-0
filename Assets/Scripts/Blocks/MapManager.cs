using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;
using VoxelSystem.Importer;
using System;
using System.Linq;

public partial class MapManager : Singleton<MapManager> {

    [SerializeField] Transform player;
    [SerializeField] VoxelWorld world;
    [SerializeField] MapSO mapHolder;

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
        
        // .SelectMany(bid => Enumerable.Range(bid.importMatIdStart, bid.importMatIdEnd).Zip(bid,(i,bid)=>bid))

        world.loadImportPopulateEvent += PopulateBlockType;
    }
    private void OnDisable() {
        if (world) {
            world.loadImportPopulateEvent -= PopulateBlockType;
        }
    }
    private void Start() {
        StartLoad();
    }
    void StartLoad() {
        // Debug.Log("Going to load");
        // load to world
        if (mapHolder?.mapData != null || mapHolder?.mapData.chunks.Count == 0) {
            world.LoadChunksFromData(mapHolder.mapData.chunks.Values.ToArray());
        } else {
            Debug.LogWarning("Cannot load map mapdata not set or preloaded");
        }
    }

    private void PopulateBlockType(ImportedVoxel importedVoxel, Voxel voxel) {
        // // should be one block per palette row
        // BlockTypeRef blockTypeRef = unkownBlockDefault;
        // if (blockTypeLoad.blockImportDict.ContainsKey(importedVoxel.materialId)) {
        //     BlockTypeLoad blockImportData = blockTypeLoad.blockImportDict[importedVoxel.materialId];
        //     // Debug.Log("Found key {voxel}");
        //     blockTypeRef = new BlockTypeRef().SetBlockId(blockImportData.importMatId);
        // } else {
        //     // Debug.Log($"not Found key {importedVoxel.voxelMaterialId}");
        // }
        // // blockid = Unity.Mathematics.math.clamp(blockid, 0, maxBlockId);
        // BlockTypeVoxelData btvdata = new BlockTypeVoxelData() {
        //     blockTypeRef = blockTypeRef
        // };
        // BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        // blockTypeVoxelData.blockTypeRef = blockTypeRef;
    }
}