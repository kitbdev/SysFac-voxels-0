using UnityEngine;
using System.Collections.Generic;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    [CreateAssetMenu(fileName = "VoxelMaterialSet", menuName = "VoxelSystem/VoxelMaterialSet", order = 0)]
    public partial class VoxelMaterialSetSO : ScriptableObject {

        Dictionary<VoxelMaterialId, VoxelMaterial> vmats;
        [SerializeField]
        private Material[] _allVMatMaterials;
        public TextureAtlasPacker textureAtlas;
        // todo use SOs for mats?
        public TypeChoice<VoxelMaterial> activeType = typeof(BasicMaterial);
        [SerializeField]
        private TypeSelector<VoxelMaterial>[] _mats;

        public float textureResolution => textureAtlas.textureResolution;
        public float textureScale => textureAtlas.textureBlockScale; //16f / 512f;

        public TypeSelector<VoxelMaterial>[] mats { get => _mats; set => _mats = value; }
        public Dictionary<VoxelMaterialId, VoxelMaterial> voxelMatDict { get => vmats; }
        public Material[] allVMatMaterials { get => _allVMatMaterials; protected set => _allVMatMaterials = value; }

        private void Awake() {// todo move?
            Debug.Log("VoxelMaterialSetSO awake " + mats.Length);
            UpdateVMatDict();
            UpdateMatTextures();
            // foreach (var item in collection)
            // {

            // }
            // vmats.Add()
        }
        private void OnEnable() {
            UpdateVMatDict();
        }

        private void UpdateVMatDict() {
            int i = -1;
            //(SerializableDictionary<VoxelMaterialId, VoxelMaterial>)
            vmats = mats.ToDictionary(vm => {
                i++;
                return (VoxelMaterialId)i;
            }, vm => {
                return vm.obj;
            });
        }
        [ContextMenu("Update mat")]
        private void UpdateMatTextures() {
            allVMatMaterials.ToList().ForEach(mat => {
                mat.mainTexture = textureAtlas?.atlas;
            });
        }

        public VoxelMaterialId GetIdForVoxelMaterial(TypeChoice<VoxelMaterial> voxelMaterialType) {
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

        public void AddVoxelMaterial(VoxelMaterial newVoxMat) {
            mats.Append(new TypeSelector<VoxelMaterial>(newVoxMat));
            if (newVoxMat.material != null) {
                if (!allVMatMaterials.Contains(newVoxMat.material)) {
                    allVMatMaterials.Append(newVoxMat.material);
                }
                newVoxMat.materialIndex = allVMatMaterials.ToList().IndexOf(newVoxMat.material);
            }
            UpdateVMatDict();
        }
        public TypeSelector<VoxelMaterial> AddNewVoxelMaterial(TypeChoice<VoxelMaterial> type) {
            // TypeSelector<VoxelMaterial> newMat = new TypeSelector<VoxelMaterial>(type);
            VoxelMaterial voxelMaterial = type.CreateInstance();
            AddVoxelMaterial(voxelMaterial);
            return mats.Last();
        }
    }
}