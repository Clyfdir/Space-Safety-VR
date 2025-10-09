///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 09.10.2025
///   Created: 09.10.2025

///   simulation of microgravity

using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MicrogravityObject : MonoBehaviour
{
    [Header("Initial Motion (Inspector)")]
    [SerializeField] private Vector3 initialDirection = Vector3.forward;
    [SerializeField] private float initialSpeed = 0.05f;

    [Header("Initial Rotation")]
    [SerializeField, Tooltip("Maximum initial random spin (radians per second)")]
    private float initialAngularSpeed = 0.2f;

    [Header("Microgravity Damping")]
    [SerializeField, Tooltip("1 = no decay; 0.999 = very slow decay")]
    private float linearDecay = 0.999f;
    [SerializeField, Tooltip("Rotation decay")]
    private float angularDecay = 0.999f;

    [Header("Collision response")]
    [SerializeField, Tooltip("Energy kept after bouncing off static colliders (0..1)")]
    private float bounceLossFactor = 0.8f;
    [SerializeField, Tooltip("Multiply relative velocity when hitting moving colliders")]
    private float bounceGainFactor = 1.2f;

    [Header("Spin response")]
    [SerializeField, Tooltip("Maximum spin added after each collision")]
    private float collisionSpinImpulse = 0.3f;

    [Header("Limits")]
    [SerializeField] private float maxLinearSpeed = 0.2f;
    [SerializeField] private float maxAngularSpeed = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private Vector3 linearVelocity;
    private Vector3 angularVelocity;
    private Collider col;
    private Rigidbody attachedRb; // optional (we keep it kinematic)

    private void Awake()
    {
        col = GetComponent<Collider>();
        if (TryGetComponent(out attachedRb))
        {
            attachedRb.isKinematic = true;
            attachedRb.useGravity = false;
        }
    }

    private void Start()
    {
        // Initial drift
        linearVelocity = (initialDirection.sqrMagnitude > 0f)
            ? initialDirection.normalized * initialSpeed
            : Vector3.zero;

        // Initial slow random spin
        angularVelocity = new Vector3(
            UnityEngine.Random.Range(-initialAngularSpeed, initialAngularSpeed),
            UnityEngine.Random.Range(-initialAngularSpeed, initialAngularSpeed),
            UnityEngine.Random.Range(-initialAngularSpeed, initialAngularSpeed)
        );
    }

    private void FixedUpdate()
    {
        // decay velocities (microgravity drag)
        linearVelocity *= linearDecay;
        angularVelocity *= angularDecay;

        // clamp
        if (linearVelocity.magnitude > maxLinearSpeed)
            linearVelocity = linearVelocity.normalized * maxLinearSpeed;

        if (angularVelocity.magnitude > maxAngularSpeed)
            angularVelocity = angularVelocity.normalized * maxAngularSpeed;

        // desired movement
        Vector3 move = linearVelocity * Time.fixedDeltaTime;

        // small-step movement to avoid tunneling
        float referenceSize = col.bounds.extents.magnitude * 0.5f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(move.magnitude / Mathf.Max(0.001f, referenceSize)));
        Vector3 stepMove = move / steps;

        for (int i = 0; i < steps; i++)
        {
            Vector3 halfExtents = col.bounds.extents * 0.5f;
            if (halfExtents.sqrMagnitude < 1e-6f)
                halfExtents = Vector3.one * 0.01f; // fallback tiny box

            RaycastHit[] hits = Physics.BoxCastAll(transform.position, halfExtents, stepMove.normalized,
                                                    transform.rotation, stepMove.magnitude,
                                                    ~0, QueryTriggerInteraction.Ignore);

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit? chosenHit = null;

            foreach (var h in hits)
            {
                if (h.collider == null || h.collider.isTrigger) continue;
                if (h.collider == col) continue;
                Rigidbody otherRb = h.rigidbody;
                if (otherRb != null && attachedRb != null && otherRb == attachedRb) continue;
                if (h.distance <= 1e-4f) continue;

                chosenHit = h;
                break;
            }

            if (chosenHit.HasValue)
            {
                var hit = chosenHit.Value;
                Vector3 normal = hit.normal;
                Rigidbody otherRb = hit.rigidbody;

                // detect other velocity
                Vector3 otherVel = Vector3.zero;
                if (otherRb != null && !otherRb.isKinematic)
                    otherVel = otherRb.linearVelocity;
                else
                {
                    var otherCtrl = hit.collider.GetComponent<MicrogravityObject>();
                    if (otherCtrl != null)
                        otherVel = otherCtrl.GetLinearVelocity();
                }

                bool otherIsMoving = otherVel.sqrMagnitude > 1e-6f;

                if (!otherIsMoving)
                {
                    // Static: bounce and lose energy
                    linearVelocity = Vector3.Reflect(linearVelocity, normal) * bounceLossFactor;
                }
                else
                {
                    // Moving object: always apply velocity along normal
                    Vector3 otherVelAlongNormal = Vector3.Project(otherVel, normal);

                    // Ensure stationary objects get pushed
                    if (linearVelocity.sqrMagnitude < 1e-6f)
                    {
                        linearVelocity = otherVelAlongNormal;
                    }
                    else
                    {
                        // Already moving: reflect along normal and add velocity
                        Vector3 reflected = Vector3.Reflect(linearVelocity, normal);
                        linearVelocity = reflected + otherVelAlongNormal;
                    }

                    // Apply gain factor
                    linearVelocity *= bounceGainFactor;
                }

                // add a bit of random spin on impact
                angularVelocity += new Vector3(
                    UnityEngine.Random.Range(-collisionSpinImpulse, collisionSpinImpulse),
                    UnityEngine.Random.Range(-collisionSpinImpulse, collisionSpinImpulse),
                    UnityEngine.Random.Range(-collisionSpinImpulse, collisionSpinImpulse)
                );

                // clamp velocities
                linearVelocity = Vector3.ClampMagnitude(linearVelocity, maxLinearSpeed);
                angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxAngularSpeed);

                // move up to contact point
                float travel = Mathf.Max(0f, hit.distance - 0.001f);
                transform.position += stepMove.normalized * travel;
                transform.position += normal * 0.001f;

                if (debug)
                    Debug.Log($"[MicrogravityController] Hit {hit.collider.name}, vel={linearVelocity}, spin={angularVelocity}");

                break; // stop this step
            }
            else
            {
                transform.position += stepMove;
            }
        }

        // Apply spin (rotation)
        transform.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.Self);
    }

    public Vector3 GetLinearVelocity() => linearVelocity;
}

