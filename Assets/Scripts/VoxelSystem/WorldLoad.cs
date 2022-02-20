using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    public class WorldLoad : MonoBehaviour {

        [SerializeField] VoxelWorld world;
        public Importer.ImportedVoxelData voxelData;

        private void Reset() {
            world = GetComponent<VoxelWorld>();
        }
        private void Awake() {
            world ??= GetComponent<VoxelWorld>();
        }
        private void Start() {
            LoadData();
        }
        [ContextMenu("Load Data")]
        public void LoadData() {
            world?.LoadFullImportVoxelData(voxelData?.fullVoxelImportData);
        }
    }
}