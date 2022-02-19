using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem {
    public class VoxelToolSphere : VoxelToolBase {
        public struct VoxelToolEdit {
            public System.Action<BoundsInt, Vector3Int, Voxel> editAction;
            public void Edit(BoundsInt bounds, Vector3Int bpos, Voxel voxel) {
//?
            }
            public System.Func<Voxel, Voxel> modifyFunc;// todo better way
            public System.Func<Voxel, bool> blockQuery;
            // public static implicit VoxelToolEdit operator(System.Action<BoundsInt, Vector3Int, Voxel> action) => new VoxelToolEdit(){editAction=action};
        }
        public struct VoxelToolEdit<TVData> where TVData : VoxelData {
            public System.Action<BoundsInt, Vector3Int, TVData> editAction;
        }
        VoxelToolEdit SphereEdit() {
            VoxelToolEdit voxelToolEdit = new VoxelToolEdit();
            voxelToolEdit.editAction = (bounds, vpos, voxel) => {
                // voxel.SetVoxelMaterialId()
            };
            return voxelToolEdit;
        }

    }
}