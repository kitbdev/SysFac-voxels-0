using UnityEngine;
using System.Collections.Generic;
using Kutil;

namespace VoxelSystem {
    [CreateAssetMenu(fileName = "VoxelMaterialSet", menuName = "Voxel/VoxelMaterialSet", order = 0)]
    public class VoxelMaterialSetSO : ScriptableObject {
        public SerializableDictionary<VoxelMaterialId, VoxelMaterial> vmats;
        public Material[] materials;
        public float textureResolution;
        public ImplementsType<VoxelMaterial> impltype;
        public List<ImplementsType<VoxelMaterial>> impltypes;
    }
}