using UnityEngine;
using System.Collections.Generic;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    [CreateAssetMenu(fileName = "VoxelMaterialSet", menuName = "Voxel/VoxelMaterialSet", order = 0)]
    public partial class VoxelMaterialSetSO : ScriptableObject {

        [HideInInspector]
        public SerializableDictionary<VoxelMaterialId, VoxelMaterial> vmats;
        public Material[] materials;
        public float textureResolution = 512f;
        // todo use SOs vor mats?
        public TypeSelector<VoxelMaterial>[] mats;

        private void OnValidate() {
            mats.ToList().ForEach(m => { m.OnValidate(); });
        }
        public VoxelMaterialId GetIdForVoxelMaterial(ImplementsType<VoxelMaterial> voxelMaterialType) {
            // todo use hash for ids?
            int vmat = mats.ToList().FindIndex(tsm => { return tsm.type == voxelMaterialType; });
            return vmat;
        }
    }
}