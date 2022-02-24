using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem {

    public class VoxelCollider : MonoBehaviour {

        public enum ColliderType {
            MESH,
            BOXES
        }

        public VoxelChunk chunk;
        public bool enableCollision = true;
        [SerializeField]
        private ColliderType _colliderType;
        public ColliderType colliderType {
            get => _colliderType;
            set {
                _colliderType = value;
                // UpdateColliders();
            }
        }

        private void Reset() {
            chunk ??= GetComponent<VoxelChunk>();
        }

        public void UpdateColliders() {
            RemoveBoxColliders();
            if (chunk != null && enableCollision) {
                if (colliderType == ColliderType.BOXES) {
                    AddBoxColliders();
                } else {
                    if (gameObject.TryGetComponent<MeshCollider>(out var mc)) {
                        // might need to be re-set to update
                        mc.sharedMesh = chunk.GetMesh();
                    } else {
                        // add meshcollider
                        if (!gameObject.TryGetComponent<MeshCollider>(out _)) {
                            mc = gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = chunk.GetMesh();
                        }
                    }
                }
            } else {
                RemoveColliders();
            }
        }
        // private void LocalUpdateColliders(Vector3Int pos) {
        //     RemoveBoxColliders();
        //     if (world.enableCollision) {
        //         if (world.useBoxColliders) {
        //             AddBoxColliders();
        //         }
        //     }
        // }
        // public void AddColliders() {
        //     // todo? cache this useBoxColliders
        //     // if (!world.enableCollision) return;
        //     if (colliderType == ColliderType.BOXES) {
        //         RemoveMeshCollider();
        //         AddBoxColliders();
        //     } else {
        //         RemoveBoxColliders();
        //         if (!gameObject.TryGetComponent<MeshCollider>(out _)) {
        //             var mc = gameObject.AddComponent<MeshCollider>();
        //             mc.sharedMesh = chunk.GetMesh();
        //         }
        //     }
        // }
        private void AddBoxColliders() {
            var collgo = new GameObject($"chunk {chunk.chunkPos} col");
            collgo.layer = gameObject.layer;
            // collgo.hideFlags = HideFlags.HideAndDontSave;// todo?
            collgo.transform.parent = transform;
            collgo.transform.localPosition = Vector3.zero;
            List<Bounds> surfaceVoxels = new List<Bounds>();
            for (int i = 0; i < chunk.volume; i++) {
                Vector3Int vpos = chunk.GetLocalPos(i);
                Voxel voxel = chunk.GetLocalVoxelAt(i);
                var vmat = voxel.GetVoxelMaterial<TexturedMaterial>(chunk.world.materialSet);
                if (!vmat.hasCollision) {
                    continue;
                }
                bool isHiddenCol = IsHiddenCol(vpos);
                if (!isHiddenCol) {
                    surfaceVoxels.Add(new Bounds(((Vector3)vpos) * chunk.world.voxelSize, Vector3.one * chunk.world.voxelSize));
                }
            }
            foreach (var survox in surfaceVoxels) {
                BoxCollider boxCollider = collgo.AddComponent<BoxCollider>();
                boxCollider.center = survox.center;
                boxCollider.size = survox.size;
            }
        }

        private bool IsHiddenCol(Vector3Int vpos) {
            bool allNeighborsHaveColliders = true;
            foreach (Vector3Int dir in Voxel.unitDirs) {
                Voxel nvoxel = chunk.GetVoxelN(vpos + dir);
                var nvmat = nvoxel?.GetVoxelMaterial<TexturedMaterial>(chunk.world.materialSet);
                if (nvoxel != null && nvmat.hasCollision) {
                    allNeighborsHaveColliders = false;
                    break;
                }
            }
            return allNeighborsHaveColliders;
        }

        public void RemoveColliders() {
            RemoveBoxColliders();
            RemoveMeshCollider();
        }
        private void RemoveMeshCollider() {
            if (gameObject.TryGetComponent<MeshCollider>(out var mcol)) {
                if (Application.isPlaying) {
                    Destroy(mcol);
                } else {
                    DestroyImmediate(mcol);
                }
            }
        }
        private void RemoveBoxColliders() {
            if (transform.childCount > 0) {
                if (Application.isPlaying) {
                    Destroy(transform.GetChild(0).gameObject);
                } else {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
        }
    }
}