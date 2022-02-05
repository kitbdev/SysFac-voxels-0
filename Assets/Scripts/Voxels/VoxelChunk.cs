using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;

/// <summary>
/// handles a single chunk of voxels
/// </summary>
public class VoxelChunk : MonoBehaviour {

    [SerializeField, ReadOnly] Vector3Int _chunkPos;
    int _resolution;
    [SerializeField, HideInInspector] Voxel[] _voxels;
    [SerializeField, HideInInspector] VoxelWorld _world;

    public bool showDebug = false;

    public Vector3Int chunkPos { get => _chunkPos; private set => _chunkPos = value; }
    public int resolution { get => _resolution; private set => _resolution = value; }
    public Voxel[] voxels { get => _voxels; private set => _voxels = value; }
    public VoxelWorld world { get => _world; private set => _world = value; }

    public int floorArea => resolution * resolution;
    public int volume => resolution * resolution * resolution;

    public void Initialize(VoxelWorld world, Vector3Int chunkPos, int resolution, float voxelSize) {
        this.world = world;
        this.chunkPos = chunkPos;
        this.resolution = resolution;
        // this.voxelSize = voxelSize;
        // visuals = GetComponent<MeshGen>();
        // if (!visuals) {
        //     var mf = gameObject.AddComponent<MeshFilter>();
        //     var mr = gameObject.AddComponent<MeshRenderer>();
        //     visuals = gameObject.AddComponent<MeshGen>();
        // }
        FillVoxelsNew();
    }
    protected void FillVoxelsNew() {
        voxels = new Voxel[volume];
        for (int i = 0; i < volume; i++) {
            // y,z,x
            Vector3Int position = Pos(i);
            Voxel voxel = new Voxel{ };
            voxels[i] = voxel;
            // voxel.index = i;
            // voxel.value = 0;
            // if (WorldPosition(i).y < 2) 
        }
        // visuals.CreateMesh(this);
    }
    /// <summary>
    /// position of voxel at index i
    /// </summary>
    /// <param name="i"></param>
    /// <returns>v3int position</returns>
    public Vector3Int Pos(int i) {
        Vector3Int pos = Vector3Int.zero;
        pos.x = i % resolution;
        pos.z = (i / resolution) % resolution;
        pos.y = i / floorArea;
        return pos;
    }
    public int IndexAt(Vector3Int pos) => IndexAt(pos.x, pos.y, pos.z);
    public int IndexAt(int x, int y, int z) {
        if (x < 0 || x >= resolution || y < 0 || y >= resolution || z < 0 || z >= resolution)
            return -1;
        int index = x + y * floorArea + z * resolution;
        return index;
    }
}