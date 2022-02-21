using System.Collections.Generic;
using System.Linq;
using FileToVoxCore.Vox;
using UnityEngine;

namespace VoxelSystem.Importer.VoxCore {

    public class VoxCoreImporter : MonoBehaviour {
        static bool debug = false;

        public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
            VoxReader voxReader = new VoxReader();
            VoxModel model = voxReader.LoadModel(importSettings.filepath);
            if (model == null) return null;
            if (debug) Debug.Log($"Loaded VoxCore {model} vf:{model.VoxelFrames.Count} tn:{model.TransformNodeChunks.Count}");
            FullVoxelImportData fullVoxelImportData = new FullVoxelImportData();
            fullVoxelImportData.chunkResolution = importSettings.chunkResolution;
            fullVoxelImportData.voxelSize = importSettings.voxelSize;
            fullVoxelImportData.models = new VoxelModelImportData[0];
            List<VoxelModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.models.ToList();
            for (int i = 0; i < model.VoxelFrames.Count; i++) {
                FileToVoxCore.Vox.Chunks.TransformNodeChunk transformNodeChunk = model.TransformNodeChunks[i + 1];

                VoxelModelImportData voxelModelImportData = LoadModel(model.VoxelFrames[i], transformNodeChunk, importSettings);
                if (voxelModelImportData != null) {
                    voxelRoomModelImportDatas.Add(voxelModelImportData);
                }
            }
            fullVoxelImportData.models = voxelRoomModelImportDatas.ToArray();
            return fullVoxelImportData;
        }

        static VoxelModelImportData LoadModel(FileToVoxCore.Vox.VoxelData vdata,
            FileToVoxCore.Vox.Chunks.TransformNodeChunk tra,
            VoxelImportSettings importSettings) {
            var chunkRes = importSettings.chunkResolution;
            Vector3Int modelSize = new Vector3Int(vdata.VoxelsWide, vdata.VoxelsDeep, vdata.VoxelsTall);
            Vector3 worldPositionFrame = Convert(tra.TranslationAt());
            // if (worldPositionFrame == FileToVoxCore.Schematics.Tools.Vector3.zero)
            //     return null;
            if (debug) Debug.Log($"LoadModel size {modelSize} wp:{worldPositionFrame}");
            Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
            int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
            ChunkImportData[] chunks = new ChunkImportData[numChunksTotal];
            for (int y = 0, rci = 0; y < numChunksPerDir.y; y++) {
                for (int z = 0; z < numChunksPerDir.z; z++) {
                    for (int x = 0; x < numChunksPerDir.x; x++, rci++) {
                        // for (int rci = 0; rci < chunks.Length; rci++) {
                        chunks[rci] = new ChunkImportData() {
                            voxels = new ImportedVoxel[chunkRes * chunkRes * chunkRes],
                            chunkPos = new Vector3Int(x, y, z),
                        };
                    }
                }
            }
            if (debug) Debug.Log($"model size {modelSize} ncpd:{numChunksPerDir} numchunkstotal:{numChunksTotal} chunkres:{chunkRes}");

            for (int x = 0; x < modelSize.x; x++) {
                for (int z = 0; z < modelSize.z; z++) {
                    for (int y = 0; y < modelSize.y; y++) {

                        Vector3Int voxelpos = new Vector3Int(x, y, z);
                        Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
                        Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
                        int chunkIndex =
                            // (chunkpos.y / numChunksPerDir.y) * chunkRes * chunkRes +
                            // (chunkpos.z / numChunksPerDir.z) * chunkRes +
                            // (chunkpos.x / numChunksPerDir.x);
                            chunks.ToList().FindIndex(c => c.chunkPos == chunkpos);
                        if (chunkIndex == -1) {
                            Debug.LogError($"Vox importer failed to get chunk index {chunkIndex} from cp:{chunkpos} ncpd:{numChunksPerDir} vp{voxelpos} lp{localpos} chunkres{chunkRes}");
                            return default;
                        }
                        ChunkImportData chunkData = chunks[chunkIndex];
                        //({chunkData.chunkPos})
                        // chunkData.chunkPos = chunkpos;
                        // int index = x + y * chunkRes + z * chunkRes * chunkRes;
                        // if (debug && !vdata.ContainsKey(x, y, z)) {
                        //     Debug.Log($"cannot find {x},{y},{z} gp:{vdata.GetGridPos(x, y, z)}");
                        // }
                        int matid = vdata.GetSafe(x, z, y);
                        // if (matid != 0) {
                        // Debug.Log($"found {chunkpos} {voxelpos} is {matid} ({data_index})");
                        // }
                        chunkData.voxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new ImportedVoxel() {
                            materialId = matid,
                            // materialColor = color
                        };
                    }
                }
            }
            Matrix4x4 matrix4x4 = Convert(FileToVoxCore.Utils.VoxUtils.ReadMatrix4X4FromRotation(
                tra.RotationAt(), tra.TranslationAt()));
            var modelImportData = new VoxelModelImportData() {
                // id = 
                modelSize = modelSize,
                position = Vector3Int.FloorToInt(worldPositionFrame),
                trMatrix = matrix4x4,
                numChunksByAxis = numChunksPerDir,
                chunks = chunks
            };
            return modelImportData;
        }
        static Vector3 Convert(FileToVoxCore.Schematics.Tools.Vector3 vector3) {
            return new Vector3(vector3.X, vector3.Z, vector3.Y);
        }
        static Vector4 Convert(FileToVoxCore.Schematics.Tools.Vector4 vector4) {
            return new Vector4(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }
        static Vector3Int ConvertI(FileToVoxCore.Schematics.Tools.Vector3 vector3) {
            return Vector3Int.FloorToInt(Convert(vector3));
        }
        static Matrix4x4 Convert(FileToVoxCore.Schematics.Tools.Matrix4x4 matrix) {
            return new Matrix4x4(
                Convert(matrix.GetColumn(0)),
                Convert(matrix.GetColumn(1)),
                Convert(matrix.GetColumn(2)),
                Convert(matrix.GetColumn(3))
            );
        }
    }
}