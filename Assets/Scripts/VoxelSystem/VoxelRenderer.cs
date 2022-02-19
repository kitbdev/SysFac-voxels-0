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

        public void Initialize(VoxelChunk chunk) {
            voxelMesher = chunk.world.mesher.CreateInstance();
            voxelMesher.Initialize(chunk, this, chunk.world.renderNullSides);
        }

        [ContextMenu("ClearMesh")]
        public void ClearMesh() {
            voxelMesher.ClearMesh();
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
        /// <summary>
        /// called by mesher after update is complete
        /// </summary>
        internal void ApplyMesh() {
            Mesh mesh = voxelMesher.ApplyMesh();

            meshFilter ??= GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            if (TryGetComponent<MeshCollider>(out var meshcol)) {
                // needs to be re set to update for some reason
                meshcol.sharedMesh = mesh;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                // mark scene not saved
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
#endif
        }
    }
}