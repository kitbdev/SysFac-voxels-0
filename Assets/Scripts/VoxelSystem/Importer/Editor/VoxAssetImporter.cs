using Kutil;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using VoxReader;

namespace VoxelSystem.Importer {
    /// <summary>
    /// Imports MagicaVoxel .vox files
    /// </summary>
    [ScriptedImporter(1, "vox")]
    public class VoxAssetImporter : ScriptedImporter {

        [Header("Data")]
        [ReadOnly]
        public int numChunks = 0;
        [Header("Settings")]
        public VoxelImportSettings voxelImportSettings;
        public bool asPrefab = true;
        public bool areRoomsLimbs = false;

        public override void OnImportAsset(AssetImportContext ctx) {

            voxelImportSettings.filepath = ctx.assetPath;
            VoxelRoomData[] voxelRoomDatas = VoxelImporter.Load(voxelImportSettings);
            if (voxelRoomDatas == null || voxelRoomDatas.Length < 1) {
                // load fail
                Debug.LogError($"Failed to load {ctx.assetPath} vox info");
                return;
            }
            string filename = System.IO.Path.GetFileName(ctx.assetPath);
            if (asPrefab) {

                GameObject maingo = new GameObject("voxel load");
                VoxelWorld voxelWorld = maingo.AddComponent<VoxelWorld>();
                voxelWorld.voxelSize = 0.5f;
                voxelWorld.materialSet =
                    AssetDatabase.LoadAssetAtPath<VoxelMaterialSetSO>("Assets/Data/defVoxelMaterialSet.asset");
                // FixedChunkLoader fixedChunkLoader = maingo.AddComponent<FixedChunkLoader>();
                // fixedChunkLoader.SetChunksToLoadOrigin();
                // voxelWorld.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
                
                voxelWorld.LoadRoomFromData(voxelRoomDatas);
                // fixedChunkLoader.LoadChunks();
                // voxelWorld.set
                // maingo.AddComponent<VoxelRenderer>()
                ctx.AddObjectToAsset($"voxel {filename} prefab", maingo);
                ctx.SetMainObject(maingo);
            } else {
                ImportedVoxelData importedVoxelData = ScriptableObject.CreateInstance<ImportedVoxelData>();
                importedVoxelData.modelName = filename;
                importedVoxelData.roomsData = voxelRoomDatas;
                ctx.AddObjectToAsset("imported vox asset", importedVoxelData);
                ctx.SetMainObject(importedVoxelData);
            }
        }
    }
}