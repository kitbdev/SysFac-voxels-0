using UnityEngine;

namespace VoxelSystem.Importer {
    // [CreateAssetMenu(fileName = "ImportedVoxelData", menuName = "SysFac/ImportedVoxelData", order = 0)]
    public class ImportedVoxelData : ScriptableObject {
        public string modelName;
        public FullVoxelImportData fullVoxelImportData;
    }
    [System.Serializable]
    public class FullVoxelImportData {
        public int chunkResolution;
        public float voxelSize;
        public VoxelModelImportData[] models;
    }
    [System.Serializable]
    public class VoxelModelImportData {
        // public int id;
        // public UnityEngine.Vector3 offset;
        public Vector3Int modelSize;
        public Vector3Int numChunksByAxis;
        public Vector3Int position;
        public Matrix4x4 trMatrix;
        public ChunkImportData[] chunks;
        // public ImportedVoxel[] voxels;//?
    }
    [System.Serializable]
    public class ChunkImportData {
        public Vector3Int chunkPos;
        public ImportedVoxel[] voxels;
    }
    [System.Serializable]
    public struct ImportedVoxel {
        public int materialId;
        // public UnityEngine.Color materialColor;
    }
    [System.Serializable]
    public class VoxelImportSettings {
        [HideInInspector]
        public string filepath;
        // public VoxelMaterialSetSO voxelMaterialSet;
        public float voxelSize = 1f;
        public int chunkResolution = 16;
        public Vector3Int chunkPosOffset;

        public override string ToString() {
            return "VoxelImportSettings" +
                "filepath:" + filepath + " " +
                // "voxelMaterialSet:" + voxelMaterialSet + " " +
                "voxelSize:" + voxelSize + " " +
                "chunkResolution:" + chunkResolution + " " +
                "chunkPosOffset:" + chunkPosOffset + " ";
        }
    }
}