///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 18.10.2025
///   Created: 19.10.2025

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

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        childColliders.AddRange(GetComponentsInChildren<Collider>());
        if (TryGetComponent(out Collider parentCol))
            childColliders.Remove(parentCol);

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
    }

    private void FixedUpdate()
    {
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
            currentTargetIndex++;
            if (currentTargetIndex >= targetPositions.Length)
                currentTargetIndex = 0;

            if (randomizeRotation)
            {
                rotationAxis = UnityEngine.Random.onUnitSphere;
                currentRotationSpeed = rotationSpeed * UnityEngine.Random.Range(1f - randomRotationVariance, 1f + randomRotationVariance);
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

    public void PauseMoving() => isMovingFloating = false;
    public void ReviseMoving() { RotateObject(); ThrowObject(); }

    public void ThrowObject()
    {
        if (currentVelocity.sqrMagnitude < 0.0001f)
            return;

        isThrown = true;
        isMovingFloating = false;
        throwVelocity = currentVelocity * throwSpeedCoefficient;
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
        if (grabInteractable != null)
        {
            if (grabInteractable.isSelected) return;
            if (grabInteractable.isHovered) return;
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
            {
                handVelocity = (handPos - lastPos) / Time.fixedDeltaTime;
            }
            lastHandPositions[interactor] = handPos;

            // --- NEW COLLIDER-BASED DISTANCE CALCULATION ---
            float distance = float.MaxValue;
            foreach (var col in childColliders)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(handPos);
                float d = Vector3.Distance(handPos, closest);
                if (d < distance)
                    distance = d;
            }

            // --- Push logic using collider distance ---
            if (distance < pushRange && handVelocity.sqrMagnitude > 0.0001f)
            {
                float distanceFactor = 1f - Mathf.Clamp01(distance / pushRange);
                pushVelocity = handVelocity * pushForce * distanceFactor;

                // Clamp maximum push speed
                if (pushVelocity.magnitude > maxPushSpeed)
                    pushVelocity = pushVelocity.normalized * maxPushSpeed;

                isPushed = true;
                anyPush = true;
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (targetPositions != null && targetPositions.Length > 0)
        {
            Gizmos.color = Color.cyan;
            Vector3 prev = initialPosition != null ? initialPosition.position : transform.position;

            foreach (var t in targetPositions)
            {
                if (t == null) continue;
                Gizmos.DrawLine(prev, t.position);
                Gizmos.DrawSphere(t.position, 0.02f);
                prev = t.position;
            }

            if (initialPosition != null && targetPositions.Length > 0 && targetPositions[^1] != null)
                Gizmos.DrawLine(targetPositions[^1].position, targetPositions[0].position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pushRange);
    }
#endif
}



