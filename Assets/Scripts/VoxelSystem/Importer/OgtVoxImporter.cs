using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelSystem.Importer {
    public class OgtVoxImporter {
        public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
            VoxLoader loader = new VoxLoader(importSettings);
            // ManagedOgtVox managedOgtVox = new ManagedOgtVox();
            // CsharpVoxReader.VoxReader voxReader = new CsharpVoxReader.VoxReader(importSettings.filepath, loader);
            // voxReader.Read();
            return loader.fullVoxelImportData;
        }
    }
}