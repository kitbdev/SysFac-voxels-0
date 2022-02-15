using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public abstract class VoxelMesher {

        [SerializeField] protected VoxelChunk chunk;
        [SerializeField] protected VoxelWorld world;
        [SerializeField] protected VoxelRenderer renderer;
        protected float voxelSize;

        public virtual void Initialize(VoxelChunk chunk, VoxelRenderer renderer) {
            this.chunk = chunk;
            this.world = chunk.world;
            this.renderer = renderer;
            voxelSize = world.voxelSize;
        }
        public abstract void ClearMesh();
        public abstract void UpdateMesh();
        public abstract void UpdateMeshAt(Vector3Int vpos);
        internal abstract Mesh ApplyMesh();

        protected void FinishedMesh() {
            renderer.ApplyMesh();
        }
    }
}