using UnityEngine;
using System.Collections.Generic;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    [CreateAssetMenu(fileName = "VoxelMaterialSet", menuName = "Voxel/VoxelMaterialSet", order = 0)]
    public partial class VoxelMaterialSetSO : ScriptableObject {
        public SerializableDictionary<VoxelMaterialId, VoxelMaterial> vmats;
        public Material[] materials;
        public float textureResolution;
        public TypeSelector<VoxelMaterial>[] mats;
        private void OnValidate() {
            mats.ToList().ForEach(m => { m.OnValidate(); });
        }
    }
}