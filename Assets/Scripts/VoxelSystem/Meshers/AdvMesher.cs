using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using static Unity.Mathematics.math;
using Unity.Collections;
using Unity.Mathematics;
using Kutil;

namespace VoxelSystem.Mesher {
    [System.Serializable]
    public class AdvMesher : VoxelMesher {

        public override Kutil.TypeChoice<VoxelMaterial> neededMaterial => typeof(BasicMaterial);

        [System.Serializable]
        struct MeshGenPData {
            public int numVertices;
            public int numTriangles;
            public NativeArray<FaceData> faceDatas;
            // todo greedy meshing
            [System.Serializable]
            public struct FaceData {
                public float3 voxelPos;
                public VoxelDirection faceNormal;
                public float2 texcoord;
            }
        }
        [SerializeField] Mesh mesh;
        [SerializeField] MeshGenPData preprocessMeshData;

        public override void Initialize(VoxelChunk chunk, VoxelRenderer renderer) {
            base.Initialize(chunk, renderer);
        }
        internal override Mesh ApplyMesh() {
            return mesh;
        }

        public override void ClearMesh() {
            throw new System.NotImplementedException();
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
            PreprocessExecute(0);
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            // Debug.Log($"faces:{preprocessMeshData.faceDatas.Length} v:{preprocessMeshData.numVertices} t:{preprocessMeshData.numTriangles}");
            // genmesh execute
            Bounds meshBounds = new Bounds(Vector3.one * world.chunkSize / 2f, Vector3.one * world.chunkSize);
            GenMeshJob.ScheduleParallel(
                    meshData, preprocessMeshData, voxelSize, meshBounds, materialSet.textureScale,
                    default).Complete();
            preprocessMeshData.faceDatas.Dispose();
            mesh = new Mesh();
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
        List<MeshGenPData.FaceData> tlist;
        void PreprocessExecute(int jindex) {
            // calculate mesh needs 
            tlist = new List<MeshGenPData.FaceData>();
            for (int y = 0; y < chunk.resolution; y++) {
                for (int z = 0; z < chunk.resolution; z++) {
                    for (int x = 0; x < chunk.resolution; x++) {
                        CheckVertex(new Vector3Int(x, y, z));
                    }
                }
            }
            // meshGenPData.faceDatas = new NativeArray<MeshGenPData.FaceData>(tlist.ToArray(), Allocator.Persistent);
            preprocessMeshData.faceDatas = new NativeArray<MeshGenPData.FaceData>(tlist.ToArray(), Allocator.Persistent);
            preprocessMeshData.numVertices = preprocessMeshData.faceDatas.Length * 4;
            preprocessMeshData.numTriangles = preprocessMeshData.faceDatas.Length * 2;
        }
        void CheckVertex(Vector3Int vpos) {
            var voxel = chunk.GetLocalVoxelAt(vpos);
            BasicMaterial voxelMat = voxel.GetVoxelMaterial<BasicMaterial>(materialSet);
            if (voxelMat.isInvisible) {
                return;
            }
            // check all directions
            for (int d = 0; d < Voxel.unitDirs.Length; d++) {
                Vector3Int normalDir = Voxel.unitDirs[d];
                // cull check
                Voxel coverNeighbor = chunk.GetVoxelN(vpos + normalDir);
                BasicMaterial neimat = coverNeighbor?.GetVoxelMaterial<BasicMaterial>(materialSet);

                bool renderFace = coverNeighbor != null && neimat.isTransparent;
                // bool renderFace = coverNeighbor == null || coverNeighbor.isTransparent;// also render null walls
                // Debug.Log($"check {vpos}-{d}: {vpos + normalDir}({chunk.IndexAt(vpos + normalDir)}) is {coverNeighbor} r:{renderFace}");
                if (!renderFace) {
                    continue;
                }
                // add face
                MeshGenPData.FaceData faceData = new MeshGenPData.FaceData() {
                    voxelPos = (Vector3)vpos,
                    faceNormal = ((VoxelDirection)d),
                    texcoord = (Vector2)voxelMat.textureCoord
                };
                tlist.Add(faceData);
            }
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

            [WriteOnly]
            MeshStream meshStream;

            public void Execute(int index) {
                GenMeshExecute(index);
            }
            public static JobHandle ScheduleParallel(
                Mesh.MeshData meshData, MeshGenPData meshGenPData, float voxelSize, Bounds meshBounds,
                float textureUVScale,
                JobHandle dependency) {
                var job = new GenMeshJob();
                job.meshGenPData = meshGenPData;
                job.voxelSize = voxelSize;
                job.textureUVScale = textureUVScale;
                job.meshStream.Setup(meshData, meshBounds, meshGenPData.numVertices, meshGenPData.numTriangles * 3);
                int jobLength = 1;
                return job.ScheduleParallel(jobLength, 1, dependency);
            }
            void GenMeshExecute(int jindex) {
                // Debug.Log($"Job start {jindex)} {meshGenPData.faceDatas.Length}");
                int vi = 0, ti = 0;
                for (int i = 0; i < meshGenPData.faceDatas.Length; i++) {
                    MeshGenPData.FaceData faceData = meshGenPData.faceDatas[i];
                    int d = (int)faceData.faceNormal;
                    // Debug.Log($"face {i} dir:{d} p:{faceData.voxelPos} uv:{faceData.texcoord} vi:{vi} ti:{ti}");
                    float3 normal = unitDirs[d];
                    float4 tangent = math.float4(dirTangents[d], -1);
                    float3 vertexpos = faceData.voxelPos * voxelSize - math.float3(voxelSize / 2f);
                    vertexpos += vOffsets[d] * voxelSize;
                    float2 uvfrom = faceData.texcoord * textureUVScale;
                    float2 uvto = (faceData.texcoord + float2(1f)) * textureUVScale;
                    meshStream.SetFace(
                        vi, ti, vertexpos, math.float2(voxelSize), normal, tangent, uvfrom, uvto);
                    vi += 4;
                    ti += 2;
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