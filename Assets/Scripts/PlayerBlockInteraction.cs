using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlockInteraction : MonoBehaviour {

    public float maxRayDist = 10;
    public LayerMask blockMask = Physics.DefaultRaycastLayers;

    public float blockBreakDuration = 0.5f;
    float blockBreakTimer = 0;

    public VoxelWorld world;
    Transform cam;

    private void Awake() {
        world ??= GameManager.Instance.mainWorld;
        cam = Camera.main.transform;
    }
    private void Update() {
        if (Mouse.current.leftButton.isPressed) {
            CheckCursorBlock();
            // if (Time.time > blockBreakTimer) {

            // }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            // todo break block
            // VoxelChunk voxelChunk = world.GetChunkWithBlock(Vector3Int.zero);
		}
        if (Mouse.current.rightButton.wasReleasedThisFrame) {
            CheckCursorBlock();
			// place a block
        }
    }
    void CheckCursorBlock() {
        Ray camRay = new Ray(cam.position, cam.forward);
        Debug.DrawRay(camRay.origin, camRay.direction * maxRayDist, Color.black, 0.1f);
        if (Physics.Raycast(camRay, out var hit, maxRayDist, blockMask, QueryTriggerInteraction.Ignore)) {
            Vector3Int blockPos = world.WorldposToBlockpos(hit.collider.bounds.center);
            // Debug.Log($"hit {hit.collider.name} bp:{blockPos}");
            BlockType blockType = world.GetBlockTypeAt(blockPos);
            if (blockType != null) {
                // Debug.Log($" {blockType}");
            }
        }
    }
}