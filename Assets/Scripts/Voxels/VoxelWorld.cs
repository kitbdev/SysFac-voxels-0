using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutils;

/// <summary>
/// handles chunks. all chunks need to be in a world
/// </summary>
public class VoxelWorld : MonoBehaviour
{
    public float voxelSize = 1;
    [SerializeField]
    List<VoxelChunk> world = new List<VoxelChunk>();
    [SerializeField]
    List<VoxelChunk> activeWorld = new List<VoxelChunk>();
}