using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using static Unity.Mathematics.math;

namespace VoxelSystem.Mesher {
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16 {
        public ushort a, b, c;

        public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16 {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }
    public struct SubmeshDescData {
        public int vertexCount;
        public int indexCount;
        public Bounds bounds;
    }
    public struct MeshStream {
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        [NativeDisableContainerSafetyRestriction]
        NativeArray<Vertex> vertexStream;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
            Setup(meshData, vertexCount, indexCount, new SubmeshDescData[1]{new SubmeshDescData(){
                vertexCount = vertexCount, indexCount = indexCount, bounds = bounds
            }});
        }
        public void Setup(Mesh.MeshData meshData, int vertexCount, int indexCount, SubmeshDescData[] submeshes) {
            var descriptor = new NativeArray<VertexAttributeDescriptor>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            descriptor[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3
            );
            descriptor[2] = new VertexAttributeDescriptor(
                VertexAttribute.Tangent, dimension: 4
            );
            descriptor[3] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2
            );
            meshData.SetVertexBufferParams(vertexCount, descriptor);
            descriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = submeshes.Length;
            int indexAcc = 0;
            for (int i = 0; i < submeshes.Length; i++) {
                meshData.SetSubMesh(i, new SubMeshDescriptor(indexAcc, submeshes[i].indexCount) {
                    bounds = submeshes[i].bounds,
                    vertexCount = submeshes[i].vertexCount
                },
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices
                );
                indexAcc += submeshes[i].indexCount;
            }
            // submeshStartTIndex = new NativeArray<int>(submeshStartTIndexArray, Allocator.Persistent);

            vertexStream = meshData.GetVertexData<Vertex>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
            // Debug.Log($"mesh setup verts{vertexCount} indexcount{indexCount} bounds{bounds}");
        }
        public void Dispose(Unity.Jobs.JobHandle jobHandle) {
            // submeshStartTIndex.Dispose(jobHandle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int vIndex, Vertex vertex) => vertexStream[vIndex] = new Vertex {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTriangle(int tIndex, int3 triangle) => triangles[tIndex] = triangle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFaceCentered(int vIndex, int tIndex, float3 center, float2 extents, float3 normal, float4 tangent, float2 uvfrom, float2 uvto) {
            float3 tang = tangent.xyz;
            float3 bitang = cross(normal, tang) * tangent.w;
            float3 bottomLeft = center - (extents.x * tang + extents.y * bitang);
            SetFace(vIndex, tIndex, bottomLeft, extents * 2f, normal, tangent, uvfrom, uvto);
        }
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFace(int vIndex, int tIndex, float3 bottomLeftPos, float2 size, float3 normal, float4 tangent, float2 uvfrom, float2 uvto) {
            // Debug.Log($"setface {vIndex}/{stream0.Length} {tIndex}/{triangles.Length}");
            // note this wont weld with any others
            // vertex.tangent.xw = float2(1f, -1f);
            // todo? make sure this is optimized
            Vertex vertex = new Vertex();
            vertex.normal = normal;
            vertex.tangent = tangent;
            float3 tang = tangent.xyz;
            float3 bitang = cross(normal, tang) * tangent.w;
            float3 topRight = bottomLeftPos + (size.x * tang + size.y * bitang);

            vertex.position = bottomLeftPos;
            vertex.texCoord0 = uvfrom;
            SetVertex(vIndex, vertex);
            vertex.position = bottomLeftPos + size.x * tang;
            vertex.texCoord0.x = uvto.x;
            vertex.texCoord0.y = uvfrom.y;
            SetVertex(vIndex + 1, vertex);
            vertex.position = bottomLeftPos + size.y * bitang;
            vertex.texCoord0.x = uvfrom.x;
            vertex.texCoord0.y = uvto.y;
            SetVertex(vIndex + 2, vertex);
            vertex.position = topRight;
            vertex.texCoord0 = uvto;
            SetVertex(vIndex + 3, vertex);
            SetTriangle(tIndex + 0, vIndex + int3(0, 2, 1));
            SetTriangle(tIndex + 1, vIndex + int3(1, 2, 3));
        }

    }
}