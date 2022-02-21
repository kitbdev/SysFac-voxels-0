using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using UnityEngine;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public class SimpleMesher : VoxelMesher {

        public override TypeChoice<VoxelMaterial> neededMaterial => typeof(TexturedMaterial);
        // public override TypeChoice<VoxelData>[] neededDatas => new TypeChoice<VoxelData>[] {
        //                 typeof(MeshCacheVoxelData) };

        float textureUVScale = 16f / 512;

        Mesh mesh;
        List<Vector3> vertices;
        // List<Vector3> normals;
        [System.Serializable]
        struct SubmeshData {
            public List<int> triangles;
        }
        [SerializeField] List<SubmeshData> submeshDatas;
        List<Vector2> uvs;

        public override void Initialize(VoxelChunk chunk, VoxelRenderer renderer, bool renderNullSides = false) {
            base.Initialize(chunk, renderer, renderNullSides);
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
            // mesh.SetSubMeshes(submeshDatas.Select(sb => new UnityEngine.Rendering.SubMeshDescriptor()))
            // mesh.SetTriangles(triangles, 0, false);
            mesh.subMeshCount = submeshDatas.Count;
            for (int i = 0; i < submeshDatas.Count; i++) {
                SubmeshData submeshdata = submeshDatas[i];
                mesh.SetTriangles(submeshdata.triangles, i, false);
            }
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

            int submeshCount = materialSet.allUsedMaterials.Length;
            mesh.subMeshCount = submeshCount;
            vertices = new List<Vector3>();
            // triangles = new List<int>();
            // seperate mesh for transparents or whatever
            submeshDatas = new List<SubmeshData>();
            for (int i = 0; i < mesh.subMeshCount; i++) {
                submeshDatas.Add(new SubmeshData {
                    triangles = new List<int>()
                });
            }
            // normals = new List<Vector3>();
            uvs = new List<Vector2>();
        }
        void ClearForMeshUpdate() {
            SetupMesh();
            // // per run
            // vertices.Clear();
            // // triangles.Clear();
            // if (submeshDatas != null) {
            //     foreach (var submeshData in submeshDatas) {
            //         submeshData.triangles.Clear();
            //     }
            // } else {
            //     submeshDatas = new List<SubmeshData>();
            // }
            // // normals.Clear();
            // uvs.Clear();
            // // ClearCaches();
            // mesh.Clear();
        }
        void ClearCaches() {
            vertices.Clear();
            // triangles.Clear();
            submeshDatas.Clear();
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
            // todo? other performance stuff
            TexturedMaterial voxelMat = voxel.GetVoxelMaterial<TexturedMaterial>(materialSet);
            if (voxelMat.isInvisible) {
                return;
            }

            Vector3 fromVec = Vector3.zero;
            Vector3 toVec = Vector3.one * chunk.world.voxelSize;
            Vector2 uvfrom = Vector2.zero;
            Vector2 uvto = Vector2.one;
            Vector2 texoffset = voxelMat.textureCoord;
            // create faces
            for (int d = 0; d < Voxel.unitDirs.Length; d++)
            // int d = 0;
            {
                Vector3Int normalDir = Voxel.unitDirs[d];
                Vector3Int rightTangent = Voxel.dirTangents[d];
                Vector3Int upTangent = Vector3Int.FloorToInt(-Vector3.Cross(normalDir, rightTangent));
                // cull check
                Voxel coverNeighbor = chunk.GetVoxelN(vpos + normalDir);
                TexturedMaterial neimat = coverNeighbor?.GetVoxelMaterial<TexturedMaterial>(materialSet);

                bool renderFace;
                if (renderNullSides) {
                    // render face if neighbor is invisible or one of us is transparent
                    renderFace = coverNeighbor == null || (neimat.isInvisible || (neimat.isTransparent ^ voxelMat.isTransparent));
                } else {
                    renderFace = coverNeighbor != null && (neimat.isInvisible || (neimat.isTransparent ^ voxelMat.isTransparent));
                }
                // Debug.Log($"check {vpos}-{d}: {vpos + normalDir}({chunk.IndexAt(vpos + normalDir)}) is {coverNeighbor} r:{renderFace}");
                if (!renderFace) {
                    continue;
                }
                texoffset = voxelMat.textureOverrides.textureCoords[d];
                int submesh = voxelMat.materialIndex;
                // add face
                Vector3 vertexpos = (Vector3)vpos * voxelSize - Vector3.one * voxelSize / 2;
                vertexpos += Voxel.vOffsets[d] * voxelSize;
                CreateFace(vertexpos, normalDir, rightTangent, upTangent, fromVec, toVec, uvfrom, uvto, texoffset, submesh);
            }
        }

        private void CreateFace(Vector3 vertexpos, Vector3 normal, Vector3 rightTangent, Vector3 upTangent, Vector3 fromVec, Vector3 toVec, Vector2 uvfrom, Vector2 uvto, Vector2 texoffset, int submesh = 0) {
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
            AddFaceTris(vcount, vcount + 1, vcount + 2, vcount + 3, submesh);
        }

        /// <summary>
        /// add tris for a face, given verts
        /// </summary>
        /// <param name="v0">bottom left</param>
        /// <param name="v1">bottom right</param>
        /// <param name="v2">top left</param>
        /// <param name="v3">top right</param>
        void AddFaceTris(int v0, int v1, int v2, int v3, int submesh = 0) {
            submeshDatas[submesh].triangles.Add(v0);
            submeshDatas[submesh].triangles.Add(v2);
            submeshDatas[submesh].triangles.Add(v1);
            submeshDatas[submesh].triangles.Add(v2);
            submeshDatas[submesh].triangles.Add(v3);
            submeshDatas[submesh].triangles.Add(v1);
        }

    }
}