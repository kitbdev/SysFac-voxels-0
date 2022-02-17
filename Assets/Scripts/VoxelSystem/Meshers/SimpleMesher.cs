using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public class SimpleMesher : VoxelMesher {

        public override TypeChoice<VoxelMaterial> neededMaterial => typeof(BasicMaterial);
        // public override TypeChoice<VoxelData>[] neededDatas => new TypeChoice<VoxelData>[] {
        //                 typeof(MeshCacheVoxelData) };

        float textureUVScale = 16f / 512;

        Mesh mesh;
        List<Vector3> vertices;
        // List<Vector3> normals;
        List<int> triangles;
        List<Vector2> uvs;

        public override void Initialize(VoxelChunk chunk, VoxelRenderer renderer) {
            base.Initialize(chunk, renderer);
            SetupMesh();
            textureUVScale = materialSet.textureScale;
        }

        public override void ClearMesh() {
            ClearForMeshUpdate();
        }
        public override void UpdateMesh() {
            // neighbor updates handled at higher level
            ClearForMeshUpdate();
            CreateMeshVoxels();
            FinishedMesh();
        }

        public override void UpdateMeshAt(Vector3Int vpos) {
            // todo?
            UpdateMesh();
        }

        internal override Mesh ApplyMesh() {
            mesh.vertices = vertices.ToArray();
            mesh.SetTriangles(triangles, 0, false);
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents(); // for normal maps
            mesh.RecalculateBounds();
            return mesh;
        }
        void SetupMesh() {
            // todo use advanced mesh api to be better and faster
            mesh = new Mesh();
            mesh.name = "Chunk Mesh Simple";
            vertices = new List<Vector3>();
            triangles = new List<int>();
            // normals = new List<Vector3>();
            uvs = new List<Vector2>();
            // todo seperate mesh for liquids? transparents?
        }
        void ClearForMeshUpdate() {
            // per run
            vertices.Clear();
            triangles.Clear();
            // foreach (var submeshData in submeshDatas) {
            //     submeshData.triangles.Clear();
            // }
            // normals.Clear();
            uvs.Clear();
            // ClearCaches();
            mesh.Clear();
        }
        void ClearCaches() {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
        }
        void CreateMeshVoxels() {
            // create 6 faces, one in each direction, for each voxel

            // todo meshes for other shapes
            for (int y = 0; y < chunk.resolution; y++) {
                for (int z = 0; z < chunk.resolution; z++) {
                    for (int x = 0; x < chunk.resolution; x++) {
                        CreateBlock(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        void CreateBlock(Vector3Int vpos) {
            // get block type
            var voxel = chunk.GetLocalVoxelAt(vpos);
            // todo other performance stuff
            // var block = BlockManager.Instance.GetBlockTypeAtIndex(voxel.blockId);
            BasicMaterial voxelMat = voxel.GetVoxelMaterial<BasicMaterial>(materialSet);
            if (voxelMat.isInvisible) {
                return;
            }

            Vector3 fromVec = Vector3.zero;
            Vector3 toVec = Vector3.one * chunk.world.voxelSize;
            Vector2 uvfrom = Vector2.zero;
            Vector2 uvto = Vector2.one;
            Vector2 texoffset = voxelMat.textureCoord;

            void CreateFace(Vector3 vertexpos, Vector3 normal, Vector3 rightTangent, Vector3 upTangent) {
                int vcount = vertices.Count;
                // Debug.Log($"Creating face pos:{vertexpos} n:{normal} uv:{texoffset}");
                // vertices 
                vertices.Add(vertexpos + fromVec);
                vertices.Add(vertexpos + fromVec + Vector3.Scale(rightTangent, toVec));
                vertices.Add(vertexpos + fromVec + Vector3.Scale(upTangent, toVec));
                vertices.Add(vertexpos + fromVec + Vector3.Scale(rightTangent + upTangent, toVec));
                // uvs
                uvs.Add(textureUVScale * (texoffset + uvfrom));
                uvs.Add(textureUVScale * (texoffset + Vector2.right * uvto + uvfrom));
                uvs.Add(textureUVScale * (texoffset + Vector2.up * uvto + uvfrom));
                uvs.Add(textureUVScale * (texoffset + Vector2.one * uvto + uvfrom));
                // tris
                AddTriSquare(vcount, vcount + 1, vcount + 2, vcount + 3);
            }
            // create faces
            for (int d = 0; d < Voxel.unitDirs.Length; d++)
            // int d = 0;
            {
                Vector3Int normalDir = Voxel.unitDirs[d];
                Vector3Int rightTangent = Voxel.dirTangents[d];
                Vector3Int upTangent = Vector3Int.FloorToInt(-Vector3.Cross(normalDir, rightTangent));
                // cull check
                Voxel coverNeighbor = chunk.GetVoxelN(vpos + normalDir);
                BasicMaterial neimat = coverNeighbor?.GetVoxelMaterial<BasicMaterial>(materialSet);
                // bool renderFace = coverNeighbor != null && neimat.isTransparent;
                bool renderFace = coverNeighbor == null || neimat.isTransparent;// render null sides
                // Debug.Log($"check {vpos}-{d}: {vpos + normalDir}({chunk.IndexAt(vpos + normalDir)}) is {coverNeighbor} r:{renderFace}");
                if (!renderFace) {
                    continue;
                }
                texoffset = voxelMat.textureOverrides.textureCoords[d];
                // add face
                Vector3 vertexpos = (Vector3)vpos * voxelSize - Vector3.one * voxelSize / 2;
                vertexpos += Voxel.vOffsets[d] * voxelSize;
                CreateFace(vertexpos, normalDir, rightTangent, upTangent);
            }
        }

        /// <summary>
        /// add tris for a face, given verts
        /// </summary>
        /// <param name="v0">bottom left</param>
        /// <param name="v1">bottom right</param>
        /// <param name="v2">top left</param>
        /// <param name="v3">top right</param>
        void AddTriSquare(int v0, int v1, int v2, int v3, int submesh = 0) {
            triangles.Add(v0);
            triangles.Add(v2);
            triangles.Add(v1);
            triangles.Add(v2);
            triangles.Add(v3);
            triangles.Add(v1);
        }

    }
}