using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public abstract class VoxelMesher {

        protected VoxelChunk chunk;
        protected VoxelWorld world;
        protected VoxelRenderer renderer;
        protected float voxelSize;
        public bool renderNullSides;

        public virtual TypeChoice<VoxelMaterial> neededMaterial => null;
        public virtual TypeChoice<VoxelData>[] neededDatas => new TypeChoice<VoxelData>[0];

        protected VoxelMaterialSetSO materialSet => world?.materialSet;

        public virtual void Initialize(VoxelChunk chunk, VoxelRenderer renderer, bool renderNullSides=false) {
            this.chunk = chunk;
            this.world = chunk.world;
            this.renderer = renderer;
            this.renderNullSides = renderNullSides;
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