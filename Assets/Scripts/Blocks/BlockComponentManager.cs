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
    private void OnEnable() {
        world.loadImportPopulateEvent += PopulateBlockType;
    }
    private void OnDisable() {
        if (world) world.loadImportPopulateEvent -= PopulateBlockType;
    }

    private void PopulateBlockType(ImportedVoxel importedVoxel, Voxel voxel) {
        voxel.SetOrAddVoxelDataFor<BlockTypeVoxelData>(new BlockTypeVoxelData() {
            blockTypeRef = new BlockTypeRef().SetBlockId(importedVoxel.materialId / 8 + 1)
        });
    }
}