using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using VoxelSystem;

public class DebugShowBlockData : MonoBehaviour {

    [SerializeField] bool isOn = false;
    [SerializeField] InputActionReference toggleButton;
    [SerializeField] float maxRayDist = 100;
    [SerializeField] LayerMask blockMask = Physics.DefaultRaycastLayers;
    [SerializeField] VoxelWorld world;
    [SerializeField] TMP_Text tmpText;
    [SerializeField] GameObject debugGO;

    [SerializeField, ReadOnly] Vector3Int targetBlockPos;
    [SerializeField, ReadOnly] Voxel targetVoxel;

    Transform cam;
    private void Reset() {
        world ??= GameManager.Instance.mainWorld;
    }
    private void Awake() {
        world ??= GameManager.Instance.mainWorld;
        cam = Camera.main.transform;
        ClearText();
    }
    private void OnEnable() {
        toggleButton.action.Enable();
        toggleButton.action.performed += c => {
            isOn = !isOn;
            if (!isOn) {
                ClearText();
            }
        };
    }
    private void Update() {
        if (isOn) {
            // check mouse ray
            Ray camRay = new Ray(cam.position, cam.forward);
            // Debug.DrawRay(camRay.origin, camRay.direction * maxRayDist, Color.black, 0.1f);
            if (Physics.Raycast(camRay, out var hit, maxRayDist, blockMask, QueryTriggerInteraction.Ignore)) {
                // targetBlockPos = world.WorldposToBlockpos(hit.collider.bounds.center);
                targetBlockPos = world.WorldposToBlockpos(hit.point + camRay.direction * 0.001f);
                targetVoxel = world.GetVoxelAt(targetBlockPos);
            }else{
                // targetVoxel = null;
            }
            UpdateText();
        }
    }
    void UpdateText() {
        if (debugGO) debugGO.SetActive(true);
        if (tmpText) {
            tmpText.text = $"{targetBlockPos}: {targetVoxel?.ToStringFull()}" ?? "None";
        }
    }
    void ClearText() {
        if (debugGO) debugGO.SetActive(false);
        if (tmpText) {
            tmpText.text = "";
        }
    }
}