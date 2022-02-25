using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;

[SelectionBase, DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float sprintSpeed = 8;
    [SerializeField, Min(0f)] float moveAcceleration = 10;
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField] LayerMask groundLayerMask = Physics.DefaultRaycastLayers;
    [Header("Steps")]
    [SerializeField, Range(0f, 1f)] float stepSmoothing = 0.1f;
    [SerializeField, Range(0f, 5f)] float maxStepHeight = 0.3f;
    // jump and gravity
    [Header("Jump")]
    [SerializeField, Min(0)] float jumpHeight = 1;
    [SerializeField, Min(0)] float jumpDist = 2;
    [SerializeField, Min(0)] float jumpHeldHeight = 2.1f;
    [SerializeField, Min(0)] float jumpHeldDist = 5.6f;
    [SerializeField, Range(0f, 0.999f)] float fastFallSpeed = 0.1f;
    [SerializeField, Min(0)] float coyoteJumpDur = 0.1f;
    [SerializeField] float maxFallSpeed = 100;
    [Header("Water")]
    [SerializeField]
    float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)]
    float submergenceRange = 1f;
    [SerializeField, Range(0f, 10f)]
    float waterDrag = 1f;
    [SerializeField] float buoyancy = 1f;
    [SerializeField] LayerMask waterLayer;

    [Header("Out of Bounds")]
    [SerializeField] bool doOutOfBoundsCheck = true;
    [SerializeField] float oobMinHeight = -10;// todo full V3 min and max distances
    [SerializeField] public event System.Action onOOBEvent; //? unity event?

    [Space]
    [SerializeField, ReadOnly] bool isGrounded;
    [SerializeField, ReadOnly] Vector3 velocity;
    [SerializeField, ReadOnly] Vector3 connectionVelocity;

    [SerializeField, ReadOnly] float jumpDur;
    [SerializeField, ReadOnly] float heldJumpDur;
    [SerializeField, ReadOnly] float jumpVel;
    [SerializeField, ReadOnly] float jumpGrav;
    [SerializeField, ReadOnly] float holdJumpGrav;
    [SerializeField, ReadOnly] float ffallGrav;
    [SerializeField, ReadOnly] float curGrav;

    [SerializeField, ReadOnly] float submergence;

    [SerializeField] bool inWater => submergence > 0f;

    Vector3 connectionWorldPosition, connectionLocalPosition;
    float lastGroundedTime = 0f;
    // in physics steps
    int stepsSinceLastGrounded, stepsSinceLastJump;
    float minGroundNormalDP = 0;

    Rigidbody connectedBody, prevConnectedBody;

    [SerializeField, ReadOnly] PlayerInputControls playerInputControls;

    // [SerializeField] CapsuleCollider capsuleCollider;
    Rigidbody rb;

    private void OnValidate() {
        minGroundNormalDP = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        CalculateJumpVel();
    }
    private void Reset() {
        playerInputControls = GetComponent<PlayerInputControls>();
        // capsuleCollider = GetComponentInChildren<CapsuleCollider>();
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        playerInputControls = GetComponent<PlayerInputControls>();
        // capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        // headRot ??= transform.GetChild(0);
        OnValidate();
    }
    void CalculateJumpVel() {
        //http://www.mathforgameprogrammers.com/gdc2016/GDC2016_Pittman_Kyle_BuildingABetterJump.pdf
        float CalcJumpVel(float jumpHeight, float peakTime) {
            return 2 * jumpHeight / peakTime;
        }
        float CalcGrav(float jumpHeight, float peakTime) {
            return -2 * jumpHeight / (peakTime * peakTime);
        }
        jumpDur = jumpDist / moveSpeed;
        heldJumpDur = jumpHeldDist / moveSpeed;

        float ffrate = (fastFallSpeed + 1) / 2f;
        float pjumpDur = jumpDur * ffrate;
        float pheldJumpDur = heldJumpDur * ffrate;
        float pffallDur = jumpDur * (1f - ffrate);

        jumpVel = CalcJumpVel(jumpHeldHeight, pheldJumpDur);
        jumpGrav = CalcGrav(jumpHeight, pjumpDur);
        holdJumpGrav = CalcGrav(jumpHeldHeight, pheldJumpDur);
        // ? need ffallheldgrav?
        ffallGrav = CalcGrav(jumpHeight, pffallDur);

    }

    private void OnEnable() {
        playerInputControls.inputJumpReleased = true;
    }
    private void OnDisable() {

    }
    // private void Update() {
    // }
    void ClearPhysState() {
        isGrounded = false;
        prevConnectedBody = connectedBody;
        connectedBody = null;
    }
    private void FixedUpdate() {
        ClearPhysState();
        CheckGrounded();
        Move();
        // check if player somehow fell out of world
        if (doOutOfBoundsCheck) {
            if (transform.position.y <= oobMinHeight) {
                if (onOOBEvent != null) {
                    // respawn handled elsewhere
                    onOOBEvent.Invoke();
                } else {
                    float safeHeight = 10;
                    // force back up
                    transform.position = new Vector3(transform.position.x, safeHeight, transform.position.z);
                    rb.velocity = Vector3.zero;
                    velocity = Vector3.zero;
                }
            }
        }
        if (maxFallSpeed >= 0 && velocity.y <= -maxFallSpeed) {
            velocity.y = -maxFallSpeed;
            rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
        }
    }
    void Move() {
        velocity = rb.velocity;
        // input movement
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        Vector3 desVel = Vector3.zero;
        if (playerInputControls.inputMove.sqrMagnitude > 0.0001f) {
            desVel = new Vector3(playerInputControls.inputMove.x, 0, playerInputControls.inputMove.y);
            desVel = Vector3.ClampMagnitude(desVel, 1f);
            desVel = transform.TransformDirection(desVel);
            float targetSpeed = playerInputControls.inputSprint ? sprintSpeed : moveSpeed;
            desVel *= targetSpeed;
        }
        if (moveAcceleration > 0f) {
            // ignore y val
            // ? seperate accel and deaccel
            float vy = velocity.y;
            velocity = Vector3.Lerp(velocity, desVel, Time.deltaTime * moveAcceleration);
            velocity.y = vy;
        } else {
            velocity = desVel;
        }
        // gravity
        // if (inWater) {
        //     velocity.y += jumpGrav * ((1f - buoyancy * submergence) * Time.deltaTime);
        // } else
        if (isGrounded || SnapToGround()) {
            stepsSinceLastGrounded = 0;
            float groundGrav = -0.1f;
            curGrav = groundGrav;
            velocity.y = curGrav;
        } else {
            if (playerInputControls.inputJumpHold && rb.velocity.y > 0) {
                // player is holding jump, lower than usual gravity
                curGrav = holdJumpGrav;
            } else if (fastFallSpeed > 0 && rb.velocity.y <= 0) {
                // player is falling down, fast fall
                curGrav = ffallGrav;
            } else {
                // player is finishing normal jump arc after releasing jump, normal grav
                curGrav = jumpGrav;
            }
            velocity.y += curGrav * Time.deltaTime;
        }
        ClimbStep();

        // connected body
        if (connectedBody) {
            // filter small rigidbodies
            // if (connectedBody.isKinematic || connectedBody.mass >= rb.mass) {

        }
        // jump
        if (playerInputControls.inputJumpHold && playerInputControls.inputJumpReleased) {
            bool canJump = isGrounded || Time.time < lastGroundedTime + coyoteJumpDur;
            if (canJump) {
                // jump
                stepsSinceLastJump = 0;
                float jumpSpeed = jumpVel;
                if (velocity.y > 0f) {
                    jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
                }
                velocity.y += jumpSpeed;
                isGrounded = false;
                lastGroundedTime = Time.time;
            }
            playerInputControls.inputJumpReleased = false;
        }

        if (inWater) {
            velocity *= 1f - waterDrag * submergence * Time.deltaTime;
        }
        submergence = 0;
        rb.velocity = velocity;
    }
    bool SnapToGround() {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2 || inWater) {
            return false;
        }
        if (!Physics.SphereCast(rb.position, 0.1f, Vector3.down, out RaycastHit hit, 1, groundLayerMask, QueryTriggerInteraction.Ignore)) {
            return false;
        }
        if (hit.normal.y < minGroundNormalDP) {
            return false;
        }
        float speed = velocity.magnitude;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) {
            // Debug.Log("Snapping to ground");
            Vector3 nvel = (velocity - hit.normal * dot).normalized * speed;
            if (!float.IsNaN(nvel.x) && !float.IsNaN(nvel.y) && !float.IsNaN(nvel.z)) {
                // todo slight chance for invalid value, why?
                velocity = nvel;
                return true;
            } else {
                Debug.LogWarning($"snap to ground invalid vel {nvel}");
            }
        }
        return false;
    }

    bool ClimbStep() {
        // if (!isGrounded) {
        //     // ? can we step while in air? sure
        //     return false;
        // }
        Vector3 checkDir = velocity.normalized;
        checkDir.y = 0;
        if (checkDir.sqrMagnitude <= 0.1f) {
            // not moving horizontally enough to need to step
            return false;
        }
        Vector3 origin = transform.position + Vector3.up * 0.02f;
        Ray stepCheckRay = new Ray(origin, checkDir);
        float dist = 0.4f;
        float neededSpace = 0.2f;
        // Debug.DrawRay(stepCheckRay.origin, stepCheckRay.direction * dist, Color.red);
        if (Physics.Raycast(stepCheckRay, out var hit, dist, groundLayerMask, QueryTriggerInteraction.Ignore)) {
            // we are being blocked
            Ray stepHighCheckRay = new Ray(transform.position + Vector3.up * maxStepHeight, checkDir);
            // todo capsule cast to check if we have clearance
            // var p1 = capsuleCollider.center + transform.position + Vector3.up * maxStepHeight;
            // var p2 = p1;
            // p1.y += -capsuleCollider.height / 2f + capsuleCollider.radius;
            // p2.y -= -capsuleCollider.height / 2f + capsuleCollider.radius;
            // if (!Physics.CapsuleCast(p1,p2,capsuleCollider.radius,checkDir, dist, groundLayerMask,QueryTriggerInteraction.Ignore)){
            if (!Physics.Raycast(stepHighCheckRay, dist + neededSpace, groundLayerMask, QueryTriggerInteraction.Ignore)) {
                // enough space to step
                // top down check to get the height
                Ray stepDownRay = new Ray(stepHighCheckRay.origin + checkDir * (dist + neededSpace / 2f), Vector3.down);
                if (Physics.Raycast(stepDownRay, out var tophit, maxStepHeight + 0.02f, groundLayerMask, QueryTriggerInteraction.Ignore)) {
                    float stepHeight = tophit.point.y - transform.position.y;
                    // rb.position += Vector3.up * stepSmoothing;
                    // rb.AddForce(transform.up * stepSmoothing, ForceMode.VelocityChange);
                    float stepVel = Mathf.Sqrt(-2f * jumpGrav * stepHeight);
                    velocity.y += stepVel * (1f - stepSmoothing);
                    // stepsSinceLastJump = 0;// stop snap to ground
                    // Debug.Log($"Stepping! {stepHeight} v{stepVel}");
                    return true;
                }
            }
        }
        return false;
    }
    void CheckGrounded() {
        // sets isGrounded to true if we detect an object below us with a vertical facing normal
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rad = 0.08f;
        float dist = 0.1f;
        // Debug.DrawRay(origin, Vector3.down * (dist + rad), Color.red, 0.05f);
        if (Physics.SphereCast(origin, rad, Vector3.down, out var hit, dist, groundLayerMask, QueryTriggerInteraction.Ignore)) {
            if (hit.normal.y >= minGroundNormalDP) {
                isGrounded = true;
                lastGroundedTime = Time.time;
                connectedBody = hit.rigidbody;
            }
        }
    }

    // https://catlikecoding.com/unity/tutorials/movement/swimming/
    void EvaluateSubmergence() {
        if (Physics.Raycast(
            rb.position + Vector3.up * submergenceOffset,
            Vector3.down, out RaycastHit hit, submergenceRange + 1f,
            waterLayer, QueryTriggerInteraction.Collide
        )) {
            submergence = Mathf.Min(0f, 1f - hit.distance / submergenceRange);
        } else {
            submergence = 1f;
        }
    }
    private void OnTriggerEnter(Collider other) {
        // check water
        if (((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
        }
    }
    void OnTriggerStay(Collider other) {
        if (((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
        }
    }
    private void OnTriggerExit(Collider other) {
        if (((Layer)other.gameObject.layer).InLayerMask(waterLayer)) {
            EvaluateSubmergence();
        }
    }
}