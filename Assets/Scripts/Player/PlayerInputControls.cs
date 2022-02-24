using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

[SelectionBase, DisallowMultipleComponent]
public class PlayerInputControls : MonoBehaviour {

    [Header("Input")]
    [SerializeField, ReadOnly] public Vector2 inputLook;
    [SerializeField, ReadOnly] public Vector2 inputMove;
    [SerializeField, ReadOnly] public bool inputJumpHold;
    // used to jump only when
    [SerializeField, ReadOnly] public bool inputJumpReleased;
    [SerializeField, ReadOnly] public bool inputShoot;
    [SerializeField, ReadOnly] public bool inputSprint;
    Controls controls;

    private void OnEnable() {
        controls = new Controls();
        controls.Enable();
        controls.Player.Look.performed += c => { inputLook = c.ReadValue<Vector2>(); };
        controls.Player.Look.canceled += c => { inputLook = Vector2.zero; };
        controls.Player.Move.performed += c => { inputMove = c.ReadValue<Vector2>(); };
        controls.Player.Move.canceled += c => { inputMove = Vector2.zero; };
        controls.Player.Jump.performed += c => { inputJumpHold = true; };
        controls.Player.Jump.canceled += c => { inputJumpHold = false; inputJumpReleased = true; };
        controls.Player.Sprint.performed += c => { inputSprint = true; };
        controls.Player.Sprint.canceled += c => { inputSprint = false; };
        controls.Player.Fire.performed += c => { };
        // inputJumpReleased = true;
    }
    private void OnDisable() {
        controls.Dispose();
    }
}