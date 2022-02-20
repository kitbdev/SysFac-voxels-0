using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using VoxReader;
// using VoxReader.Interfaces;

namespace VoxelSystem.Importer {
    
    public class VoxelImporter : MonoBehaviour {
        // public static void Load(VoxelWorld world, string filepath) {
        // Kutil.SaveSystem.StartLoad()
        //                 .InCustomFullPath(filepath)
        //                 .As(Kutil.SaveSystem.SaveBuilder.SerializeType.BINARY)
        //                 .TryLoadBin(out var bytes);
        // Load(world, bytes);
        // }
        // public static Vector3Int GetNumChunksAxis(VoxelImportSettings importSettings) {
        //     if (importSettings.voxelSize <= 0 ||
        //         importSettings.chunkResolution <= 0 ||
        //         importSettings.filepath == null || importSettings.filepath == "") {
        //         // Debug.LogError($"Invalid import settings {importSettings} for .vox importer");
        //     }
        //     IVoxFile voxFile = VoxReader.VoxReader.Read(importSettings.filepath);
        //     if (voxFile == null) {
        //         // Debug.LogError($"Failed to load voxels '{importSettings.filepath}'");
        //         return default;
        //     }

        //     int chunkRes = importSettings.chunkResolution;
        //     IPalette palette = voxFile.Palette;

        //     VoxelRoomModelImportData[] roomsSaveData = new VoxelRoomModelImportData[voxFile.Models.Length];
        //     for (int i = 0; i < voxFile.Models.Length;) {
        //         IModel model = voxFile.Models[i];
        //         Debug.Log($"model[{i}]: {model}");
        //         Vector3Int modelSize = ConvertVecI(model.Size);
        //         // get num chunks to make
        //         // 0/4=0 1/4=1 2/4=1 3/4=1 4/4=1 5/4=2
        //         Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
        //         return numChunksPerDir;
        //     }
        //     return default;
        // }
        // public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
        //     if (importSettings.voxelSize <= 0 ||
        //         importSettings.chunkResolution <= 0 ||
        //         importSettings.filepath == null || importSettings.filepath == "") {
        //         Debug.LogError($"Invalid import settings {importSettings} for .vox importer");
        //     }
        //     IVoxFile voxFile = VoxReader.VoxReader.Read(importSettings.filepath);
        //     if (voxFile == null) {
        //         Debug.LogError($"Failed to load voxels '{importSettings.filepath}'");
        //         return default;
        //     }
        //     // Debug.Log($"Loaded {importSettings.filepath} version {voxFile.VersionNumber}");
        //     // Debug.Log($"Loaded models:{voxFile.Models.Length} chunks:{voxFile.Chunks.Length} colors:{voxFile.Palette.Colors.Length}");

        //     int chunkRes = importSettings.chunkResolution;
        //     IPalette palette = voxFile.Palette;

        //     VoxelRoomModelImportData[] roomsSaveData = new VoxelRoomModelImportData[voxFile.Models.Length];
        //     for (int i = 0; i < voxFile.Models.Length; i++) {
        //         IModel model = voxFile.Models[i];
        //         // Debug.Log($"model[{i}]: {model}");
        //         Vector3Int modelSize = ConvertVecI(model.Size);
        //         // get num chunks to make
        //         // 0/4=0 1/4=1 2/4=1 3/4=1 4/4=1 5/4=2
        //         Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
        //         int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
        //         ChunkImportData[] chunks = new ChunkImportData[numChunksTotal];
        //         for (int rci = 0; rci < chunks.Length; rci++) {
        //             chunks[rci] = new ChunkImportData() {
        //                 voxels = new ImportedVoxel[chunkRes * chunkRes * chunkRes],
        //             };
        //         }
        //         for (int v = 0; v < model.Voxels.Length; v++) {
        //             VoxReader.Voxel voxel = model.Voxels[i];
        //             Vector3Int voxelpos = ConvertVecI(voxel.Position);
        //             if (voxelpos == Vector3Int.zero) continue;
        //             Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
        //             Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
        //             int chunkIndex = Mathf.RoundToInt(
        //                 (((float)chunkpos.y) / numChunksPerDir.y) * chunkRes * chunkRes +
        //                 (((float)chunkpos.z) / numChunksPerDir.z) * chunkRes +
        //                 (((float)chunkpos.x) / numChunksPerDir.x));
        //             // if (v < 50)
        //                 Debug.Log($"{chunkIndex} from {chunkpos} ncpd{numChunksPerDir} vp{voxel} lp{localpos} chunkres{chunkRes}");
        //             ChunkImportData chunkData = chunks[chunkIndex];
        //             chunkData.chunkPos = chunkpos;
        //             // int id = palette.Colors.ToList().IndexOf(voxel.Color);
        //             UnityEngine.Color color = ConvertColor(voxel.Color);
        //             int matid = voxel.ColorIndex;// / 8 + 1;
        //             chunkData.voxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new ImportedVoxel() {
        //                 materialId = matid,
        //                 // materialColor = color
        //             };
        //         }
        //         roomsSaveData[i] = new VoxelRoomModelImportData() {
        //             // id = 
        //             // offsetint = 
        //             numChunksByAxis = numChunksPerDir,
        //             chunks = chunks
        //         };
        //     }
        //     FullVoxelImportData fullVoxelImportData = new FullVoxelImportData() {
        //         chunkResolution = chunkRes,
        //         voxelSize = importSettings.voxelSize,
        //         rooms = roomsSaveData,
        //     };
        //     return fullVoxelImportData;
        // }
        // static Vector3Int ConvertVecI(VoxReader.Vector3 vec3) => new Vector3Int(vec3.X, vec3.Y, vec3.Z);
        // // todo VoxReader colors use bytes instead of floats
        // static UnityEngine.Color ConvertColor(VoxReader.Color color) => new UnityEngine.Color(color.R, color.G, color.B, color.A);
    }
}