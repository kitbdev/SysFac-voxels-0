using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

public class FPSCamera : MonoBehaviour {

    [SerializeField] float turnSpeedX = 1;
    [SerializeField] float turnSpeedY = 1;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] float topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] float bottomClamp = -90.0f;
    // /// <summary> for settings </summary>
    // [ReadOnly] public float turnSpeedXMod = 1;
    [SerializeField] Transform yawRoter;
    [SerializeField] Transform pitchRoter;
    [SerializeField] PlayerInputControls playerInputControls;

    [SerializeField, ReadOnly] float targetPitch = 0;

    private void Reset() {
        playerInputControls = GetComponent<PlayerInputControls>();
        yawRoter = transform;
        pitchRoter = transform;
    }
    private void Awake() {
        yawRoter ??= transform;
        // pitchRoter ??= transform;
        playerInputControls ??= GetComponent<PlayerInputControls>();
    }

    private void Update() {
        if (Time.timeScale == 0) return;
        if (playerInputControls.inputLook.sqrMagnitude >= 0.01f) {
            // todo times deltatime if using a controller?
            if (pitchRoter) {
                targetPitch += -playerInputControls.inputLook.y * turnSpeedY;
                targetPitch = ClampAngle(targetPitch, bottomClamp, topClamp);
                pitchRoter.localRotation = Quaternion.Euler(targetPitch, 0f, 0f);
            }
            // * turnSpeedXMod;
            float yawrot = playerInputControls.inputLook.x * turnSpeedX;
            yawRoter.Rotate(0f, yawrot, 0f, Space.Self);
        }
    }
    void ResetPitch(){
        targetPitch = 0;
        pitchRoter.localRotation = Quaternion.Euler(targetPitch, 0f, 0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}