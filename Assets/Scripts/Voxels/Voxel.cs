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

    public int blockId;
    public VoxelShape shape;
    public bool isTransparent;
    public Vector2Int textureCoord;
    // public Color tint;
    // todo lighting data?
    // todo anim data

    public Voxel() {
        ResetToDefaults();
    }
    public Voxel(BlockType blockType) {
        blockId = blockType.id;
        shape = blockType.shape;
        isTransparent = blockType.isTransparent;
        textureCoord = BlockManager.Instance.GetBlockTexCoord(blockType);
    }

    public void ResetToDefaults() {
        shape = VoxelShape.cube;
        blockId = 0;
        isTransparent = false;
        textureCoord = Vector2Int.zero;
    }
    public void CopyValues(Voxel voxel) {
        shape = voxel.shape;
        blockId = voxel.blockId;
        isTransparent = voxel.isTransparent;
        textureCoord = voxel.textureCoord;
    }
    public override string ToString() {
        return $"Voxel {shape.ToString()} id:{blockId}";
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