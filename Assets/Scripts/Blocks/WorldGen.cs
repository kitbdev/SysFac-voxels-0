using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class WorldGen : MonoBehaviour {

    public BlockTypeRef airBlockref;
    public BlockTypeRef stoneBlockRef;
    public BlockTypeRef grassBlockRef;
    public BlockTypeRef dirtBlockRef;

    VoxelWorld world;
    BlockManager blockManager;

    private void Awake() {
        world = GetComponent<VoxelWorld>();
        blockManager = BlockManager.Instance;
    }
    private void OnEnable() {
        world.generateChunkEvent += GenChunk;
    }
    private void OnDisable() {
        if (world) world.generateChunkEvent -= GenChunk;
    }

    public void GenChunk(Vector3Int cpos) {
        if (!world.HasChunkActiveAt(cpos)) {
            return;
        }
        StartCoroutine(GenChunkCo(cpos));
    }

    // todo jobs
    IEnumerator GenChunkCo(Vector3Int cpos) {
        Debug.Log($"Generating chunk {cpos}");
        VoxelChunk chunk = world.GetChunkAt(cpos);
        float[] heightmap = new float[chunk.floorArea];
        for (int i = 0; i < chunk.floorArea; i++) {
            heightmap[i] = 2;
        }
        // todo any datas?
        VoxelMaterialId[] matData = new VoxelMaterialId[chunk.volume];
        BlockTypeVoxelData[] blockTypeData = new BlockTypeVoxelData[chunk.volume];
        for (int x = 0; x < chunk.resolution; x++) {
            for (int z = 0; z < chunk.resolution; z++) {
                int hmid = x * chunk.resolution + z;
                for (int y = 0; y < chunk.resolution; y++) {
                    Vector3Int vlpos = new Vector3Int(x, y, z);
                    Vector3Int vwpos = cpos * chunk.resolution + vlpos;
                    BlockTypeRef touse = airBlockref;
                    if (vwpos.y == heightmap[hmid] - 1) {
                        touse = grassBlockRef;
                    } else if (vwpos.y > heightmap[hmid] - 4 && vwpos.y < heightmap[hmid] - 1) {
                        touse = dirtBlockRef;
                    } else if (vwpos.y <= heightmap[hmid] - 4) {
                        touse = stoneBlockRef;
                    }
                    BlockType blockType = touse.GetBlockType();
                    // if (blockType.id > 0) {
                    //     Debug.Log(blockType);
                    // }
                    VoxelMaterialId matid = blockType.voxelMaterialId;
                    matData[chunk.IndexAt(vlpos)] = matid;
                    blockTypeData[chunk.IndexAt(vlpos)] = new BlockTypeVoxelData() { blockTypeRef = touse };
                }
            }
            // yield return null;
        }
        yield return null;
        // Debug.Log("Finished gen loop");
        // chunk.SetVoxelMaterials(matData);
        chunk.SetVoxelDatas<BlockTypeVoxelData>(blockTypeData);
        // Debug.Log("set voxel data");
        yield return null;
        // chunk.SetVoxel(chunk.IndexAt(new Vector3Int(8, 8, 8)), new Voxel(blockManager.GetBlockTypeAtIndex(2)));
        chunk.Refresh();
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct ChunkGenJob : IJobFor {
        [Unity.Collections.ReadOnly]
        Vector3Int chunkPos;
        [WriteOnly]
        NativeArray<BlockTypeVoxelData> blocks;

        public void Execute(int index) {
            // GenMeshExecute(index);
        }
    }
}