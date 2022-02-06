using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using Unity.Mathematics;
// using static Unity.Mathematics.math;

public class VoxelRenderer : MonoBehaviour {

    [SerializeField, ReadOnly] VoxelChunk chunk;
    [SerializeField, ReadOnly] VoxelWorld world;
    [SerializeField, ReadOnly] float textureUVScale = 16f / 512;

    [SerializeField, ReadOnly] Mesh mesh;
    [SerializeField, ReadOnly] List<Vector3> vertices;
    // [SerializeField, ReadOnly] List<Vector3> normals;
    [SerializeField, ReadOnly] List<int> triangles;
    [SerializeField, ReadOnly] List<Vector2> uvs;

    float voxelSize => world.voxelSize;

    public void Initialize(VoxelChunk chunk) {
        this.chunk = chunk;
        world = chunk.world;
        SetupMesh();
        textureUVScale = BlockManager.Instance.blockTextureAtlas.textureBlockScale;
    }

    public void UpdateMesh() {
        // neighbor updates handled at higher level
        ClearForMeshUpdate();
        CreateMeshVoxels();
        ApplyMesh();
    }
    void SetupMesh() {
        mesh = new Mesh();
        mesh.name = "Chunk Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        // normals = new List<Vector3>();
        uvs = new List<Vector2>();
        // todo seperate mesh for liquids? transparents?

        GetComponent<MeshFilter>().sharedMesh = mesh;
        // todo should this be here?
        if (TryGetComponent<MeshCollider>(out var meshcol)) {
            meshcol.sharedMesh = mesh;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            // mark scene not saved
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
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
    [ContextMenu("Clearcache")]
    void ClearCaches() {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }
    void ApplyMesh() {
        mesh.vertices = vertices.ToArray();
        mesh.SetTriangles(triangles, 0, false);
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents(); // for normal maps
        mesh.RecalculateBounds();
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

    [ContextMenu("VoxelGen")]
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
    class BlockData {

    }
    void CreateBlock(Vector3Int vpos) {
        // get block type
        var voxel = chunk.GetLocalVoxelAt(vpos);
        var block = BlockManager.Instance.GetBlockTypeAtIndex(voxel.blockId);
        if (voxel.shape == Voxel.VoxelShape.none) {
            return;
        }

        Vector3 fromVec = Vector3.zero;
        Vector3 toVec = Vector3.one;
        Vector2 uvfrom = Vector2.zero;
        Vector2 uvto = Vector2.one;
        Vector2 texoffset = new Vector2(0, 0) + voxel.textureCoord;

        void CreateFace(Vector3 vertexpos, Vector3 normal, Vector3 rightTangent, Vector3 upTangent) {
            int vcount = vertices.Count;
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
        for (int d = 0; d < dirs.Length; d++)
        // int d = 0;
        {
            Vector3Int normalDir = dirs[d];
            Vector3Int rightTangent = dirXs[d];
            Vector3Int upTangent = Vector3Int.FloorToInt(-Vector3.Cross(normalDir, rightTangent));
            // cull check
            Voxel coverNeighbor = chunk.GetLocalVoxelAt(vpos + normalDir);
            bool renderFace = coverNeighbor == null
                // || coverNeighbor.shape == Voxel.VoxelShape.none
                || coverNeighbor.isTransparent;
            ;
            // Debug.Log($"check {vpos}-{d}: {vpos + normalDir}({chunk.IndexAt(vpos + normalDir)}) is {coverNeighbor} r:{renderFace}");
            if (!renderFace) {
                continue;
            }
            // add face
            Vector3 vertexpos = (Vector3)vpos * voxelSize - Vector3.one * voxelSize / 2;
            vertexpos += vOffsets[d] * voxelSize;
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

    Vector3Int[] dirs = new Vector3Int[6] {
        new Vector3Int(1,0,0),// right
        new Vector3Int(0,0,1),// forward
        new Vector3Int(0,1,0),// up
        new Vector3Int(-1,0,0),// left
        new Vector3Int(0,0,-1),// back
        new Vector3Int(0,-1,0),// down
        };
    Vector3Int[] dirXs = new Vector3Int[6] {
        new Vector3Int(0,0,1),
        new Vector3Int(-1,0,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(1,0,0),
        };
    Vector3[] vOffsets = new Vector3[6] {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        };


}