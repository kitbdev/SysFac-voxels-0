using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;
using VoxelSystem.Importer;
using System;

public class BlockComponentManager : Singleton<BlockComponentManager> {

    [SerializeField] VoxelWorld world;

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
        world.loadImportPopulateEvent += PopulateBlockType;
    }
    private void OnDisable() {
        if (world) world.loadImportPopulateEvent -= PopulateBlockType;
    }

    private void PopulateBlockType(ImportedVoxel importedVoxel, Voxel voxel) {
        // should be one block per palette row
        int blockid = (importedVoxel.materialId + 7) / 8;
        if (blockid > maxBlockId) {
            blockid = 1;
        }
        // blockid = Unity.Mathematics.math.clamp(blockid, 0, maxBlockId);
        BlockTypeVoxelData btvdata = new BlockTypeVoxelData() {
            blockTypeRef = new BlockTypeRef().SetBlockId(blockid)
        };
        voxel.RawSetVoxelDataFor<BlockTypeVoxelData>(btvdata);//, true, false, false
    }
}