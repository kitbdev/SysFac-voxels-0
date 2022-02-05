using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;

/// <summary>
/// holds data for a single voxel
/// </summary>
[System.Serializable]
public class Voxel {
	public int textureId;
	public enum VoxelShape {
		none,
		cube,
		xfull,
		xsmall,
		customcubey,
		customcubexyz,
		custom,
	}
	public VoxelShape shape;
	public Color tint;
	// todo lighting data?
	// todo anim data
}