using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using static Unity.Mathematics.math;
using Unity.Collections;
using Unity.Mathematics;
using Kutil;
using System.Linq;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public class AdvMesher : VoxelMesher {

        public override Kutil.TypeChoice<VoxelMaterial> neededMaterial => typeof(TexturedMaterial);

        [System.Serializable]
        struct MeshGenPData {
            public int numTotalVertices;
            public int numTotalIndeces;
            public NativeArray<FaceData> faceDatas;
            public NativeArray<SubmeshData> submeshDatas;
            // todo greedy meshing
            [System.Serializable]
            public struct SubmeshData {
                public int numVertices;
                public int numIndeces;
                public int startIndex;
            }
            [System.Serializable]
            public struct FaceData {
                public float3 voxelPos;
                public VoxelDirection faceNormal;
                public float2 texcoord;
                public int submeshIndex;
            }
        }
        [SerializeField] Mesh mesh;
        [SerializeField] MeshGenPData preprocessMeshData;

        public override void Initialize(VoxelChunk chunk, VoxelRenderer renderer, bool renderNullSides = false) {
            base.Initialize(chunk, renderer, renderNullSides);
            mesh = new Mesh();
        }
        internal override Mesh ApplyMesh() {
            return mesh;
        }

        public override void ClearMesh() {
            mesh.Clear();
        }

        public override void UpdateMesh() {
            GenMesh();
        }

        public override void UpdateMeshAt(Vector3Int vpos) {
            UpdateMesh();
        }

        void GenMesh() {
            preprocessMeshData = new MeshGenPData();
            // todo convert preprocess to a job
            // GenMeshPreprocessJob.ScheduleParallel(chunk, preprocessMeshData, default).Complete();
            // new GenMeshPreprocessJob() {
            // chunk = chunk, meshGenPData = preprocessMeshData
            // }.Execute(0);
            PreprocessExecute(materialSet.allUsedMaterials.Length);
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            // Debug.Log($"faces:{preprocessMeshData.faceDatas.Length} v:{preprocessMeshData.numVertices} t:{preprocessMeshData.numTriangles}");
            // genmesh execute
            Bounds meshBounds = new Bounds(Vector3.one * world.chunkSize / 2f, Vector3.one * world.chunkSize);
            GenMeshJob.ScheduleParallel(
                    meshData, preprocessMeshData, voxelSize, meshBounds, materialSet.textureScale,
                    4, default).Complete();
            preprocessMeshData.faceDatas.Dispose();
            preprocessMeshData.submeshDatas.Dispose();
            mesh ??= new Mesh();
            mesh.name = "Chunk Mesh Advanced";
            mesh.bounds = meshBounds;
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            FinishedMesh();
        }

        // [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        // struct GenMeshPreprocessJob {// : IJobFor {
        //     // [ReadOnly]
        //     public VoxelChunk chunk;
        //     // [WriteOnly]
        //     public MeshGenPData meshGenPData;
        // public void Execute(int index) {
        //     PreprocessExecute(index);
        // }
        // public static JobHandle ScheduleParallel(VoxelChunk chunk, MeshGenPData meshGenPData, JobHandle dependency) {
        //     var job = new GenMeshPreprocessJob();
        //     job.chunk = chunk;
        //     job.meshGenPData = meshGenPData;
        //     int jobLength = 1;
        //     return job.ScheduleParallel(jobLength, 1, dependency);
        // }
        List<MeshGenPData.FaceData> tlist = new List<MeshGenPData.FaceData>();
        List<int> submeshflist;
        void PreprocessExecute(int numSubMeshes) {
            // calculate mesh needs 
            submeshflist = new int[numSubMeshes].ToList();
            tlist.Clear();
            // int neiChunkRes = chunk.resolution+2;
            // int[] neichunkvoxelCache = new int[neiChunkRes*neiChunkRes*neiChunkRes];
            for (int y = 0; y < chunk.resolution; y++) {
                for (int z = 0; z < chunk.resolution; z++) {
                    for (int x = 0; x < chunk.resolution; x++) {
                        CheckVertex(new Vector3Int(x, y, z));
                    }
                }
            }
            tlist.Sort((a, b) => a.submeshIndex - b.submeshIndex);
            // meshGenPData.faceDatas = new NativeArray<MeshGenPData.FaceData>(tlist.ToArray(), Allocator.Persistent);
            preprocessMeshData.faceDatas = new NativeArray<MeshGenPData.FaceData>(tlist.ToArray(), Allocator.Persistent);
            preprocessMeshData.numTotalVertices = preprocessMeshData.faceDatas.Length * 4;
            preprocessMeshData.numTotalIndeces = preprocessMeshData.faceDatas.Length * 2 * 3;
            preprocessMeshData.submeshDatas = new NativeArray<MeshGenPData.SubmeshData>(submeshflist.Select((n, i) =>
                new MeshGenPData.SubmeshData() {
                    numVertices = n * 4, numIndeces = n * 6, startIndex = submeshflist.Take(i).Sum()
                }).ToArray()
            , Allocator.Persistent);
        }
        void CheckVertex(Vector3Int vpos) {
            var voxel = chunk.GetLocalVoxelAt(vpos);
            TexturedMaterial voxelMat = voxel.GetVoxelMaterial<TexturedMaterial>(materialSet);
            if (voxelMat == null) {
                // Debug.LogWarning("Could not get voxel material");
                voxelMat = materialSet.GetDefaultVoxelMaterial<TexturedMaterial>();
                // return;
            }
            if (voxelMat.isInvisible) {
                return;
            }
            // check all directions
            for (int d = 0; d < Voxel.unitDirs.Length; d++) {
                Vector3Int normalDir = Voxel.unitDirs[d];
                // cull check
                Voxel coverNeighbor = chunk.GetVoxelN(vpos + normalDir);
                TexturedMaterial neimat = coverNeighbor?.GetVoxelMaterial<TexturedMaterial>(materialSet) ?? materialSet.GetDefaultVoxelMaterial<TexturedMaterial>();

                bool renderFace;
                renderFace = CanRenderFace(voxelMat, coverNeighbor, neimat);
                // Debug.Log($"check {vpos}-{d}: {vpos + normalDir}({chunk.IndexAt(vpos + normalDir)}) is {coverNeighbor} r:{renderFace}");
                if (!renderFace) {
                    continue;
                }
                // add face
                MeshGenPData.FaceData faceData = new MeshGenPData.FaceData() {
                    voxelPos = (Vector3)vpos,
                    faceNormal = ((VoxelDirection)d),
                    texcoord = (Vector2)voxelMat.textureOverrides.textureCoords[d],
                    submeshIndex = voxelMat.materialIndex,
                };
                tlist.Add(faceData);
                submeshflist[voxelMat.materialIndex]++;
            }
        }
        bool CanRenderFace(TexturedMaterial voxelMat, Voxel coverNeighbor, TexturedMaterial neimat) {
            bool renderFace;
            if (renderNullSides) {
                // render face if neighbor is invisible or one of us is transparent
                renderFace = coverNeighbor == null || (neimat.isInvisible || (neimat.isTransparent ^ voxelMat.isTransparent));
            } else {
                renderFace = coverNeighbor != null && (neimat.isInvisible || (neimat.isTransparent ^ voxelMat.isTransparent));
            }
            return renderFace;
        }
        // }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        struct GenMeshJob : IJobFor {

            [Unity.Collections.ReadOnly]
            MeshGenPData meshGenPData;
            [Unity.Collections.ReadOnly]
            float voxelSize;
            [Unity.Collections.ReadOnly]
            float textureUVScale;
            [Unity.Collections.ReadOnly]
            int batchSize;

            [WriteOnly]
            MeshStream meshStream;

            public void Execute(int index) {
                GenMeshExecute(index);
            }
            public static JobHandle ScheduleParallel(
                Mesh.MeshData meshData, MeshGenPData meshGenPData, float voxelSize, Bounds meshBounds,
                float textureUVScale,
                int jobLength, JobHandle dependency) {
                var job = new GenMeshJob();
                job.meshGenPData = meshGenPData;
                job.voxelSize = voxelSize;
                job.textureUVScale = textureUVScale;
                job.meshStream.Setup(meshData, meshGenPData.numTotalVertices, meshGenPData.numTotalIndeces,
                    meshGenPData.submeshDatas.Select(smd => new SubmeshDescData() {
                        bounds = meshBounds, vertexCount = smd.numVertices, indexCount = smd.numIndeces
                    }).ToArray());
                job.batchSize = (int)math.ceil(((float)meshGenPData.faceDatas.Length) / jobLength);
                // may have overflow, but oh well
                // if (job.batchSize != ((float)meshGenPData.faceDatas.Length) / jobLength) {
                //     Debug.LogWarning($"Adv Mesh Job job length {jobLength} is not a factor of num faces {meshGenPData.faceDatas.Length} mod:{meshGenPData.faceDatas.Length % jobLength}");
                // }
                JobHandle jobHandle = job.ScheduleParallel(jobLength, 1, dependency);
                job.meshStream.Dispose(jobHandle);
                return jobHandle;
            }
            void GenMeshExecute(int jobindex) {
                // Debug.Log($"Job {jobindex} start batchlen:{batchSize} faces:{meshGenPData.faceDatas.Length}");
                // for each face
                int faceStart = jobindex * batchSize;
                for (int i = faceStart; i < faceStart + batchSize
                    && i < meshGenPData.faceDatas.Length; i++) {
                    int vi = i * 4;
                    MeshGenPData.FaceData faceData = meshGenPData.faceDatas[i];
                    int d = (int)faceData.faceNormal;
                    // must be sorted by submesh
                    int ti = i * 2;// + meshGenPData.submeshDatas[faceData.submeshIndex].startIndex;
                    // Debug.Log($"face {i} dir:{d} p:{faceData.voxelPos} uv:{faceData.texcoord} vi:{vi} ti:{ti}");
                    float3 normal = unitDirs[d];
                    float4 tangent = math.float4(dirTangents[d], -1);
                    float3 vertexpos = faceData.voxelPos * voxelSize - math.float3(voxelSize / 2f);
                    vertexpos += vOffsets[d] * voxelSize;
                    float2 uvfrom = faceData.texcoord * textureUVScale;
                    float2 uvto = (faceData.texcoord + float2(1f)) * textureUVScale;
                    meshStream.SetFace(
                        vi, ti, vertexpos, math.float2(voxelSize), normal, tangent, uvfrom, uvto);
                    // vi += 4;
                    // ti += 2;
                }
                // Debug.Log("Job end");
            }
            readonly static float3[] unitDirs = new float3[6] {
            math.float3(1,0,0),// right
            math.float3(0,0,1),// forward
            math.float3(0,1,0),// up
            math.float3(-1,0,0),// left
            math.float3(0,0,-1),// back
            math.float3(0,-1,0),// down
            };
            readonly static float3[] dirTangents = new float3[6] {
            math.float3(0,0,1),
            math.float3(-1,0,0),
            math.float3(1,0,0),
            math.float3(0,0,-1),
            math.float3(1,0,0),
            math.float3(1,0,0),
            };
            readonly static float3[] vOffsets = new float3[6] {
            math.float3(1,0,0),
            math.float3(1,0,1),
            math.float3(0,1,0),
            math.float3(0,0,1),
            math.float3(0,0,0),
            math.float3(0,0,1),
            };
        }
    }
}