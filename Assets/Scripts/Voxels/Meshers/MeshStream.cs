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
    public struct MeshStream {
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        [NativeDisableContainerSafetyRestriction]
        NativeArray<Vertex> stream0;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;
        // NativeArray<int3> triangles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
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

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) {
                bounds = bounds,
                vertexCount = vertexCount
            },
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontValidateIndices
            );

            stream0 = meshData.GetVertexData<Vertex>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
            // tsize size(int) = 4
            // usize size(int3) = 3*4 = 12
            // bytelen = len * tsize = 64 * 4 = 256
            // ulen = bytelen/usize = 256/12 = 64/3 = 21 (rounded)
            // ulen * usize = 21 * 12 = 252
            // ulen * usize == bytelen
            // ... so num triangles must be divisible by 3?
            // todo look at this again
            // triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex vertex) => stream0[index] = new Vertex {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0
        };
        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

        public void SetFace(int vIndex, int tIndex, float3 center, float2 extents, float3 normal, float4 tangent, float2 uvfrom, float2 uvto) {
            // note this wont weld with any others
            // vertex.tangent.xw = float2(1f, -1f);
            // todo make sure this is optimized
            Vertex vertex = new Vertex();
            vertex.normal = normal;
            vertex.tangent = tangent;
            float3 tang = tangent.xyz;
            float3 bitang = cross(normal, tang) * tangent.w;
            float3 halfdiag = extents.x * tang + extents.y * bitang;
            float3 bottomLeft = center - halfdiag;
            float3 topRight = center + halfdiag;

            vertex.position = bottomLeft;
            vertex.texCoord0 = uvfrom;
            SetVertex(vIndex, vertex);
            vertex.position = bottomLeft + extents.x * tang * 2;
            vertex.texCoord0.x = uvto.x;
            vertex.texCoord0.y = uvfrom.y;
            SetVertex(vIndex + 1, vertex);
            vertex.position = bottomLeft + extents.y * bitang * 2;
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