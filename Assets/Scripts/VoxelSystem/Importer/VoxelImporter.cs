using UnityEngine;
using VoxReader;

namespace VoxelSystem {
    public class VoxelImporter : MonoBehaviour {
        public static void Load(VoxelWorld world, string filepath) {
            Kutil.SaveSystem.StartLoad()
                            .InCustomFullPath(filepath)
                            .As(Kutil.SaveSystem.SaveBuilder.SerializeType.BINARY)
                            .TryLoadBin(out var bytes);
            Load(world, bytes);
        }
        public static void Load(VoxelWorld world, byte[] filedata) {
            VoxReader.Interfaces.IVoxFile voxFile = VoxReader.VoxReader.Read(filedata);
            if (voxFile == null) {
                Debug.LogError($"Failed to load voxels ''");
                return;
            }
            Debug.Log($"Loaded version {voxFile.VersionNumber}");
            Debug.Log($"Loaded models:{voxFile.Models.Length} chunks:{voxFile.Chunks.Length} colors:{voxFile.Palette.Colors.Length}");
            for (int i = 0; i < voxFile.Models.Length; i++) {
                VoxReader.Interfaces.IModel model = voxFile.Models[i];
                Debug.Log($"model[{i}]: {model}");
                Vector3Int vector3Int = ToUnityUnit(model.Size);
                //todo cont get num chunks to make
                for (int v = 0; v < model.Voxels.Length; v++) {
                    VoxReader.Voxel voxel = model.Voxels[v];
                    //
                }
            }
            // VoxelWorld.ChunkSaveData[] chunkSaveDatas;
            // todo 
            // world.LoadChunksFromData(chunkSaveDatas);
        }
        static Vector3Int ToUnityUnit(VoxReader.Vector3 vec3) => new Vector3Int(vec3.X, vec3.Y, vec3.Z);
    }
}