using UnityEngine;

[System.Serializable]
public class BlockType {
	public string idname;
	public string displayName;
	public Voxel.VoxelShape shape;
	public int textureId;
	[Min(0)]
	public int maxStack;
	public int itemid;
}