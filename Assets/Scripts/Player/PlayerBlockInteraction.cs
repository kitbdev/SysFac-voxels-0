using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelSystem;

public class PlayerBlockInteraction : MonoBehaviour {

    public float maxRayDist = 10;
    public LayerMask blockMask = Physics.DefaultRaycastLayers;

    [SerializeField] BlockTypeRef dirtref;// = new BlockTypeRef().SetBlockName("dirt");
    [SerializeField] BlockTypeRef airref;// = new BlockTypeRef().SetBlockName("air");
    // public float blockBreakDuration = 0.5f;
    // float blockBreakTimer = 0;
    [SerializeField] [Kutil.ReadOnly] Vector3Int targetBlockPos;
    [SerializeField] [Kutil.ReadOnly] Vector3 targetBlockNorm;

    public VoxelWorld world;
    Transform cam;

    private void Awake() {
        world ??= GameManager.Instance.mainWorld;
        cam = Camera.main.transform;
    }
    private void Update() {
        bool validTarget = CheckCursorBlock();
        if (validTarget && Mouse.current.leftButton.isPressed) {

        }
        if (validTarget && Mouse.current.leftButton.wasPressedThisFrame) {
            BreakBlock();
        }
        if (validTarget && Mouse.current.rightButton.wasReleasedThisFrame) {
            // place a block
            PlaceBlock();
        }
    }
    bool CheckCursorBlock() {
        Ray camRay = new Ray(cam.position, cam.forward);
        Debug.DrawRay(camRay.origin, camRay.direction * maxRayDist, Color.black, 0.1f);
        if (Physics.Raycast(camRay, out var hit, maxRayDist, blockMask, QueryTriggerInteraction.Ignore)) {
            targetBlockPos = world.WorldposToBlockpos(hit.collider.bounds.center);
            targetBlockNorm = world.transform.InverseTransformDirection(hit.normal).normalized;// for placing 
            // Debug.Log($"hit {hit.collider.name} bp:{blockPos}");
            BlockTypeRef blockTypeRef = GetBlockType(targetBlockPos);

            // if (blockType != null) {
            Voxel voxel = world.GetVoxelAt(targetBlockPos);
            BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
            Debug.Log($"hit {blockTypeRef} {blockTypeVoxelData.defVoxelData.voxel != null}");
            // }
            return true;
        }
        return false;
    }
    void PlaceBlock() {
        SetBlockType(targetBlockPos + Vector3Int.FloorToInt(targetBlockNorm), dirtref);
    }
    void BreakBlock() {
        SetBlockType(targetBlockPos, airref);
    }

    private BlockTypeRef GetBlockType(Vector3Int blockPos) {
        Voxel voxel = world.GetVoxelAt(blockPos);
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        return blockTypeVoxelData.blockTypeRef;
    }
    private void SetBlockType(Vector3Int blockPos, BlockTypeRef blocktype) {
        Voxel voxel = world.GetVoxelAt(blockPos);
        // Debug.Log($"Setting {blockTypeVoxelData} at {blockPos} to {blocktype} {blockTypeVoxelData.defVoxelData.voxel != null}");
        // blockTypeVoxelData.SetBlockType(blocktype);
        BlockTypeVoxelData.SetBlockType(voxel, blocktype);
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        Debug.Log($"Set {blockTypeVoxelData} at {blockPos} to {blocktype}");
        VoxelChunk voxelChunk = world.GetChunkWithBlock(blockPos);
        voxelChunk.Refresh(); 
    }
}