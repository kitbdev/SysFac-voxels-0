using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingRb : MonoBehaviour {

    [SerializeField]
    float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)]
    float submergenceRange = 1f;
    [SerializeField, Range(0f, 10f)]
    float waterDrag = 1f;
    [SerializeField] float buoyancy = 1f;
    [SerializeField] LayerMask waterLayer;
    [SerializeField, ReadOnly] float submergence;
    [SerializeField, ReadOnly] bool didUseGravity;

    [SerializeField] bool inWater => submergence > 0f;

    Rigidbody rb;

    private void Reset() {
        // waterLayer = Layer.NameToLayer("Water");
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        // rb.useGravity = false;
    }
    private void FixedUpdate() {
        if (inWater) {
            float drag = Mathf.Max(0f, 1f - waterDrag * submergence * Time.deltaTime);
            rb.velocity *= drag;
			rb.angularVelocity *= drag;
            rb.AddForce(
                Physics.gravity * -(buoyancy * submergence),
                ForceMode.Acceleration
            );
            submergence = 0f;
        }
    }
    void EvaluateSubmergence() {
        if (!rb.IsSleeping() && Physics.Raycast(
            rb.position + Vector3.up * submergenceOffset,
            -Vector3.up, out RaycastHit hit, submergenceRange + 1f,
            waterLayer, QueryTriggerInteraction.Collide
        )) {
            submergence = 1f - hit.distance / submergenceRange;
        } else {
            submergence = 1f;
        }
    }
    private void OnTriggerEnter(Collider other) {
        // check water
        if (((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
            didUseGravity = rb.useGravity;
            // rb.useGravity = false;//todo
        }
    }
    void OnTriggerStay(Collider other) {
        if (!rb.IsSleeping() &&
            ((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
        }
    }
    private void OnTriggerExit(Collider other) {
        if (((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
            rb.useGravity = didUseGravity;
        }
    }
}