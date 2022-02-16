using UnityEngine;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    /// <summary>
    /// holds data for a single voxel
    /// </summary>
    [System.Serializable]
    public class NVoxel {
        
        VoxelMaterialId voxelMaterialId;
        [SerializeReference]
        VoxelData[] voxelDatas;

        public bool GetVoxelDataType<T>(out T voxelData)
            where T : VoxelData {
            voxelData = null;
            return false;
        }

        public override string ToString() {
            return $"Voxel {voxelMaterialId} datas:{voxelDatas.Length}";
        }

        public static Vector3Int[] GetUnitNeighbors(Vector3Int pos, bool includeSelf = false) {
            var neighbors = unitDirs.Select((v) => { return v + pos; });
            if (!includeSelf) {
                neighbors = neighbors.Skip(1);
            }
            return neighbors.ToArray();
        }

        public static Vector3Int[] unitDirs = new Vector3Int[6] {
        new Vector3Int(1,0,0),// right
        new Vector3Int(0,0,1),// forward
        new Vector3Int(0,1,0),// up
        new Vector3Int(-1,0,0),// left
        new Vector3Int(0,0,-1),// back
        new Vector3Int(0,-1,0),// down
        };
        public static Vector3Int[] cubePositions = {
        new Vector3Int(0,0,0),//0
        new Vector3Int(1,0,0),//1
        new Vector3Int(1,0,1),//2
        new Vector3Int(0,0,1),//3
        new Vector3Int(0,1,0),//4
        new Vector3Int(1,1,0),//5
        new Vector3Int(1,1,1),//6
        new Vector3Int(0,1,1),//7
        };
        public readonly static Vector3Int[] dirTangents = new Vector3Int[6] {
        new Vector3Int(0,0,1),
        new Vector3Int(-1,0,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(1,0,0),
        };
        public readonly static Vector3[] vOffsets = new Vector3[6] {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        };
    }
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

        // public Voxel() {
        //     ResetToDefaults();
        // }
        // public Voxel(BlockType blockType) {
        //     blockId = blockType.id;
        //     shape = blockType.shape;
        //     isTransparent = blockType.isTransparent;
        //     textureCoord = BlockManager.Instance.GetBlockTexCoord(blockType);
        // }

        public void ResetToDefaults() {
            shape = VoxelShape.cube;
            blockId = 0;
            isTransparent = false;
            textureCoord = Vector2Int.zero;
        }
        // public void CopyValues(Voxel voxel) {
            // shape = voxel.shape;
            // blockId = voxel.blockId;
            // isTransparent = voxel.isTransparent;
            // textureCoord = voxel.textureCoord;
        // }
        public override string ToString() {
            return $"Voxel {shape.ToString()} id:{blockId}";
        }

    }

    public enum VoxelDirection {
        RIGHT = 0,
        FORWARD = 1,
        UP = 2,
        LEFT = 3,
        BACK = 4,
        DOWN = 5,
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
}