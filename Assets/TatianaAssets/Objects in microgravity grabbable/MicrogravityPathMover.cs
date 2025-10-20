///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 18.10.2025
///   Created: 20.10.2025 // 

///   microgravity movement in predefined paths

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MicrogravityPathMover : MonoBehaviour
{
    [Header("Path Settings")]
    private Transform initialPosition;
    [SerializeField] private Transform[] targetPositions;
    [SerializeField] private float moveSpeed = 0.05f;
    [SerializeField] private float arriveThreshold = 0.01f;

    [Header("Control")]
    public bool isMovingFloating = true;
    public bool isRotatingFloating = true;

    [Header("Smooth Step Settings")]
    [SerializeField] private float stepResolutionFactor = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private float randomRotationVariance = 0.5f;

    [Header("Throw Settings")]
    [SerializeField] private float throwSpeedCoefficient = 0.5f;
    [SerializeField] private float throwDecay = 0.98f;

    [Header("Push Settings (XR Interactor)")]
    [SerializeField, Tooltip("How strong hand pushes are when near object.")]
    private float pushForce = 0.4f;
    [SerializeField, Tooltip("How far hand can be for push effect (in meters).")]
    private float pushRange = 0.2f;
    [SerializeField, Tooltip("How fast the push velocity decays each frame.")]
    private float pushDecay = 0.98f;
    [SerializeField, Tooltip("Maximum push speed of object.")]
    private float maxPushSpeed = 1.0f;

    [Header("Collision Settings")]
    [SerializeField, Tooltip("Tag of colliders that should stop this object when touched.")]
    private string stopTag = "StoppingFloatingObject";

    [SerializeField, Tooltip("How far to slightly move back on collision.")]
    private float collisionBackDistance = 0.1f; // ... m back

    [SerializeField, Tooltip("Enable or disable collision checking.")]
    public bool isCheckingCollisions = true;

    private List<Collider> childColliders = new();
    private int currentTargetIndex = 0;
    private float referenceSize = 0.1f;
    private Vector3 rotationAxis = Vector3.up;
    private float currentRotationSpeed;

    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    private bool isThrown = false;
    private Vector3 throwVelocity;

    private bool isPushed = false;
    private Vector3 pushVelocity;

    private XRGrabInteractable grabInteractable;
    private Dictionary<XRBaseInteractor, Vector3> lastHandPositions = new();

    private bool isBlocked = false;
    private bool collisionActive = false; // NEW — to delay trigger reaction until startup settles

    private bool isBacking = false;
    private Vector3 backVelocity = Vector3.zero;
    [SerializeField] private float backSpeed = 0.1f; // initial speed of backward movement
    [SerializeField] private float backDecay = 0.9f; // how fast the back movement slows down

    private bool ignorePushThisFrame = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        childColliders.AddRange(GetComponentsInChildren<Collider>());
        if (TryGetComponent(out Collider parentCol))
            childColliders.Remove(parentCol); // don't count main trigger collider

        if (childColliders.Count > 0)
        {
            Bounds combined = childColliders[0].bounds;
            for (int i = 1; i < childColliders.Count; i++)
                combined.Encapsulate(childColliders[i].bounds);
            referenceSize = combined.extents.magnitude * stepResolutionFactor;
        }

        rotationAxis = UnityEngine.Random.onUnitSphere;
        currentRotationSpeed = rotationSpeed;

        if (randomizeRotation)
            currentRotationSpeed *= UnityEngine.Random.Range(1f - randomRotationVariance, 1f + randomRotationVariance);
    }

    private void Start()
    {
        if (initialPosition == null)
            initialPosition = new GameObject($"{name}_Start").transform;

        initialPosition.position = transform.position;
        lastPosition = transform.position;

        // Delay enabling trigger blocking to avoid startup overlap false positives
        Invoke(nameof(EnableCollisionDetection), 0.5f);
    }

    private void EnableCollisionDetection() => collisionActive = true;

    private void FixedUpdate()
    {
        if (isBacking)
        {
            transform.position += backVelocity * Time.fixedDeltaTime;
            backVelocity *= backDecay;

            if (backVelocity.magnitude < 0.001f)
            {
                backVelocity = Vector3.zero;
                isBacking = false;
            }
        }

        if (isBlocked)
            return;

        currentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        if (isThrown)
        {
            ApplyThrowMotion();
            return;
        }

        UpdateHandPush();

        if (isPushed)
        {
            ApplyPushMotion();
            return;
        }

        if (isMovingFloating)
            MoveAlongPath();

        if (isRotatingFloating)
            RotateObject();
    }


    private void MoveAlongPath()
    {
        if (targetPositions == null || targetPositions.Length == 0)
            return;

        Transform target = targetPositions[currentTargetIndex];
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance < arriveThreshold)
        {
            currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Length;

            if (randomizeRotation)
            {
                rotationAxis = UnityEngine.Random.onUnitSphere;
                currentRotationSpeed = rotationSpeed *
                    UnityEngine.Random.Range(1f - randomRotationVariance, 1f + randomRotationVariance);
            }
            return;
        }

        Vector3 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
        int steps = Mathf.Max(1, Mathf.CeilToInt(move.magnitude / Mathf.Max(0.001f, referenceSize)));
        Vector3 stepMove = move / steps;

        for (int i = 0; i < steps; i++)
            transform.position += stepMove;
    }

    private void RotateObject()
    {
        transform.Rotate(rotationAxis, currentRotationSpeed * Time.fixedDeltaTime, Space.Self);
    }

    public void ThrowObject()
    {
        if (currentVelocity.sqrMagnitude < 0.0001f)
            return;

        isThrown = true;
        isMovingFloating = false;
        throwVelocity = currentVelocity * throwSpeedCoefficient;

        StartCheckingCollisions();
    }

    private void ApplyThrowMotion()
    {
        transform.position += throwVelocity * Time.fixedDeltaTime;
        throwVelocity *= throwDecay;

        if (throwVelocity.magnitude < 0.001f)
        {
            isThrown = false;
            throwVelocity = Vector3.zero;
        }

        if (isRotatingFloating)
            RotateObject();
    }

    private void UpdateHandPush()
    {
        if (ignorePushThisFrame)
        {
            ignorePushThisFrame = false; // skip this frame only
            return;
        }

        if (grabInteractable != null)
        {
            if (grabInteractable.isSelected) return;
            //if (grabInteractable.isHovered) return;//
        }

        XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        bool anyPush = false;

        foreach (var interactor in interactors)
        {
            if (interactor == null || interactor.attachTransform == null)
                continue;

            Vector3 handPos = interactor.attachTransform.position;
            Vector3 handVelocity = Vector3.zero;

            if (lastHandPositions.TryGetValue(interactor, out Vector3 lastPos))
                handVelocity = (handPos - lastPos) / Time.fixedDeltaTime;

            lastHandPositions[interactor] = handPos;

            float distance = float.MaxValue;
            foreach (var col in childColliders)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(handPos);
                float d = Vector3.Distance(handPos, closest);
                if (d < distance) distance = d;
            }

            if (distance < pushRange && handVelocity.sqrMagnitude > 0.0001f)
            {
                float distanceFactor = 1f - Mathf.Clamp01(distance / pushRange);
                pushVelocity = handVelocity * pushForce * distanceFactor;

                if (pushVelocity.magnitude > maxPushSpeed)
                    pushVelocity = pushVelocity.normalized * maxPushSpeed;

                isPushed = true;
                anyPush = true;

                StartCheckingCollisions();
            }
        }

        if (!anyPush && pushVelocity.magnitude < 0.001f)
        {
            isPushed = false;
            pushVelocity = Vector3.zero;
        }
    }

    private void ApplyPushMotion()
    {
        transform.position += pushVelocity * Time.fixedDeltaTime;
        pushVelocity *= pushDecay;

        if (pushVelocity.magnitude < 0.001f)
        {
            isPushed = false;
            pushVelocity = Vector3.zero;
        }

        if (isRotatingFloating)
            RotateObject();
    }

    // ===== COLLISION DETECTION (TAG FILTER + DELAY) =====
    private void OnTriggerEnter(Collider other)
    {
        if (!isCheckingCollisions) return; // NEW
        if (!collisionActive) return;
        if (other.isTrigger) return;
        if (!other.CompareTag(stopTag)) return;
        if (other.transform.IsChildOf(transform)) return;
        StopMotion();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isCheckingCollisions) return; // NEW
        if (!collisionActive) return;
        if (other.isTrigger) return;
        if (!other.CompareTag(stopTag)) return;
        if (other.transform.IsChildOf(transform)) return;
        StopMotion();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isCheckingCollisions) return; // NEW
        if (!collisionActive) return;
        if (other.isTrigger) return;
        if (!other.CompareTag(stopTag)) return;
        isBlocked = false;
    }

    private void StopMotion()
    {
        // Instead of instant move, start backing
        if (currentVelocity.sqrMagnitude > 0.0001f)
        {
            backVelocity = -currentVelocity.normalized * backSpeed;
            isBacking = true;
        }

        // Stop all other motions
        isThrown = false;
        isPushed = false;
        throwVelocity = Vector3.zero;
        pushVelocity = Vector3.zero;
        isBlocked = true;
    }


    public void WhenSelectedStopMovingAlongPath() // call it in inspector in grab interactanle events, when selected
    {
        isMovingFloating = false;
        isRotatingFloating = false;
    }

    public void WhenUnselected() // call it in inspector in grab interactanle events, when exited
    {
        isRotatingFloating = true;

        // Clear hand positions to prevent accidental push
        lastHandPositions.Clear();

        // Prevent push from applying this frame
        ignorePushThisFrame = true;

        // Update lastPosition to make velocity calculation correct
        lastPosition = transform.position;

        // Optional: delay collision detection briefly to avoid StopMotion interference
        collisionActive = false;
        Invoke(nameof(EnableCollisionDetection), 0.01f);

        // Call ThrowObject immediately
        ThrowObject();
    }

    public void StartCheckingCollisions() 
    {
        if (!isCheckingCollisions)
        {
            isCheckingCollisions = true;
        }
        if (isMovingFloating)
        {
            isMovingFloating = false;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pushRange);
    }
#endif
}






