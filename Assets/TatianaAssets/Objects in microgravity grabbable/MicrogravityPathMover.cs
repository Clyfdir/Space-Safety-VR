///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 18.10.2025
///   Created: 18.10.2025

///   microgravity movement in predefined paths

using System;
using System.Collections.Generic;
using UnityEngine;

public class MicrogravityPathMover : MonoBehaviour
{
    [Header("Path Settings")]
    private Transform initialPosition;
    [SerializeField] private Transform[] targetPositions;
    [SerializeField] private float moveSpeed = 0.05f;
    [SerializeField] private float arriveThreshold = 0.01f;

    [Header("Control")]
    [Tooltip("If true, object moves along its path.")]
    public bool isMovingFloating = true;
    [Tooltip("If true, object rotates while floating.")]
    public bool isRotatingFloating = true;

    [Header("Smooth Step Settings")]
    [SerializeField, Tooltip("Smaller = smoother movement (like ISS float).")]
    private float stepResolutionFactor = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField, Tooltip("Base rotation speed (degrees per second).")]
    private float rotationSpeed = 10f;
    [SerializeField, Tooltip("Randomize spin direction and speed slightly.")]
    private bool randomizeRotation = true;
    [SerializeField, Tooltip("How strong the random spin offset is.")]
    private float randomRotationVariance = 0.5f;

    [Header("Throw Settings")]
    [SerializeField, Tooltip("How much slower a throw is compared to current speed.")]
    private float throwSpeedCoefficient = 0.5f;
    [SerializeField, Tooltip("How quickly thrown motion decays (1 = no decay, 0.95 = slows gradually).")]
    private float throwDecay = 0.98f;

    private List<Collider> childColliders = new();
    private int currentTargetIndex = 0;
    private float referenceSize = 0.1f;

    // Rotation state
    private Vector3 rotationAxis = Vector3.up;
    private float currentRotationSpeed;

    // --- NEW: Velocity tracking ---
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    // --- NEW: Throw state ---
    private bool isThrown = false;
    private Vector3 throwVelocity;

    private void Awake()
    {
        // Collect all child colliders (ignore parent’s collider)
        childColliders.AddRange(GetComponentsInChildren<Collider>());
        if (TryGetComponent(out Collider parentCol))
            childColliders.Remove(parentCol);

        // Compute combined bounds of all child colliders
        if (childColliders.Count > 0)
        {
            Bounds combined = childColliders[0].bounds;
            for (int i = 1; i < childColliders.Count; i++)
                combined.Encapsulate(childColliders[i].bounds);
            referenceSize = combined.extents.magnitude * stepResolutionFactor;
        }

        // Initialize rotation parameters
        rotationAxis = UnityEngine.Random.onUnitSphere;
        currentRotationSpeed = rotationSpeed;
        if (randomizeRotation)
            currentRotationSpeed *= UnityEngine.Random.Range(1f - randomRotationVariance, 1f + randomRotationVariance);
    }

    private void Start()
    {
        // initial position
        initialPosition.position = transform.position;

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // --- NEW: Calculate velocity each frame ---
        currentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        // --- NEW: If thrown, override normal movement ---
        if (isThrown)
        {
            ApplyThrowMotion();
            return;
        }

        if (isMovingFloating)
        {
            MoveAlongPath();
        }

        if (isRotatingFloating)
        {
            RotateObject();
        }
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
            // Move to next target
            currentTargetIndex++;
            if (currentTargetIndex >= targetPositions.Length)
                currentTargetIndex = 0;

            // Randomize rotation slightly at each waypoint (for natural feel)
            if (randomizeRotation)
            {
                rotationAxis = UnityEngine.Random.onUnitSphere;
                currentRotationSpeed = rotationSpeed * UnityEngine.Random.Range(1f - randomRotationVariance, 1f + randomRotationVariance);
            }

            return;
        }

        // Smooth motion toward target
        Vector3 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
        int steps = Mathf.Max(1, Mathf.CeilToInt(move.magnitude / Mathf.Max(0.001f, referenceSize)));
        Vector3 stepMove = move / steps;

        for (int i = 0; i < steps; i++)
        {
            transform.position += stepMove;
        }
    }

    private void RotateObject()
    {
        transform.Rotate(rotationAxis, currentRotationSpeed * Time.fixedDeltaTime, Space.Self);
    }

    public void PauseMoving()
    {
        isMovingFloating = false;
    }

    public void ReviseMoving()
    {
        RotateObject();
        ThrowObject();
    }

    // --- NEW: Throw logic ---
    public void ThrowObject()
    {
        if (currentVelocity.sqrMagnitude < 0.0001f)
            return; // no movement, nothing to throw

        isThrown = true;
        isMovingFloating = false; // pause path motion
        throwVelocity = currentVelocity * throwSpeedCoefficient;
    }

    private void ApplyThrowMotion()
    {
        transform.position += throwVelocity * Time.fixedDeltaTime;

        // Gradual slowdown
        throwVelocity *= throwDecay;

        if (throwVelocity.magnitude < 0.001f)
        {
            isThrown = false;
            throwVelocity = Vector3.zero;
        }

        // Allow rotation while drifting
        if (isRotatingFloating)
            RotateObject();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (targetPositions == null || targetPositions.Length == 0)
            return;

        Gizmos.color = Color.cyan;
        Vector3 prev = initialPosition != null ? initialPosition.position : transform.position;

        foreach (var t in targetPositions)
        {
            if (t == null) continue;
            Gizmos.DrawLine(prev, t.position);
            Gizmos.DrawSphere(t.position, 0.02f);
            prev = t.position;
        }

        // Loop back for visualization
        if (initialPosition != null && targetPositions.Length > 0 && targetPositions[^1] != null)
            Gizmos.DrawLine(targetPositions[^1].position, targetPositions[0].position);
    }
#endif
}




