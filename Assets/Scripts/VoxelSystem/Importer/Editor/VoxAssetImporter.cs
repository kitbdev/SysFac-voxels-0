using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using VoxReader;

namespace VoxelSystem {
    [ScriptedImporter(1, "vox")]
    public class VoxAssetImporter : ScriptedImporter {
        public bool test1;
        public override void OnImportAsset(AssetImportContext ctx) {
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
            VoxelImporter.Load(voxelWorld, ctx.assetPath);
            // voxelWorld.set
            // maingo.AddComponent<VoxelRenderer>()
            ctx.AddObjectToAsset("main vox go", maingo);
            ctx.SetMainObject(maingo);
        }
    }
}