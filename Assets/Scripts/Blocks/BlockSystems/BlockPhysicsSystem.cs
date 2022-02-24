using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;
using Kutil;

public class BlockPhysicsSystem : MonoBehaviour {
    
}

public struct WindVD : VoxelData {
    // affect neighbors
}
// like for wind or water
public struct PushDataVD : VoxelData {
    public float force;
    public Vector3 direction;
}
public class PushableEntity : MonoBehaviour {
    Rigidbody rb;
}