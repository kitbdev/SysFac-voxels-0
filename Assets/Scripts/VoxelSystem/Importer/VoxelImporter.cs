using UnityEngine;
using VoxReader;

namespace VoxelSystem.Importer {
    [System.Serializable]
    public class VoxelRoomData {
        public int id;
        public UnityEngine.Vector3 offset;
        public Vector3Int offsetInt;
        public RawChunkData[] rawChunks;
    }
    [System.Serializable]
    public struct RawVoxel {
        public int materialId;
    }
    [System.Serializable]
    public class RawChunkData {
        public Vector3Int chunkPos;
        public RawVoxel[] rawVoxels;
    }
    [System.Serializable]
    public class VoxelImportSettings {
        [HideInInspector]
        public string filepath;
        public VoxelMaterialSetSO voxelMaterialSet;
        public float voxelSize = 1f;
        public int chunkResolution = 16;
        public Vector3Int chunkPosOffset;

        public override string ToString() {
            return "VoxelImportSettings" +
                "filepath:" + filepath + " " +
                "voxelMaterialSet:" + voxelMaterialSet + " " +
                "voxelSize:" + voxelSize + " " +
                "chunkResolution:" + chunkResolution + " " +
                "chunkPosOffset:" + chunkPosOffset + " ";
        }
    }
    public class VoxelImporter : MonoBehaviour {
        // public static void Load(VoxelWorld world, string filepath) {
        // Kutil.SaveSystem.StartLoad()
        //                 .InCustomFullPath(filepath)
        //                 .As(Kutil.SaveSystem.SaveBuilder.SerializeType.BINARY)
        //                 .TryLoadBin(out var bytes);
        // Load(world, bytes);
        // }
        public static VoxelRoomData[] Load(VoxelImportSettings importSettings) {
            if (importSettings.voxelSize <= 0 ||
                importSettings.chunkResolution <= 0 ||
                importSettings.filepath == null || importSettings.filepath == "") {
                Debug.LogError($"Invalid import settings {importSettings} for .vox importer");
            }
            VoxReader.Interfaces.IVoxFile voxFile = VoxReader.VoxReader.Read(importSettings.filepath);
            if (voxFile == null) {
                Debug.LogError($"Failed to load voxels '{importSettings.filepath}'");
                return default;
            }
            Debug.Log($"Loaded {importSettings.filepath} version {voxFile.VersionNumber}");
            Debug.Log($"Loaded models:{voxFile.Models.Length} chunks:{voxFile.Chunks.Length} colors:{voxFile.Palette.Colors.Length}");

            for (int i = 0; i < voxFile.Models.Length; i++) {
                VoxReader.Interfaces.IModel model = voxFile.Models[i];
                Debug.Log($"model[{i}]: {model}");
                Vector3Int modelSize = ToUnityUnit(model.Size);
                // get num chunks to make
                Vector3Int numChunksPerDir = (Vector3Int.one + modelSize) / importSettings.chunkResolution;
                int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
                for (int v = 0; v < model.Voxels.Length; v++) {
                    VoxReader.Voxel voxel = model.Voxels[v];
                    //
                }
            }
            // VoxelWorld.VoxelRoomData[] chunkSaveDatas;
            // todo 
            // world.LoadChunksFromData(chunkSaveDatas);
            return default;
        }
        static Vector3Int ToUnityUnit(VoxReader.Vector3 vec3) => new Vector3Int(vec3.X, vec3.Y, vec3.Z);
    }
}