using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;

namespace VoxelSystem {
    public class FixedChunkLoader : MonoBehaviour {

        [SerializeField] VoxelWorld world;
        [SerializeField] Vector3Int[] chunksToLoad;
        [SerializeField] bool callOnStart = true;
        [SerializeField] bool callOnEnable = false;

        private void Reset() {
            world = GetComponent<VoxelWorld>();
        }
        private void Awake() {
            world ??= GetComponent<VoxelWorld>();
        }
        private void OnEnable() {
            if (callOnEnable) {
                LoadChunks();
            }
        }
        private void Start() {
            if (callOnStart) {
                LoadChunks();
            }
        }
        [ContextMenu("Reset chunks to load")]
        public void SetChunksToLoadOrigin() {
            chunksToLoad = new Vector3Int[1]{
                Vector3Int.zero
            };
        }
        [ContextMenu("set cube")]
        void SetChunksToLoadCube() {
            List<Vector3Int> toload = new List<Vector3Int>();
            int rad = 1;
            for (int x = -rad; x <= rad; x++) {
                for (int z = -rad; z <= rad; z++) {
                    for (int y = -rad; y <= rad; y++) {
                        toload.Add(new Vector3Int(x, y, z));
                    }
                }
            }
            chunksToLoad = toload.ToArray();
        }
        [ContextMenu("Reload")]
        public void LoadChunks() {
            world ??= GetComponent<VoxelWorld>();
            world.UnloadAllChunks();
            world.LoadChunks(chunksToLoad);
        }
    }
}