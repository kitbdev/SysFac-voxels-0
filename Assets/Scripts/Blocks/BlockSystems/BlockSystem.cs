using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;

public class BlockSystem : Singleton<BlockSystem> {

    public TickUpdater blockTick;
    public List<int> updateableBlockVDs = new List<int>();

    private void OnValidate() {
        blockTick ??= GetComponent<TickUpdater>();
    }
    // protected override void Awake() {
    protected override void Awake() {
        base.Awake();
        blockTick ??= GetComponent<TickUpdater>();
    }
    private void OnEnable() {
        blockTick.onTickUpdateEvent += TestBlockUpdate;
    }
    private void OnDisable() {
        blockTick.onTickUpdateEvent -= TestBlockUpdate;
    }
    void TestBlockUpdate(TickUpdater.TickUpdateArgs tickarg) {
        // Debug.Log($"update! {tickarg}");
    }
    public Voxel[] GetVoxelsWith(System.Type voxelDataType){
        // todo queryable?
        // todo maintain a list? of all voxels that need to update
        return null;
    }
}