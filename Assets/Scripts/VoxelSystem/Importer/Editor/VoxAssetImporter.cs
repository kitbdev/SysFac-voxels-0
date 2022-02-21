using System.Collections.Generic;
using Kutil;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace VoxelSystem.Importer {
    /// <summary>
    /// Imports MagicaVoxel .vox files
    /// </summary>
    [ScriptedImporter(1, "vox")]
    public class VoxAssetImporter : ScriptedImporter {

        [Header("Data")]
        [ReadOnly]
        [SerializeField] int numModels;
        [SerializeField] OgtVox.OgtVoxImporter.ogt_vox_scene scene;
        [System.Serializable]
        struct Models {
            public Vector3Int modelSize;
            public Vector3Int offset;
            public Vector3Int chunkCountAxis;
            public int numChunks;
        }
        [ReadOnly]
        [SerializeField] List<Models> models = new List<Models>();
        // [SerializeField]
        FullVoxelImportData fullVoxelImportData;
        [Header("Settings")]
        public VoxelImportSettings voxelImportSettings = new VoxelImportSettings();
        public bool asPrefab = false;
        public bool areRoomsLimbs = false;

        public override void OnImportAsset(AssetImportContext ctx) {
            voxelImportSettings ??= new VoxelImportSettings();
            voxelImportSettings.filepath = ctx.assetPath;
            // scene = OgtVox.OgtVoxImporter.GetScene(voxelImportSettings);
            // return;
            // fullVoxelImportData = OgtVox.OgtVoxImporter.Load(voxelImportSettings);
            fullVoxelImportData = VoxImporter.Load(voxelImportSettings);
            if (fullVoxelImportData == null) {
                // load fail
                Debug.LogError($"Failed to load {ctx.assetPath} vox info");
                return;
            }
            numModels = fullVoxelImportData.models.Length;
            models.Clear();
            for (int i = 0; i < numModels; i++) {
                models.Add(new Models() {
                    modelSize = fullVoxelImportData.models[i].modelSize,
                    numChunks = fullVoxelImportData.models[i].chunks.Length,
                    offset = fullVoxelImportData.models[i].offset,
                    chunkCountAxis = fullVoxelImportData.models[i].numChunksByAxis,
                });
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