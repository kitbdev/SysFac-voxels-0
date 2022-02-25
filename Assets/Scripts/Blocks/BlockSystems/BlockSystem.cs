using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;

public struct LightVoxelData : VoxelData {// todo move
    public int intensity;
    // color?
    public int color;
}

public class BlockSystem : Singleton<BlockSystem> {

    public VoxelWorld world; // todo want to be independent of world
    public TickUpdater blockTick;
    public List<int> updateableBlockVDs = new List<int>();
    public BlockLiquidSystem liquidSystem;
    public Layer waterLayer;
    public Color[] voxelLightColors;

    [SerializeField, HideInInspector]
    SerializableDictionary<Vector3Int, List<GameObject>> gosToClearForChunk = new SerializableDictionary<Vector3Int, List<GameObject>>();

    BlockTypesHolderSO blockTypeHolder;

    private void OnValidate() {
        blockTick ??= GetComponent<TickUpdater>();
    }
    // protected override void Awake() {
    protected override void Awake() {
        base.Awake();
        blockTick ??= GetComponent<TickUpdater>();
        blockTypeHolder = GameManager.Instance.mainBlockTypeHolder;
    }
    private void OnEnable() {
        blockTick.onTickUpdateEvent += OnBlockTick;
        world.onChunkLoadEvent += OnChunkLoad;
        world.onChunkPreUnloadEvent += OnChunkUnload;
    }
    private void OnDisable() {
        blockTick.onTickUpdateEvent -= OnBlockTick;
        if (world) {
            // in case world is unloaded first when exiting
            world.onChunkLoadEvent -= OnChunkLoad;
            world.onChunkPreUnloadEvent -= OnChunkUnload;
        }
    }
    void OnBlockTick(TickUpdater.TickUpdateArgs tickarg) {
        // Debug.Log($"update! {tickarg}");
    }
    public Voxel[] GetVoxelsWith(System.Type voxelDataType) {
        // todo queryable?
        // todo maintain a list? of all voxels that need to update
        return null;
    }
    void OnChunkLoad(Vector3Int chunkpos) {
        // after this chunk has been loaded
        // todo find vds we need and but them in a list?
        VoxelChunk voxelChunk = world.GetChunkAt(chunkpos);
        for (int i = 0; i < voxelChunk.voxels.Length; i++) {
            Voxel voxel = voxelChunk.voxels[i];
            BlockType blockType = voxel.GetVoxelDataFor<BlockTypeVoxelData>().blockTypeRef.GetBlockType();
            if (voxel.TryGetVoxelDataType<LightVoxelData>(out var lightvd)) {
                // add light GO
                // todo
                // flood fill neighbors to sparsely place
                // AddGoForChunk(chunkpos, lcolgo);
            }
            if (voxel.TryGetVoxelDataType<LiquidDataVD>(out var liquidDataVD)) {
                Vector3Int bpos = voxelChunk.GetVoxelBlockPos(i);
                GameObject lcolgo = new GameObject($"Liquid Collider {chunkpos}-{bpos}");
                lcolgo.transform.parent = world.transform;
                lcolgo.transform.localPosition = world.BlockposToWorldPos(bpos);
                // Debug.Log($"Found liquid {voxel.ToStringFull()} {chunkPos} -{VoxelChunk.GetLocalPos(i, mapData.chunkResolution)}");
                lcolgo.layer = waterLayer;
                BoxCollider boxCollider = lcolgo.AddComponent<BoxCollider>();
                boxCollider.size = world.voxelSize * Vector3.one;
                boxCollider.isTrigger = true;
                AddGoForChunk(chunkpos, lcolgo);
            }
        }
    }

    private void AddGoForChunk(Vector3Int chunkpos, GameObject go) {
        if (!gosToClearForChunk.ContainsKey(chunkpos)) {
            gosToClearForChunk.Add(chunkpos, new List<GameObject>());
        }
        gosToClearForChunk[chunkpos].Add(go);
    }

    void OnChunkUnload(Vector3Int chunkpos) {
        // before this chunk is unloaded
        // todo remove from any update lists
        // todo ? save here

        if (gosToClearForChunk.ContainsKey(chunkpos)) {
            List<GameObject> clearGos = gosToClearForChunk[chunkpos];
            foreach (var go in clearGos) {
                if (Application.isPlaying) {
                    Destroy(go);
                } else {
                    DestroyImmediate(go);
                }
            }
        }

        VoxelChunk voxelChunk = world.GetChunkAt(chunkpos);
        foreach (var voxel in voxelChunk.voxels) {
            BlockType blockType = voxel.GetVoxelDataFor<BlockTypeVoxelData>().blockTypeRef.GetBlockType();
            if (voxel.TryGetVoxelDataType<LightVoxelData>(out var lightvd)) {
                // remove light GO
                // todo
            }
            if (voxel.TryGetVoxelDataType<LiquidDataVD>(out var liquidDataVD)) {

            }
        }
    }
}