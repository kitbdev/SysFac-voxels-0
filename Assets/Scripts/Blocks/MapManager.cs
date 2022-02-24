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
        Debug.Log("loaded map");
        world.chunkResolution = mapData.chunkResolution;
        foreach (var chunk in mapData.chunks) {
            for (int i = 0; i < chunk.voxels.Length; i++) {
                Voxel vox = chunk.voxels[i];
                if (vox.HasVoxelDataFor<LightVoxelData>()) {
                    // todo add a point light source with the accurate color
                    // ! but only activate it if there isnt too many
                    // voxel light system(manager)?
                    //? prefab or make it?
                }
            }
        }
        // baseMapData // todo keep?
        // todo load only part of the map, around the player
        if (mapData == null && mapData.chunks.Length != 0) {
            Debug.LogWarning("Cannot load map mapdata not set or preloaded");
            return;
        }
        world.LoadChunksFromData(mapData.chunks.ToArray());
        VoxelChunk[] voxelChunks = world.activeChunks.ToArray();
        foreach (var chunk in voxelChunks) {
            for (int i = 0; i < chunk.voxels.Length; i++) {
                Voxel vox = chunk.voxels[i];
                if (vox.voxelMaterialId == 3) {// todo still cant find
                    // liquids
                    // todo layer,tag?, and better parent
                    // todo make sure it gets removed on unload
                    GameObject lcolgo = new GameObject($"Liquid Collider {chunk.chunkPos} - {i}");
                    // todo on the chunk? maybe? make sure it gets removed
                    lcolgo.transform.parent = world.transform;
                    lcolgo.transform.localPosition = VoxelChunk.GetLocalPos(i, mapData.chunkResolution) + chunk.chunkPos * mapData.chunkResolution;
                    Debug.Log($"Found liquid {vox.ToStringFull()} {chunk.chunkPos} -{VoxelChunk.GetLocalPos(i, mapData.chunkResolution)}");
                    // lcolgo.layer = ?
                    BoxCollider boxCollider = lcolgo.AddComponent<BoxCollider>();
                    boxCollider.size = world.voxelSize * Vector3.one;
                    boxCollider.isTrigger = true;
                    // todo add another component that refers to the voxel? liquiddata?
                }
            }
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