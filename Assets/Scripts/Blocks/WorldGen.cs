using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem;

public class WorldGen : MonoBehaviour {

    [ShowAsChild(nameof(BlockTypeRef.idname))]
    public BlockTypeRef airBlockref = new BlockTypeRef("air");
    [ShowAsChild(nameof(BlockTypeRef.idname))]
    public BlockTypeRef stoneBlockRef = new BlockTypeRef("stone");
    [ShowAsChild(nameof(BlockTypeRef.idname))]
    public BlockTypeRef grassBlockRef = new BlockTypeRef("grass");
    [ShowAsChild(nameof(BlockTypeRef.idname))]
    public BlockTypeRef dirtBlockRef = new BlockTypeRef("dirt");

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
    IEnumerator GenChunkCo(Vector3Int cpos) {
        Debug.Log($"Generating chunk {cpos}");
        VoxelChunk chunk = world.GetChunkAt(cpos);
        float[] heightmap = new float[chunk.floorArea];
        for (int i = 0; i < chunk.floorArea; i++) {
            heightmap[i] = 2;
        }
        VoxelMaterialId[] matData = new VoxelMaterialId[chunk.volume];
        // DensityVoxelData[] data = new DensityVoxelData[chunk.volume];
        for (int x = 0; x < chunk.resolution; x++) {
            for (int z = 0; z < chunk.resolution; z++) {
                int hmid = x * chunk.resolution + z;
                for (int y = 0; y < chunk.resolution; y++) {
                    Vector3Int vlpos = new Vector3Int(x, y, z);
                    Vector3Int vwpos = cpos * chunk.resolution + vlpos;
                    string touse = airBlockref.idname;
                    if (vwpos.y == heightmap[hmid] - 1) {
                        touse = grassBlockRef.idname;
                    } else if (vwpos.y > heightmap[hmid] - 4 && vwpos.y < heightmap[hmid] - 1) {
                        touse = dirtBlockRef.idname;
                    } else if (vwpos.y <= heightmap[hmid] - 4) {
                        touse = stoneBlockRef.idname;
                    }
                    BlockType blockType = blockManager.GetBlockType(touse);
                    // if (blockType.id > 0) {
                    //     Debug.Log(blockType);
                    // }
                    VoxelMaterialId matid = blockType.voxelMaterialId;
                    // todo fix
                    matData[chunk.IndexAt(vlpos)] = matid;
                }
            }
            yield return null;
        }
        chunk.SetVoxelMaterials(matData);
        // chunk.SetVoxel(chunk.IndexAt(new Vector3Int(8, 8, 8)), new Voxel(blockManager.GetBlockTypeAtIndex(2)));
        chunk.Refresh();
    }

}