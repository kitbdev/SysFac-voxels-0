using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelSystem;

public class PlayerBlockInteraction : MonoBehaviour {

    public float maxRayDist = 10;
    public float scrollSensitivity = 1;
    public LayerMask blockMask = Physics.DefaultRaycastLayers;

    [SerializeField] BlockTypeRef dirtref;// = new BlockTypeRef().SetBlockName("dirt");
    [SerializeField] BlockTypeRef airref;// = new BlockTypeRef().SetBlockName("air");
    // public float blockBreakDuration = 0.5f;
    // float blockBreakTimer = 0;
    [SerializeField] Transform targetBlockFollow;
    [SerializeField] VoxelWorld handVoxelW;
    [SerializeField] [ReadOnly] Vector3Int targetBlockPos;
    [SerializeField] [ReadOnly] Vector3 targetBlockNorm;
    [SerializeField] [ReadOnly] int selectedBlockType;
    [ReadOnly] public float scrollAcc = 0;
    [SerializeField] [ReadOnly] BlockTypeRef selBlocktypeRef;
    [SerializeField] bool debugMode = false;

    public VoxelWorld world;
    Transform cam;

    private void Awake() {
        world ??= GameManager.Instance.mainWorld;
        cam = Camera.main.transform;
    }
    private void Start() {
        if (targetBlockFollow) {
            targetBlockFollow.localScale *= world.voxelSize;
        }
    }
    private void Update() {
        bool validTarget = CheckCursorBlock();
        if (targetBlockFollow) {
            if (validTarget) {
                targetBlockFollow.transform.position = world.BlockposToWorldPos(targetBlockPos);
            } else {
                targetBlockFollow.transform.position = Vector2.down * 10;
            }
        }
        if (validTarget && Mouse.current.leftButton.isPressed) {

        }
        // if (Keyboard.current.uKey.wasPressedThisFrame) {
        //     if (gameObject.TryGetComponent<StarterAssets.StarterAssetsInputs>(out var si)) {
        //         si.cursorInputForLook = !si.cursorInputForLook;
        //         Cursor.lockState = si.cursorInputForLook ? CursorLockMode.Locked : CursorLockMode.None;
        //         Cursor.visible = !si.cursorInputForLook;
        //         si.look = Vector2.zero;
        //     }
        // }
        scrollAcc += -Mouse.current.scroll.y.ReadValue() * scrollSensitivity;
        int nselectedBlockType = selectedBlockType + Mathf.RoundToInt(scrollAcc);
        if (selectedBlockType != nselectedBlockType) {
            nselectedBlockType = Mathf.Clamp(nselectedBlockType, 1, BlockManager.Instance.blockTypes.Length - 1);
            scrollAcc = 0;
            selectedBlockType = nselectedBlockType;

            selBlocktypeRef = new BlockTypeRef().SetBlockId(selectedBlockType);
        }
        if (handVoxelW != null && handVoxelW.activeChunks.Count > 0 && handVoxelW.GetChunkAt(Vector3Int.zero).IsPopulated()) {
            // preview block in hand
            VoxelChunk handChunk = handVoxelW.GetChunkAt(Vector3Int.zero);
            Voxel handVoxel = handChunk.GetLocalVoxelAt(0);
            // Debug.Log($"v{voxel.");
            BlockManager.SetBlockType(handVoxel, selBlocktypeRef);
            handChunk.Refresh();
        }
        if (validTarget && Mouse.current.leftButton.wasPressedThisFrame) {
            BreakBlock();
        }
        if (validTarget && Mouse.current.middleButton.wasPressedThisFrame) {
            PickTargetBlock();
        }
        if (validTarget && Mouse.current.rightButton.wasReleasedThisFrame) {
            // place a block
            PlaceBlock();
        }
    }
    bool CheckCursorBlock() {
        Ray camRay = new Ray(cam.position, cam.forward);
        // Debug.DrawRay(camRay.origin, camRay.direction * maxRayDist, Color.black, 0.1f);
        if (Physics.Raycast(camRay, out var hit, maxRayDist, blockMask, QueryTriggerInteraction.Collide)) {
            // targetBlockPos = world.WorldposToBlockpos(hit.collider.bounds.center);
            targetBlockPos = world.WorldposToBlockpos(hit.point + camRay.direction * 0.001f);
            Debug.DrawLine(camRay.origin, hit.point + camRay.direction * 0.001f, Color.black, 0.1f);
            targetBlockNorm = world.transform.InverseTransformDirection(hit.normal).normalized;// for placing 
            BlockTypeRef blockTypeRef = GetBlockType(targetBlockPos);

            if (debugMode) Debug.Log($"hit {hit.collider.name} bp:{targetBlockPos} btr:{blockTypeRef}");
            // if (blockType != null) {
            // Voxel voxel = world.GetVoxelAt(targetBlockPos);
            // BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
            // Debug.Log($"hit {blockTypeRef}");
            // }
            return true;
        }
        return false;
    }
    void PlaceBlock() {
        SetBlockType(targetBlockPos + Vector3Int.FloorToInt(targetBlockNorm), selBlocktypeRef);
    }
    void BreakBlock() {
        SetBlockType(targetBlockPos, airref);
    }
    void PickTargetBlock() {
        selBlocktypeRef = GetBlockType(targetBlockPos);
    }

    private BlockTypeRef GetBlockType(Vector3Int blockPos) {
        Voxel voxel = world.GetVoxelAt(blockPos);
        if (voxel == null) {
            // not a valid voxel
            return default;
        }
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        return blockTypeVoxelData.blockTypeRef;
    }
    private void SetBlockType(Vector3Int blockPos, BlockTypeRef blocktype) {
        Voxel voxel = world.GetVoxelAt(blockPos);
        if (voxel == null) {

            Debug.LogWarning("out of map, No voxel there!");
            return;
        }
        // Debug.Log($"Setting {blockTypeVoxelData} at {blockPos} to {blocktype} {blockTypeVoxelData.defVoxelData.voxel != null}");
        // blockTypeVoxelData.SetBlockType(blocktype);
        BlockManager.SetBlockType(voxel, blocktype);
        BlockTypeVoxelData blockTypeVoxelData = voxel.GetVoxelDataFor<BlockTypeVoxelData>();
        // Debug.Log($"Set {blockTypeVoxelData} at {blockPos} to {blocktype}");
        VoxelChunk voxelChunk = world.GetChunkWithBlock(blockPos);
        voxelChunk.Refresh(true);
    }
}