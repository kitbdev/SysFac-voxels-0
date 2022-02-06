using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using Unity.Mathematics;
// using static Unity.Mathematics.math;

public class VoxelRenderer : MonoBehaviour {

    [SerializeField, ReadOnly] VoxelChunk chunk;
    [SerializeField, ReadOnly] VoxelWorld world;

    [SerializeField, ReadOnly] Mesh mesh;
    [SerializeField, ReadOnly] List<Vector3> vertices;
    // [SerializeField, ReadOnly] List<Vector3> normals;
    [SerializeField, ReadOnly] List<int> triangles;
    [SerializeField, ReadOnly] List<Vector2> uvs;

    float voxelSize => world.voxelSize;

    public void Initialize(VoxelChunk chunk) {
        this.chunk = chunk;
        world = chunk.world;
        CreateMesh();
    }

    public void UpdateMesh() {
        // neighbor updates handled at higher level
        ClearForMeshUpdate();
        CreateMeshVoxels();
        ApplyMesh();
    }
    void CreateMesh() {
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
        // Vector3Int[] dirs = new Vector3Int[6] {
        //     new Vector3Int(1,0,0),
        //     new Vector3Int(0,1,0),
        //     new Vector3Int(0,0,1),
        //     new Vector3Int(-1,0,0),
        //     new Vector3Int(0,-1,0),
        //     new Vector3Int(0,0,-1),
        //     };
        // Vector3Int[] dirXs = new Vector3Int[6] {
        //     new Vector3Int(0,1,0),
        //     new Vector3Int(0,0,1),
        //     new Vector3Int(1,0,0),
        //     new Vector3Int(0,1,0),
        //     new Vector3Int(0,0,1),
        //     new Vector3Int(1,0,0),
        //     };
        // Vector3Int[] dirZs = new Vector3Int[6] {
        //     new Vector3Int(0,0,1),
        //     new Vector3Int(1,0,0),
        //     new Vector3Int(0,1,0),
        //     new Vector3Int(0,0,1),
        //     new Vector3Int(1,0,0),
        //     new Vector3Int(0,1,0),
        //     };
        // Vector3Int V3IMul(Vector3Int vec, int val) {
        //     vec.x *= val;
        //     vec.y *= val;
        //     vec.z *= val;
        //     return vec;
        // }
        // create meshes for cubes
        // for (int d = 0; d < dirs.Length; d++)
        // int d = 0;
        // {
        //     // Debug.Log($"drawing0");
        //     Vector3Int dir = dirs[d];
        //     Vector3Int dirx = dirXs[d];
        //     Vector3Int dirz = dirZs[d];
        //     Vector3Int absdir = new Vector3Int(math.abs(dir.x), math.abs(dir.y), math.abs(dir.z));
        //     bool isNeg = dir == absdir;
        //     // dir.x < 0 || dir.y > 0 || dir.z < 0; // for triangle winding
        //     Vector3 dirv = new Vector3(dir.x > 0 ? dir.x : 0, dir.y > 0 ? dir.y : 0, dir.z > 0 ? dir.z : 0);
        //     Vector3 dirxv = (Vector3)dirx * voxelSize;
        //     Vector3 dirzv = (Vector3)dirz * voxelSize;
        //     for (int y = 0; y < chunk.resolution; y++) {
        //         for (int z = 0; z < chunk.resolution; z++) {
        //             for (int x = 0; x < chunk.resolution; x++) {
        //                 // Vector3Int pos = V3IMul(dirx, x) + V3IMul(dirz, z) + V3IMul(absdir, y);
        //                 Vector3Int pos = new Vector3Int(x, y, z);//todo
        //                 var voxel = chunk.GetLocalVoxelAt(pos);
        //                 if (voxel.shape == Voxel.VoxelShape.none) {
        //                     continue;
        //                 }
        //                 //  && voxel.shape != Voxel.VoxelShape.customcubey) {
        //                 // Debug.Log($"drawingv");
        //                 // if (!voxel.IsAboveSurface) {
        //                 //     continue;
        //                 // }
        //                 // todo check other chunks
        //                 // var coverNeighbor = chunk.GetVoxelAt(pos + dir);
        //                 var coverNeighbor = chunk.GetLocalVoxelAt(pos + dir);
        //                 bool isBlocked = coverNeighbor != null &&
        //                     coverNeighbor.shape == Voxel.VoxelShape.cube &&
        //                     !coverNeighbor.isTransparent;
        //                 if (isBlocked) {
        //                     continue;
        //                 }
        //                 // we need to show
        //                 // todo be greedy https://0fps.net/2012/06/30/meshing-in-a-minecraft-game/
        //                 // add two tris
        //                 var cpos = (Vector3)pos * voxelSize + dirv * voxelSize - Vector3.one * voxelSize / 2;
        //                 int vcount = vertices.Count;
        //                 if (voxel.shape == Voxel.VoxelShape.customcubey) {
        //                     Vector3 vector3 = dirzv * 0.9f;
        //                 }
        //                 vertices.Add(cpos);
        //                 vertices.Add(cpos + dirxv);
        //                 vertices.Add(cpos + dirzv);
        //                 vertices.Add(cpos + dirxv + dirzv);
        //                 // todo uvs
        //                 float uvscale = 1f / 4;
        //                 Vector2 texoffset = Vector2.zero;
        //                 uvs.Add(uvscale * (texoffset + Vector2.zero));
        //                 uvs.Add(uvscale * (texoffset + Vector2.right));
        //                 uvs.Add(uvscale * (texoffset + Vector2.up));
        //                 uvs.Add(uvscale * (texoffset + Vector2.one));
        //                 if (!isNeg) {
        //                     triangles.Add(vcount);
        //                     triangles.Add(vcount + 2);
        //                     triangles.Add(vcount + 1);
        //                     triangles.Add(vcount + 1);
        //                     triangles.Add(vcount + 2);
        //                     triangles.Add(vcount + 3);
        //                     // AddTriFace(vcount,vcount+2,vcount+1,vcount+3,col);
        //                 } else {
        //                     // AddTriFace(vcount,vcount+2,vcount+1,vcount+3,col);
        //                     triangles.Add(vcount);
        //                     triangles.Add(vcount + 1);
        //                     triangles.Add(vcount + 2);
        //                     triangles.Add(vcount + 1);
        //                     triangles.Add(vcount + 3);
        //                     triangles.Add(vcount + 2);
        //                 }
        //             }
        //         }
        //     }
        // }
        // meshes for other shapes

        for (int y = 0; y < chunk.resolution; y++) {
            for (int z = 0; z < chunk.resolution; z++) {
                for (int x = 0; x < chunk.resolution; x++) {
                    CreateCube(new Vector3Int(x, y, z));
                }
            }
        }
    }
    void CreateCube(Vector3Int vpos) {
        // get block type
        var voxel = chunk.GetLocalVoxelAt(vpos);
        var block = BlockManager.Instance.GetBlockTypeAtIndex(voxel.blockId);
        if (voxel.shape == Voxel.VoxelShape.none) {
            return;
        }
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.one;
        // create faces
        for (int d = 0; d < dirs.Length; d++)
        // int d = 0;
        {
            Vector3Int dir = dirs[d];
            // cull check
            var coverNeighbor = chunk.GetLocalVoxelAt(vpos + dir);
            bool isBlocked = coverNeighbor != null &&
                coverNeighbor.shape == Voxel.VoxelShape.cube &&
                !coverNeighbor.isTransparent;
            if (isBlocked) {
                return;
            }
            void CreateFace(Vector3 vpos, Vector3 normal, Vector3 rightTangent, Vector3 upTangent) {
                var vertexpos = (Vector3)vpos * voxelSize - Vector3.one * voxelSize / 2;
                int vcount = vertices.Count;
                vertices.Add(vertexpos);
                vertices.Add(vertexpos + rightTangent);
                vertices.Add(vertexpos + upTangent);
                vertices.Add(vertexpos + rightTangent + upTangent);
                AddTriSquare(vcount, vcount + 2, vcount + 1, vcount + 3);

            }
            Vector3Int dirx = dirXs[d];
            Vector3Int dirz = dirZs[d];
            Vector3Int absdir = new Vector3Int(math.abs(dir.x), math.abs(dir.y), math.abs(dir.z));
            Vector3 dirv = new Vector3(dir.x > 0 ? dir.x : 0, dir.y > 0 ? dir.y : 0, dir.z > 0 ? dir.z : 0);
            Vector3 dirxv = (Vector3)dirx * voxelSize;
            Vector3 dirzv = (Vector3)dirz * voxelSize;
            // create vertices
            var vertexpos = (Vector3)vpos * voxelSize + dirv * voxelSize - Vector3.one * voxelSize / 2;
            int vcount = vertices.Count;
            vertices.Add(vertexpos);
            vertices.Add(vertexpos + dirxv);
            vertices.Add(vertexpos + dirzv);
            vertices.Add(vertexpos + dirxv + dirzv);
            // uvs
            float uvscale = 1f / 4;
            Vector2 texoffset = Vector2.zero;
            uvs.Add(uvscale * (texoffset + Vector2.zero));
            uvs.Add(uvscale * (texoffset + Vector2.right));
            uvs.Add(uvscale * (texoffset + Vector2.up));
            uvs.Add(uvscale * (texoffset + Vector2.one));
            // tris
            AddTriSquare(vcount, vcount + 2, vcount + 1, vcount + 3);
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
        new Vector3Int(0,1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,1),
        new Vector3Int(0,1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,1),
        };
    Vector3Int[] dirZs = new Vector3Int[6] {
        new Vector3Int(0,0,1),
        new Vector3Int(0,1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,1),
        new Vector3Int(0,1,0),
        new Vector3Int(1,0,0),
        };


}