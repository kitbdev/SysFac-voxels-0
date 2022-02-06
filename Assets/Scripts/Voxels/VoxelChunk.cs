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
        if (!visuals) {
            //     var mf = gameObject.AddComponent<MeshFilter>();
            //     var mr = gameObject.AddComponent<MeshRenderer>();
            visuals = gameObject.AddComponent<VoxelRenderer>();
        }
        if (world.enableCollision) {
            if (!world.useBoxColliders) {
                gameObject.AddComponent<MeshCollider>();
            } else {
                if (gameObject.TryGetComponent<MeshCollider>(out var mcol)) {
                    if (Application.isPlaying) {
                        Destroy(mcol);
                    } else {
                        DestroyImmediate(mcol);
                    }
                }
            }
        }
        visuals.Initialize(this);
        FillVoxelsNew();
    }
    protected void FillVoxelsNew() {
        voxels = new Voxel[volume];
        for (int i = 0; i < volume; i++) {
            // y,z,x
            Vector3Int position = GetLocalPos(i);
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
        RemoveBoxColliders();
        if (world.enableCollision) {
            if (world.useBoxColliders) {
                AddBoxColliders();
            }
        }
        if (andNeighbors) {
            // updates the 7 neighbors behind, below, and left (otherwise recursion?)
            for (int i = 1; i < Voxel.cubePositions.Length; i++) {
                Vector3Int dir = -Voxel.cubePositions[i];
                VoxelChunk neighbor = world.GetNeighbor(this, dir);
                if (neighbor) {
                    neighbor.Refresh(false);
                }
            }
        }
    }
    public void LocalRefresh(Vector3Int pos, int size){

    }

    private void AddBoxColliders() {
        var collgo = new GameObject($"chunk {chunkPos} col");
        collgo.transform.parent = transform;
        collgo.transform.localPosition = Vector3.zero;
        List<Bounds> surfaceVoxels = new List<Bounds>();
        for (int i = 0; i < volume; i++) {
            Vector3Int vpos = GetLocalPos(i);
            // todo blockdata for collision on/off
            Voxel voxel = GetLocalVoxelAt(i);
            if (voxel.blockId == 0) {
                continue;
            }
            bool hidden = IsVoxelHidden(vpos);
            if (!hidden) {
                surfaceVoxels.Add(new Bounds(vpos, Vector3.one));
            }
        }
        foreach (var survox in surfaceVoxels) {
            BoxCollider boxCollider = collgo.AddComponent<BoxCollider>();
            boxCollider.center = survox.center;
            boxCollider.size = survox.size;
        }
    }
    private void RemoveBoxColliders() {
        if (transform.childCount > 0) {
            if (Application.isPlaying) {
                Destroy(transform.GetChild(0).gameObject);
            } else {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

    /// <summary>
    /// Includes neighbors in other chunks
    /// </summary>
    /// <param name="pos">voxel position</param>
    /// <returns></returns>
    public Voxel GetVoxelN(Vector3Int localpos) {
        Voxel voxel = GetLocalVoxelAt(localpos);
        if (voxel != null) {
            return voxel;
        }
        Vector3Int cdir = new Vector3Int(
            localpos.x >= resolution ? 1 : (localpos.x < 0 ? -1 : 0),
            localpos.y >= resolution ? 1 : (localpos.y < 0 ? -1 : 0),
            localpos.z >= resolution ? 1 : (localpos.z < 0 ? -1 : 0)
        );
        localpos -= cdir * resolution;
        Voxel copy = world.GetChunkAt(chunkPos + cdir)?.GetVoxelN(localpos);
        return copy;
    }

    /// <summary>
    /// True if voxel has nontransparent voxels on all sides
    /// </summary>
    /// <param name="vpos"></param>
    /// <returns></returns>
    public bool IsVoxelHidden(Vector3Int vpos) {
        bool hidden = true;
        foreach (Vector3Int dir in Voxel.unitDirs) {
            Voxel voxel = GetVoxelN(vpos + dir);
            if (voxel == null || voxel.isTransparent) {
                hidden = false;
                break;
            }
        }
        return hidden;
    }

    /// <summary>
    /// local position of voxel at index i
    /// </summary>
    /// <param name="i"></param>
    /// <returns>v3int position</returns>
    public Vector3Int GetLocalPos(int i) {
        Vector3Int pos = Vector3Int.zero;
        pos.x = i % resolution;
        pos.z = (i / resolution) % resolution;
        pos.y = i / floorArea;
        return pos;
    }
    public int IndexAt(Vector3Int localpos) => IndexAt(localpos.x, localpos.y, localpos.z);
    public int IndexAt(int x, int y, int z) {
        if (x < 0 || x >= resolution || y < 0 || y >= resolution || z < 0 || z >= resolution)
            return -1;
        int index = x + y * floorArea + z * resolution;
        return index;
    }
    public Voxel GetLocalVoxelAt(Vector3Int localpos) => GetLocalVoxelAt(IndexAt(localpos));
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