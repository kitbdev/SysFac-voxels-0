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
        // [ReadOnly]
        // [SerializeField]
        FullVoxelImportData fullVoxelImportData;
        [Header("Settings")]
        public VoxelImportSettings voxelImportSettings = new VoxelImportSettings();
        public bool asPrefab = false;
        public bool areRoomsLimbs = false;

        public override void OnImportAsset(AssetImportContext ctx) {
            voxelImportSettings ??= new VoxelImportSettings();
            voxelImportSettings.filepath = ctx.assetPath;
            fullVoxelImportData = VoxelImporter.Load(voxelImportSettings);
            if (fullVoxelImportData == null) {
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
                voxelWorld.enableCollision = false;
                voxelWorld.LoadFullImportVoxelData(fullVoxelImportData);
                // fixedChunkLoader.LoadChunks();
                // voxelWorld.set
                // maingo.AddComponent<VoxelRenderer>()
                ctx.AddObjectToAsset($"voxel {filename} prefab", maingo);
                ctx.SetMainObject(maingo);
            } else {
                ImportedVoxelData importedVoxelData = ScriptableObject.CreateInstance<ImportedVoxelData>();
                importedVoxelData.modelName = filename;
                importedVoxelData.fullVoxelImportData = fullVoxelImportData;
                ctx.AddObjectToAsset("imported vox asset", importedVoxelData);
                ctx.SetMainObject(importedVoxelData);
            }
        }
    }
}