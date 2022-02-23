using System.Collections.Generic;
using System.Linq;
using FileToVoxCore.Vox;
using UnityEngine;

namespace VoxelSystem.Importer.VoxCore {

    public class VoxCoreImporter : MonoBehaviour {
        static bool debug = true;

        public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
            debug = importSettings.debugMode;
            VoxReader voxReader = new VoxReader();
            VoxModel model = voxReader.LoadModel(importSettings.filepath);
            if (model == null) return null;
            if (debug) Debug.Log($"Loaded VoxCore {model} vf:{model.VoxelFrames.Count} tn:{model.TransformNodeChunks.Count}");
            FullVoxelImportData fullVoxelImportData = new FullVoxelImportData();
            fullVoxelImportData.chunkResolution = importSettings.chunkResolution;
            fullVoxelImportData.voxelSize = importSettings.voxelSize;
            fullVoxelImportData.models = new VoxelModelImportData[0];

            if (debug) {
                int i = 0;
                string palmap = model.PaletteColorIndex?.Aggregate("", (s, v) => {
                    i++;
                    if (i == v) {
                        return s;
                    } else {
                        return s += $"{i}:{v},";
                    }
                });
                Debug.Log($"PaletteColorIndex:[{palmap}]");
            }
            // model.PaletteColorIndex.to
            List<VoxelModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.models.ToList();
            for (int i = 0; i < model.VoxelFrames.Count; i++) {
                FileToVoxCore.Vox.Chunks.TransformNodeChunk transformNodeChunk = model.TransformNodeChunks[i + 1];

                VoxelModelImportData voxelModelImportData = LoadModel(model.VoxelFrames[i], transformNodeChunk, importSettings);
                // convert mat id to use correct index
                if (importSettings.applyPaletteIndexCorrection) {
                    foreach (var chunk in voxelModelImportData.chunks) {
                        for (int v = 0; v < chunk.voxels.Length; v++) {
                            chunk.voxels[v].materialId = model.PaletteColorIndex[chunk.voxels[v].materialId] - 1;
                        }
                    }
                }
                if (voxelModelImportData != null) {
                    voxelRoomModelImportDatas.Add(voxelModelImportData);
                }
            }
            fullVoxelImportData.models = voxelRoomModelImportDatas.ToArray();
            return fullVoxelImportData;
        }


