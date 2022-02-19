using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxReader;
using VoxReader.Interfaces;

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
        public UnityEngine.Color materialColor;
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
            IVoxFile voxFile = VoxReader.VoxReader.Read(importSettings.filepath);
            if (voxFile == null) {
                Debug.LogError($"Failed to load voxels '{importSettings.filepath}'");
                return default;
            }
            Debug.Log($"Loaded {importSettings.filepath} version {voxFile.VersionNumber}");
            Debug.Log($"Loaded models:{voxFile.Models.Length} chunks:{voxFile.Chunks.Length} colors:{voxFile.Palette.Colors.Length}");

            int chunkRes = importSettings.chunkResolution;
            IPalette palette = voxFile.Palette;

            VoxelRoomData[] roomsSaveData = new VoxelRoomData[voxFile.Models.Length];
            for (int i = 0; i < voxFile.Models.Length; i++) {
                IModel model = voxFile.Models[i];
                Debug.Log($"model[{i}]: {model}");
                Vector3Int modelSize = ConvertVecI(model.Size);
                // get num chunks to make
                // 0/4=0 1/4=1 2/4=1 3/4=1 4/4=1 5/4=2
                Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
                int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
                RawChunkData[] rawChunks = new RawChunkData[numChunksTotal];
                for (int rci = 0; rci < rawChunks.Length; rci++) {
                    rawChunks[rci] = new RawChunkData() {
                        rawVoxels = new RawVoxel[chunkRes * chunkRes * chunkRes],
                    };
                }
                for (int v = 0; v < model.Voxels.Length; v++) {
                    VoxReader.Voxel voxel = model.Voxels[i];
                    Vector3Int voxelpos = ConvertVecI(voxel.Position);
                    Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
                    Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
                    int chunkIndex =
                        chunkpos.y / numChunksPerDir.y * chunkRes * chunkRes +
                        chunkpos.z / numChunksPerDir.z * chunkRes +
                        chunkpos.x / numChunksPerDir.x;
                    RawChunkData rawChunkData = rawChunks[chunkIndex];
                    rawChunkData.chunkPos = chunkpos;
                    // int id = palette.Colors.ToList().IndexOf(voxel.Color);
                    UnityEngine.Color color = ConvertColor(voxel.Color);
                    int matid = voxel.ColorIndex / 8 + 1;
                    rawChunkData.rawVoxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new RawVoxel() {
                        materialId = matid,
                        materialColor = color
                    };
                }
                roomsSaveData[i] = new VoxelRoomData() {
                    // id = 
                    // offsetint = 
                    rawChunks = rawChunks
                };
            }
            return roomsSaveData;
        }
        static Vector3Int ConvertVecI(VoxReader.Vector3 vec3) => new Vector3Int(vec3.X, vec3.Y, vec3.Z);
        // todo VoxReader colors use bytes instead of floats
        static UnityEngine.Color ConvertColor(VoxReader.Color color) => new UnityEngine.Color(color.R, color.G, color.B, color.A);
    }
}