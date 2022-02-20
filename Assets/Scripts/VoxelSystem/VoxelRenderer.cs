using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using VoxelSystem.Mesher;

namespace VoxelSystem {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VoxelRenderer : MonoBehaviour {

        [SerializeField, SerializeReference]
        VoxelMesher voxelMesher;
        MeshFilter meshFilter;
        VoxelChunk chunk;

        public void Initialize(VoxelChunk chunk) {
            this.chunk = chunk;
            UpdateMesher();
        }

        [ContextMenu("ClearMesh")]
        public void ClearMesh() {
            voxelMesher.ClearMesh();
        }

        [ContextMenu("UpdateMesher")]
        public void UpdateMesher() {
            UpdateMaterials();
            voxelMesher = chunk.world.mesher.CreateInstance();
            voxelMesher.Initialize(chunk, this, chunk.world.renderNullSides);
        }
        [ContextMenu("UpdateMesh")]
        public void UpdateMesh() {

            voxelMesher.UpdateMesh();
        }
        public void UpdateMeshAt(Vector3Int vpos) {
            voxelMesher.UpdateMeshAt(vpos);
        }
        public Mesh GetMesh() {
            return meshFilter?.sharedMesh;
        }
        [ContextMenu("UpdateMats")]
        public void UpdateMaterials() {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = chunk?.world.materialSet?.allUsedMaterials;
        }
        /// <summary>
        /// called by mesher after update is complete
        /// </summary>
        internal void ApplyMesh() {
            Mesh mesh = voxelMesher.ApplyMesh();

            meshFilter ??= GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                // mark scene not saved
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
#endif
        }
    }
}