        static VoxelModelImportData LoadModel(FileToVoxCore.Vox.VoxelData vdata,
            FileToVoxCore.Vox.Chunks.TransformNodeChunk tnode,
            VoxelImportSettings importSettings) {
            var chunkRes = importSettings.chunkResolution;
            Vector3Int modelSize = new Vector3Int(vdata.VoxelsWide, vdata.VoxelsDeep, vdata.VoxelsTall);
            Vector3Int worldPos = ConvertI(tnode.TranslationAt()) - modelSize / 2;
            // if (worldPositionFrame == FileToVoxCore.Schematics.Tools.Vector3.zero)
            //     return null;
            string modelName = tnode.Name;
            if (debug) Debug.Log($"Load Model '{modelName}' size:{modelSize} wp:{worldPos}");
            Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
            Vector3Int startChunkPos = Vector3Int.zero;
            Vector3Int localOffset = Vector3Int.zero;
            var worldChunkAlignment = importSettings.worldChunkAlignment;
            if (worldChunkAlignment) {
                startChunkPos = VoxelWorld.ChunkPosWithBlock(worldPos, chunkRes);
                var endChunkPos = VoxelWorld.ChunkPosWithBlock(worldPos + modelSize, chunkRes) + Vector3Int.one;
                numChunksPerDir = (endChunkPos - startChunkPos);
                // localOffset = new Vector3Int(
                //     (worldPos.x % chunkRes),
                //     (worldPos.y % chunkRes),
                //     (worldPos.z % chunkRes)
                //     );
                // numChunksPerDir = Vector3Int.CeilToInt(new Vector3(
                //     (modelSize.x - 0f + localOffset.x) / chunkRes,
                //     (modelSize.y - 0f + localOffset.y) / chunkRes,
                //     (modelSize.z - 0f + localOffset.z) / chunkRes
                //     ));
            }
            int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
            ChunkImportData[] chunks = new ChunkImportData[numChunksTotal];
            for (int y = 0, rci = 0; y < numChunksPerDir.y; y++) {
                for (int z = 0; z < numChunksPerDir.z; z++) {
                    for (int x = 0; x < numChunksPerDir.x; x++, rci++) {
                        // for (int rci = 0; rci < chunks.Length; rci++) {
                        chunks[rci] = new ChunkImportData() {
                            voxels = new ImportedVoxel[chunkRes * chunkRes * chunkRes],
                            chunkPos = startChunkPos + new Vector3Int(x, y, z),
                        };
                    }
                }
            }
            if (debug) Debug.Log($"model size {modelSize} ncpd:{numChunksPerDir} numchunkstotal:{numChunksTotal} chunkres:{chunkRes} chst:{startChunkPos} wp:{worldPos}");

            for (int x = 0; x < modelSize.x; x++) {
                for (int z = 0; z < modelSize.z; z++) {
                    for (int y = 0; y < modelSize.y; y++) {

                        Vector3Int voxelpos = new Vector3Int(x, y, z);
                        var blockpos = voxelpos;
                        if (worldChunkAlignment) {
                            blockpos += worldPos;
                            // voxelpos += localOffset;
                        }
                        // todo offset this if not useing worldschunk alignment?
                        Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(blockpos, chunkRes);
                        Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(blockpos, chunkpos, chunkRes);
                        int chunkIndex =
                            chunks.ToList().FindIndex(c => c.chunkPos == chunkpos);
                        // (chunkpos.y / numChunksPerDir.y) * chunkRes * chunkRes +
                        // (chunkpos.z / numChunksPerDir.z) * chunkRes +
                        // (chunkpos.x / numChunksPerDir.x);
                        if (chunkIndex == -1) {
                            Debug.LogError($"Vox importer failed to get chunk index {chunkIndex} from cp:{chunkpos} ncpd:{numChunksPerDir} vp{voxelpos} lp{localpos} bp:{blockpos} chunkres{chunkRes}");
                            return default;
                        }
                        ChunkImportData chunkData = chunks[chunkIndex];
                        //({chunkData.chunkPos})
                        // chunkData.chunkPos = chunkpos;
                        // int index = x + y * chunkRes + z * chunkRes * chunkRes;
                        // if (debug && !vdata.ContainsKey(x, y, z)) {
                        //     Debug.Log($"cannot find {x},{y},{z} gp:{vdata.GetGridPos(x, y, z)}");
                        // }
                        // model size is already swiched so this gives accurate values
                        int matid = vdata.GetSafe(x, z, y);
                        // if (matid != 0) {
                        // Debug.Log($"found {chunkpos} {voxelpos} is {matid} ({data_index})");
                        // }
                        if (VoxelChunk.IndexAt(localpos, chunkRes) < 0) {
                            Debug.LogError($"Vox importer failed to get voxel id of ci:{chunkIndex} cp:{chunkpos} ncpd:{numChunksPerDir} vp:{voxelpos} lp:{localpos} bp:{blockpos} chunkres:{chunkRes}");
                            return default;
                        }
                        chunkData.voxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new ImportedVoxel() {
                            materialId = matid,
                            // materialColor = color
                        };
                    }
                }
            }
            Matrix4x4 matrix4x4 = Convert(FileToVoxCore.Utils.VoxUtils.ReadMatrix4X4FromRotation(
                tnode.RotationAt(), tnode.TranslationAt()));
            var modelImportData = new VoxelModelImportData() {
                // id = 
                modelName = modelName,
                modelSize = modelSize,
                position = worldPos,
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