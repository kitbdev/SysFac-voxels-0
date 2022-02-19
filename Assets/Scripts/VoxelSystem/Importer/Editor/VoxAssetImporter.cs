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
            VoxelImporter.Load(voxelImportSettings);
            if (asPrefab) {

                GameObject maingo = new GameObject("voxel load");
                VoxelWorld voxelWorld = maingo.AddComponent<VoxelWorld>();
                voxelWorld.voxelSize = 0.5f;
                voxelWorld.materialSet =
                    AssetDatabase.LoadAssetAtPath<VoxelMaterialSetSO>("Assets/Data/defVoxelMaterialSet.asset");
                FixedChunkLoader fixedChunkLoader = maingo.AddComponent<FixedChunkLoader>();
                fixedChunkLoader.SetChunksToLoadOrigin();
                // voxelWorld.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
                voxelWorld.ReloadPool();
                fixedChunkLoader.LoadChunks();
                // voxelWorld.set
                // maingo.AddComponent<VoxelRenderer>()
                string filename = System.IO.Path.GetFileName(ctx.assetPath);
                ctx.AddObjectToAsset($"voxel {filename} prefab", maingo);
                ctx.SetMainObject(maingo);
            } else {
                ImportedVoxelData importedVoxelData = ScriptableObject.CreateInstance<ImportedVoxelData>();
                ctx.AddObjectToAsset("imported data asset", importedVoxelData);
                ctx.SetMainObject(importedVoxelData);
            }
        }
    }
}