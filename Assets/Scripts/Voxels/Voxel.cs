using UnityEngine;
using Kutil;

/// <summary>
/// holds data for a single voxel
/// </summary>
[System.Serializable]
public class Voxel {
    public enum VoxelShape {
        none,
        cube,
        xfull,
        xsmall,
        customcubey,
        customcubexyz,
        custom,
    }
    public VoxelShape shape;
    public int blockId;
    // public Color tint;
    // todo lighting data?
    // todo anim data

    public void ResetToDefaults() {
        shape = VoxelShape.cube;
        blockId = 0;
    }
    public void CopyValues(Voxel voxel) {
        shape = voxel.shape;
        blockId = voxel.blockId;
    }
    public override string ToString() {
        return $"Voxel {shape.ToString()} tex:{blockId}";
    }
}
[System.Serializable]
public class FatVoxel {
    public int index;
    public Vector3Int position;
    [SerializeField]
    public VoxelChunk chunk;

    public Voxel.VoxelShape shape;
    public int textureId;

    public override string ToString() {
        return $"Voxel {index} {position} c{chunk.chunkPos}";
    }
}