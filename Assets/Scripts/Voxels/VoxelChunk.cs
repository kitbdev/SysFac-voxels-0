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
    [SerializeField, ReadOnly] Voxel[] _voxels;
    [SerializeField, HideInInspector] VoxelWorld _world;

    private VoxelRenderer visuals;

    public bool showDebug = false;

    public Vector3Int chunkPos { get => _chunkPos; private set => _chunkPos = value; }
    public int resolution { get => _resolution; private set => _resolution = value; }
    public Voxel[] voxels { get => _voxels; private set => _voxels = value; }
    public VoxelWorld world { get => _world; private set => _world = value; }

    public int floorArea => resolution * resolution;
    public int volume => resolution * resolution * resolution;

    public void Initialize(VoxelWorld world, Vector3Int chunkPos, int resolution) {
        this.world = world;
        this.chunkPos = chunkPos;
        this.resolution = resolution;
        // this.voxelSize = voxelSize;
        visuals = GetComponent<VoxelRenderer>();
        // if (!visuals) {
        //     var mf = gameObject.AddComponent<MeshFilter>();
        //     var mr = gameObject.AddComponent<MeshRenderer>();
        //     visuals = gameObject.AddComponent<MeshGen>();
        // }
        visuals.Initialize(this);
        FillVoxelsNew();
    }
    protected void FillVoxelsNew() {
        voxels = new Voxel[volume];
        for (int i = 0; i < volume; i++) {
            // y,z,x
            Vector3Int position = Pos(i);
            Voxel voxel = new Voxel { };
            voxel.shape = Voxel.VoxelShape.cube;
            voxels[i] = voxel;
            // voxel.index = i;
            // if (WorldPosition(i).y < 2) 
        }
        // visuals.CreateMesh(this);
    }
    public void Clear() {
        voxels = null;
    }

    [ContextMenu("ResetValue")]
    public void ResetValues() {
        for (int i = 0; i < volume; i++) {
            voxels[i].ResetToDefaults();
        }
        // Refresh(true);
    }
    public void SetVoxel(int index, Voxel voxel) {
        voxels[index].CopyValues(voxel);
    }
    public void SetAll(Voxel voxel) {
        for (int i = 0; i < volume; i++) {
            voxels[i].CopyValues(voxel);
        }
    }
    public void SetData(Voxel[] data) {
        if (data.Length != volume) {
            Debug.LogWarning($"Error in Chunk SetData size {volume} vs {data.Length}", this);
            return;
        }
        for (int i = 0; i < volume; i++) {
            voxels[i].CopyValues(data[i]);
        }
    }
    public void Refresh(bool andNeighbors = false) {
        visuals.UpdateMesh();
        if (andNeighbors) {
            // todo
            // for (int i = 1; i < VoxelCube.cubePositions.Length; i++) {
            //     Vector3Int dir = -VoxelCube.cubePositions[i];
            //     VoxelChunk neighbor = world.GetNeighbor(this, dir);
            //     if (neighbor) {
            //         neighbor.Refresh(false);
            //     }
            // }
        }
    }

    /// <summary>
    /// local position of voxel at index i
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
    public Voxel GetLocalVoxelAt(Vector3Int pos) => GetLocalVoxelAt(IndexAt(pos));
    public Voxel GetLocalVoxelAt(int index) {
        if (index >= 0 && index < voxels.Length)
            return voxels[index];
        else
            return null;
    }
    private void OnDrawGizmos() {
        if (!showDebug || !world) {
            return;
        }
        Gizmos.color = new Color(0, 0.8f, 0.5f, 0.1f);
        Gizmos.DrawCube(world.ChunkposToWorldposCenter(chunkPos), world.chunkSize * Vector3.one);
        Gizmos.color = Color.white;
    }
}