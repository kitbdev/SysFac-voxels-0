using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using Unity.Mathematics;
using System.Linq;

namespace VoxelSystem {
    [System.Serializable]
    class VoxelVolume {
        [SerializeField, Min(1)]
        int _xResolution,
            _zResolution,
            _yResolution;
        [SerializeField, ReadOnly] Voxel[] _voxels;
        // [SerializeField, ReadOnly]
        // private VoxelRenderer visuals;// ? this here

        public Voxel[] voxels { get => _voxels; protected set => _voxels = value; }
        public int xResolution { get => _xResolution; protected set => _xResolution = value; }
        public int zResolution { get => _zResolution; protected set => _zResolution = value; }
        public int yResolution { get => _yResolution; protected set => _yResolution = value; }
        public int volume => xResolution * zResolution * yResolution;
        public int[] resolutions => new int[] { xResolution, yResolution, zResolution };

        public VoxelVolume(int xResolution, int zResolution, int yResolution) {
            this.xResolution = xResolution;
            this.zResolution = zResolution;
            this.yResolution = yResolution;
            this.voxels = new Voxel[0];
            // todo populate?
        }
        public void PopulateWithNewVoxels(VoxelMaterialId voxelMaterialId, VoxelData[] neededDatas = null) {
            voxels = new Voxel[volume];
            if (neededDatas == null) neededDatas = new VoxelData[0];
            for (int i = 0; i < volume; i++) {
                // voxeldata is a struct, so it is passed by value and doesnt need to be copied individually
                Voxel voxel = new Voxel(voxelMaterialId, neededDatas.ToArray());
                voxels[i] = voxel;
            }
        }
        public void PopulateWithNewVoxels(VoxelMaterialId[] voxelMaterialIds, VoxelData[] neededDatas = null) {
            if (voxelMaterialIds.Length != volume) return;
            if (neededDatas == null) neededDatas = new VoxelData[0];
            voxels = new Voxel[volume];
            for (int i = 0; i < volume; i++) {
                // voxeldata is a struct, so it is passed by value and doesnt need to be copied individually
                Voxel voxel = new Voxel(voxelMaterialIds[i], neededDatas.ToArray());
                voxels[i] = voxel;
            }
        }
        public void PopulateWithExistingVoxels(Voxel[] newVoxels) {
            if (newVoxels.Length != volume) {
                Debug.LogError($"Cannot set voxels to voxel volume, wrong size {newVoxels.Length} vs {volume}");
                return;
            }
            voxels = newVoxels;
        }
        public void ClearAllVoxels() {
            this.voxels = new Voxel[0];
        }

        public Voxel GetVoxelAt(int index) {
            if (index < 0 || index >= voxels.Length) {
                return null;
            } else {
                return voxels[index];
            }
        }
        public Voxel GetVoxelAt(Vector3Int pos) {
            return GetVoxelAt(IndexAt(pos));
        }
        public int IndexAt(Vector3Int pos) {
            return IndexAt(pos, xResolution, yResolution, zResolution);
        }
        public Vector3Int GetLocalPos(int index) {
            return GetLocalPos(index, xResolution, yResolution, zResolution);
        }

        public static int IndexAt(Vector3Int pos, int resolution) {
            return IndexAt(pos, resolution, resolution, resolution);
        }
        public static int IndexAt(Vector3Int pos, int xResolution, int yResolution, int zResolution) {
            if (pos.x < 0 || pos.x >= xResolution || pos.y < 0 || pos.y >= yResolution || pos.z < 0 || pos.z >= zResolution)
                return -1;
            // in x -> z -> y order
            return pos.x + pos.z * xResolution + pos.y * xResolution * zResolution;
        }
        public static Vector3Int GetLocalPos(int index, int resolution) {
            // assumes all resolutions are the same size
            return GetLocalPos(index, resolution, resolution);
        }
        public static Vector3Int GetLocalPos(int index, int xResolution, int yResolution, int zResolution) {
            // technically y resolution isnt needed
            return GetLocalPos(index, xResolution, zResolution);
        }
        public static Vector3Int GetLocalPos(int index, int xResolution, int zResolution) {
            Vector3Int pos = Vector3Int.zero;
            // todo test
            pos.y = index / (xResolution * zResolution);
            index -= (pos.y * xResolution * zResolution);
            pos.z = index / xResolution;
            pos.x = index % xResolution;
            return pos;
        }
    }
}