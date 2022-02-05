using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutils;

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
            Voxel voxel = new Voxel { chunk = this, position = position };
            voxels[i] = voxel;
            // voxel.index = i;
            // voxel.value = 0;
            // if (WorldPosition(i).y < 2) 
        }
        // visuals.CreateMesh(this);
    }
}