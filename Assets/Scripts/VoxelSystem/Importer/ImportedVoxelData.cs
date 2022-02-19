using UnityEngine;

namespace VoxelSystem.Importer {
    // [CreateAssetMenu(fileName = "ImportedVoxelData", menuName = "SysFac/ImportedVoxelData", order = 0)]
    public class ImportedVoxelData : ScriptableObject {
        public VoxelRoomData[] roomsData;
    }
}