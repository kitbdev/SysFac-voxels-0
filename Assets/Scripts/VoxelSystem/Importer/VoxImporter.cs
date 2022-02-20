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
        public VoxelRoomModelImportData currentRoom;
        public int chunkRes => importSettings.chunkResolution;

        public VoxLoader(VoxelImportSettings importSettings) {
            this.importSettings = importSettings;
            fullVoxelImportData = new FullVoxelImportData();
            currentRoom = new VoxelRoomModelImportData();
            fullVoxelImportData.chunkResolution = chunkRes;
            fullVoxelImportData.voxelSize = importSettings.voxelSize;
            fullVoxelImportData.rooms = new VoxelRoomModelImportData[0];
        }
        // void NewRoom() {
        //     List<VoxelRoomModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.rooms.ToList();
        //     voxelRoomModelImportDatas.Add(currentRoom);
        //     currentRoom = new VoxelRoomModelImportData();
        //     fullVoxelImportData.rooms = voxelRoomModelImportDatas.ToArray();
        // }

        public void LoadModel(int sizeX, int sizeY, int sizeZ, byte[,,] data) {
            Vector3Int modelSize = new Vector3Int(sizeX, sizeZ, sizeY);
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
            for (int z = 0; z < sizeZ; z++) {
                for (int y = 0; y < sizeY; y++) {
                    for (int x = 0; x < sizeX; x++) {
                        Vector3Int voxelpos = new Vector3Int(x, z, y);
                        Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
                        Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
                        int chunkIndex =
                            // (chunkpos.y / numChunksPerDir.y) * chunkRes * chunkRes +
                            // (chunkpos.z / numChunksPerDir.z) * chunkRes +
                            // (chunkpos.x / numChunksPerDir.x);
                            chunks.ToList().FindIndex(c => c.chunkPos == chunkpos);
                        ChunkImportData chunkData = chunks[chunkIndex];
                        // Debug.Log($"{chunkIndex} from {chunkpos} ({chunkData.chunkPos}) ncpd{numChunksPerDir} vp{voxelpos} lp{localpos} chunkres{chunkRes}");
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
            var roomModelImportData = new VoxelRoomModelImportData() {
                // id = 
                // offsetint = 
                numChunksByAxis = numChunksPerDir,
                chunks = chunks
            };
            List<VoxelRoomModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.rooms.ToList();
            voxelRoomModelImportDatas.Add(roomModelImportData);
            fullVoxelImportData.rooms = voxelRoomModelImportDatas.ToArray();
        }

        public void LoadPalette(uint[] palette) {

        }

        public void NewGroupNode(int id, Dictionary<string, byte[]> attributes, int[] childrenIds) {

        }

        public void NewLayer(int id, string name, Dictionary<string, byte[]> attributes) {

        }

        public void NewMaterial(int id, Dictionary<string, byte[]> attributes) {

        }

        public void NewShapeNode(int id, Dictionary<string, byte[]> attributes, int[] modelIds, Dictionary<string, byte[]>[] modelsAttributes) {

        }

        public void NewTransformNode(int id, int childNodeId, int layerId, string name, Dictionary<string, byte[]>[] framesAttributes) {

        }

        public void SetMaterialOld(int paletteId, MaterialOld.MaterialTypes type, float weight, MaterialOld.PropertyBits property, float normalized) {

        }

        public void SetModelCount(int count) {

        }
    }
}