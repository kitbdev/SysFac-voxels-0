namespace VoxelSystem {
    [System.Serializable]
    public struct VoxelMaterialId {
        public int id;
        public bool IsValid() => id >= 0;
        public static implicit operator int(VoxelMaterialId vmid) => vmid.id;
        public static implicit operator VoxelMaterialId(int nid) => new VoxelMaterialId() { id = nid };
        public override string ToString() => ((int)id).ToString();
    }
}