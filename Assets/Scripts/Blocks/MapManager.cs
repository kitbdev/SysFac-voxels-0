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
    [SerializeField] bool loadOnStart = true;
    public Color[] voxelLightColors;

    MapData baseMapData;

    private void Reset() {
        world = GameManager.Instance?._mainWorld;
    }
    protected override void Awake() {
        base.Awake();
        world = GameManager.Instance?._mainWorld;
    }
    private void OnEnable() {
        world.loadImportPopulateEvent += PopulateBlockType;
    }
    private void OnDisable() {
        if (world) {
            world.loadImportPopulateEvent -= PopulateBlockType;
        }
    }
    private void Start() {
        if (loadOnStart) {
            LoadMap();
        }
    }
    struct LightVoxelData : VoxelData {// todo move
        // color?
        int color;
    }
    [ContextMenu("Load")]
    void LoadMap() {
        // Debug.Log("Going to load");
        // load to world
        world.Clear();
        var mapData = mapHolder.GetMapData();
        foreach (var chunk in mapData.chunks) {
            foreach (var vox in chunk.voxels) {
                if (vox.HasVoxelDataFor<LightVoxelData>()) { 
                    // todo add a point light source with the accurate color
                    // ! but only activate it if there isnt too many
                    // voxel light system(manager)?
                }
            }
        }
        // baseMapData // todo keep?
        // todo load only part of the map, around the player
        if (mapData != null || mapData.chunks.Length == 0) {
            world.LoadChunksFromData(mapData.chunks.ToArray());
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