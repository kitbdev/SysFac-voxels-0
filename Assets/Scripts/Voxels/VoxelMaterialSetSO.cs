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
        private void Awake() {
            Debug.Log("VoxelMaterialSetSO awake " + mats.Length);
            vmats = new SerializableDictionary<VoxelMaterialId, VoxelMaterial>();
            // foreach (var item in collection)
            // {

            // }
            // vmats.Add()
        }
        public VoxelMaterialId GetIdForVoxelMaterial(ImplementsType<VoxelMaterial> voxelMaterialType) {
            // todo use hash for ids?
            int vmat = mats.ToList().FindIndex(tsm => { return tsm.type == voxelMaterialType; });
            return vmat;
        }
        public VoxelMaterial GetVoxelMaterial(VoxelMaterialId id) {
            return mats[id].obj;
        }
        public T GetVoxelMaterial<T>(VoxelMaterialId id) where T : VoxelMaterial {
            // if (!vmats.ContainsKey(id)) {
            if (id < 0 || id >= mats.Length) {
                Debug.LogWarning($"VoxelMaterial {id} not found!");
                return null;
            }
            return (T)mats[id].obj;
        }
    }
}