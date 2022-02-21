using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CsharpVoxReader;
using CsharpVoxReader.Chunks;

namespace VoxelSystem.Importer {
    public class VoxImporter {
        public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
            VoxLoader loader = new VoxLoader(importSettings);
            CsharpVoxReader.VoxReader voxReader = new CsharpVoxReader.VoxReader(importSettings.filepath, loader);
            voxReader.Read();
            return loader.fullVoxelImportData;
        }
    }
    public class VoxLoader : IVoxLoader {
        public VoxelImportSettings importSettings;
        public FullVoxelImportData fullVoxelImportData;
        public VoxelModelImportData currentRoom;
        public int chunkRes => importSettings.chunkResolution;
        public Vector3Int latestPos;
        bool debug = false;

        public VoxLoader(VoxelImportSettings importSettings) {
            if (debug) Debug.Log($"New VoxLoader");
            this.importSettings = importSettings;
            fullVoxelImportData = new FullVoxelImportData();
            currentRoom = new VoxelModelImportData();
            fullVoxelImportData.chunkResolution = chunkRes;
            fullVoxelImportData.voxelSize = importSettings.voxelSize;
            fullVoxelImportData.models = new VoxelModelImportData[0];
            latestPos = Vector3Int.zero;
        }
        // void NewRoom() {
        //     List<VoxelRoomModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.rooms.ToList();
        //     voxelRoomModelImportDatas.Add(currentRoom);
        //     currentRoom = new VoxelRoomModelImportData();
        //     fullVoxelImportData.rooms = voxelRoomModelImportDatas.ToArray();
        // }

        public void LoadModel(int sizeX, int sizeY, int sizeZ, byte[,,] data) {
            if (debug) Debug.Log($"LoadModel {sizeX},{sizeY},{sizeZ}");
            Vector3Int modelSize = new Vector3Int(sizeX, sizeY, sizeZ);
            // get voxels
            // ImportedVoxel[] voxels = new ImportedVoxel[sizeX * sizeY * sizeZ];
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
            // Debug.Log($"model size {modelSize} ncpd{numChunksPerDir} numchunkst{numChunksTotal} chunkres{chunkRes}");
            for (int z = 0; z < sizeZ; z++) {
                for (int y = 0; y < sizeY; y++) {
                    for (int x = 0; x < sizeX; x++) {
                        Vector3Int voxelpos = new Vector3Int(x, y, z);
                        Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
                        Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
                        int chunkIndex =
                            // (chunkpos.y / numChunksPerDir.y) * chunkRes * chunkRes +
                            // (chunkpos.z / numChunksPerDir.z) * chunkRes +
                            // (chunkpos.x / numChunksPerDir.x);
                            chunks.ToList().FindIndex(c => c.chunkPos == chunkpos);
                        if (chunkIndex == -1) {
                            Debug.Log($"Vox importer failed to get chunk index {chunkIndex} from cp:{chunkpos} ncpd:{numChunksPerDir} vp{voxelpos} lp{localpos} chunkres{chunkRes}");
                            return;
                        }
                        ChunkImportData chunkData = chunks[chunkIndex];
                        //({chunkData.chunkPos})
                        // chunkData.chunkPos = chunkpos;
                        int matid = (int)data[x, y, z];
                        // if (matid != 0) {
                        //     Debug.Log($"found {chunkpos} {voxelpos} is {matid}");
                        // }
                        // int index = x + y * chunkRes + z * chunkRes * chunkRes;
                        // voxels[index] = new ImportedVoxel() { materialId = (int)data[x, y, z] };
                        chunkData.voxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new ImportedVoxel() {
                            materialId = matid,
                            // materialColor = color
                        };
                    }
                }
            }
            var roomModelImportData = new VoxelModelImportData() {
                // id = 
                modelSize = modelSize,
                position = latestPos,
                numChunksByAxis = numChunksPerDir,
                chunks = chunks
            };
            List<VoxelModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.models.ToList();
            voxelRoomModelImportDatas.Add(roomModelImportData);
            fullVoxelImportData.models = voxelRoomModelImportDatas.ToArray();
        }

        public void LoadPalette(uint[] palette) {
            if (debug) Debug.Log($"Load palette {palette.Length}");
        }

        public void NewGroupNode(int id, Dictionary<string, byte[]> attributes, int[] childrenIds) {
            if (debug) Debug.Log($"New Group Node {id}");
        }

        public void NewLayer(int id, string name, Dictionary<string, byte[]> attributes) {
            if (debug) Debug.Log($"New Layer {id}");
        }

        public void NewMaterial(int id, Dictionary<string, byte[]> attributes) {
            if (debug) Debug.Log($"New Material {id}");
        }

        public void NewShapeNode(int id, Dictionary<string, byte[]> attributes, int[] modelIds, Dictionary<string, byte[]>[] modelsAttributes) {
            if (debug) Debug.Log($"New ShapeNode {id}");
        }

        public void NewTransformNode(int id, int childNodeId, int layerId, string name, Dictionary<string, byte[]>[] framesAttributes, TransformNodeFrameData[] transformNodeFrameDatas) {
            if (debug) Debug.Log($"NewTransformNode {id} '{name}' n:{transformNodeFrameDatas.Length}");
            for (int i = 0; i < transformNodeFrameDatas.Length; i++) {
                if (debug) {
                    TransformNodeFrameData trnode = transformNodeFrameDatas[i];
                    Vector3 translate = new Vector3(
                        trnode.translationVector[0],
                        trnode.translationVector[1],
                        trnode.translationVector[2]);
                    Vector3Int rotation1 = new Vector3Int(
                        trnode.rotationMatrix[0],
                        trnode.rotationMatrix[1],
                        trnode.rotationMatrix[2]);
                    Vector3Int rotation2 = new Vector3Int(
                        trnode.rotationMatrix[3],
                        trnode.rotationMatrix[4],
                        trnode.rotationMatrix[5]);
                    Vector3Int rotation3 = new Vector3Int(
                        trnode.rotationMatrix[6],
                        trnode.rotationMatrix[7],
                        trnode.rotationMatrix[8]);

                    Debug.Log($"Transform {i} frame:{trnode.frameIndex} translate:{translate} rotation:{rotation1},{rotation2},{rotation3}");
                }
            }
        }

        public void SetMaterialOld(int paletteId, MaterialOld.MaterialTypes type, float weight, MaterialOld.PropertyBits property, float normalized) {
            if (debug) Debug.Log($"Set Material old {paletteId}");

        }

        public void SetModelCount(int count) {
            if (debug) Debug.Log($"Set Model count {count}");
        }
    }
}