using UnityEngine;
using System.Collections.Generic;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    [CreateAssetMenu(fileName = "VoxelMaterialSet", menuName = "VoxelSystem/VoxelMaterialSet", order = 0)]
    public partial class VoxelMaterialSetSO : ScriptableObject {

        Dictionary<VoxelMaterialId, VoxelMaterial> _voxelMatDict;
        [SerializeField]
        private Material[] _allUsedMaterials;
        public TextureAtlasPacker textureAtlas;
        // todo use SOs for mats?
        public TypeChoice<VoxelMaterial> activeType = typeof(BasicMaterial);
        [SerializeField]
        private TypeSelector<VoxelMaterial>[] _voxelMats;

        public float textureResolution => textureAtlas.textureResolution;
        public float textureScale => textureAtlas.textureBlockScale; //16f / 512f;

        public TypeSelector<VoxelMaterial>[] voxelMats { get => _voxelMats; set => _voxelMats = value; }
        public Dictionary<VoxelMaterialId, VoxelMaterial> voxelMatDict { get => _voxelMatDict; }
        public Material[] allUsedMaterials { get => _allUsedMaterials; protected set => _allUsedMaterials = value; }

        private void OnValidate() {
            foreach (var vm in voxelMats) {
                vm.objvalue.OnValidate(this);
            }
        }
        [ContextMenu("Re Init VMats")]
        private void ReInitVMats() {
            foreach (var vm in voxelMats) {
                vm.objvalue.Initialize(this);
            }
        }

        private void Awake() {// todo move?
            // Debug.Log("VoxelMaterialSetSO awake " + voxelMats.Length);
            UpdateVMatDict();
            UpdateMatTextures();
            // foreach (var item in collection)
            // {

            // }
            // vmats.Add()
        }
        private void OnEnable() {
            // Debug.Log("VoxelMaterialSetSO enable");
            UpdateVMatDict();
            UpdateMatTextures();
            textureAtlas.finishedPackingEvent += UpdateMatTextures;
        }
        private void OnDisable() {
            // Debug.Log("VoxelMaterialSetSO disable");
            textureAtlas.finishedPackingEvent -= UpdateMatTextures;
        }
        // public void OnDestroy() {
        //     Debug.Log("VoxelMaterialSetSO OnDestroy");
        // }

        public void ClearVoxelMats() {
            voxelMats = new TypeSelector<VoxelMaterial>[0];
        }
        public void ClearMats() {
            allUsedMaterials = new Material[0];
        }
        private void UpdateVMatDict() {
            int i = -1;
            //(SerializableDictionary<VoxelMaterialId, VoxelMaterial>)
            _voxelMatDict = voxelMats.ToDictionary(vm => {
                i++;
                return (VoxelMaterialId)i;
            }, vm => {
                return vm.objvalue;
            });
        }
        [ContextMenu("Update mat textures")]
        private void UpdateMatTextures() {
            allUsedMaterials.ToList().ForEach(mat => {
                if (textureAtlas?.atlas != null) {
                    mat.mainTexture = textureAtlas.atlas;
                }
            });
        }

        public Vector2Int GetTexCoordForName(string texname) {
            if (texname == "" || texname == "none") {
                // no texture, return default
                return Vector2Int.zero;
            } else if (textureAtlas.packDict.ContainsKey(texname)) {
                Vector2Int coord = textureAtlas.packDict[texname];
                // Debug.Log($"found {texname} coord {coord}"); 
                return coord;
            } else {
                Debug.LogWarning($"Texture for '{texname}' not found!");
                return Vector2Int.zero;
            }
        }

        public VoxelMaterialId GetDefaultId() {
            // id of first element //? or uninitialized -1
            return 0;
        }
        VoxelMaterialId GetIdForVoxelMaterial(TypeSelector<VoxelMaterial> voxelMaterial) {
            // todo use hash for ids?
            // int vmat = mats.ToList().FindIndex(tsm => { return tsm.type == voxelMaterialType; });
            int id = voxelMats.ToList().IndexOf(voxelMaterial);
            return id;
        }
        public VoxelMaterial GetVoxelMaterial(VoxelMaterialId id) {
            return voxelMats[id].objvalue;
        }
        public T GetVoxelMaterial<T>(VoxelMaterialId id) where T : VoxelMaterial {
            // if (!vmats.ContainsKey(id)) {
            if (id < 0 || id >= voxelMats.Length) {
                Debug.LogWarning($"VoxelMaterial {id} not found!");
                return null;
            }
            // if (!voxelMats[id].type.CanBeAssignedTo(typeof(T))){
            //     Debug.LogWarning($"VoxelMaterial {id} is {voxelMats[id].type} and not of type {typeof(T)}!");
            //     // return null;
            // }
            return (T)voxelMats[id].objvalue;
        }

        public VoxelMaterialId AddVoxelMaterial(VoxelMaterial newVoxMat) {
            TypeSelector<VoxelMaterial> tsMat = new TypeSelector<VoxelMaterial>(newVoxMat);
            voxelMats = voxelMats.Append(tsMat).ToArray();
            if (newVoxMat.material != null) {
                if (!allUsedMaterials.Contains(newVoxMat.material)) {
                    allUsedMaterials.Append(newVoxMat.material);
                }
                newVoxMat.materialIndex = allUsedMaterials.ToList().IndexOf(newVoxMat.material);
            }
            newVoxMat.Initialize(this);
            UpdateVMatDict();
            return GetIdForVoxelMaterial(tsMat);
        }
        public TypeSelector<VoxelMaterial> AddNewVoxelMaterial(TypeChoice<VoxelMaterial> type) {
            // TypeSelector<VoxelMaterial> newMat = new TypeSelector<VoxelMaterial>(type);
            VoxelMaterial voxelMaterial = type.CreateInstance();
            AddVoxelMaterial(voxelMaterial);
            return voxelMats.Last();
        }
    }
}