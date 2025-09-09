///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   GPT was used 
///   Created: 05.07.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

/// Launches the four corner cylinders attached to net along their local Y-axis with a one-time impulse,
/// and allows resetting them to initial positions. Cylinders remain stationary until launch.
/// Should be attached to container GameObject.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetLauncher : MonoBehaviour
{
    public static NetLauncher Instance;

    [Header("–– Corner Cylinders ––")]
    [Tooltip("Assign the Rigidbody of each corner cylinder here.")]
    public Rigidbody[] cornerCylinders = new Rigidbody[4];
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    [Header("Launch Settings")]
    [Tooltip("Impulse magnitude (units/sec) along each cylinder's local Y-axis.")]
    public float launchImpulse = 15f;
    public bool hasLaunched = false;
    public KeyCode resetKey = KeyCode.R;

    [Header("Cylinders physics")]
    [Tooltip("Linear drag applied to cylinders when they collide with cloth.")]
    [SerializeField] private float linearDamp = 0;
    [SerializeField] private float angularDamp = 1f;

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject netCloth;
    private MeshRenderer netClothMeshRend;

    void Awake()
    {
        Instance = this;

        // Capture initial transforms and configure rigidbodies before any physics steps
        int count = cornerCylinders.Length;
        initialPositions = new Vector3[count];
        initialRotations = new Quaternion[count];

        for (int i = 0; i < count; i++)
        {
            Rigidbody rb = cornerCylinders[i];
            if (rb == null) continue;

            // Record initial state
            initialPositions[i] = rb.transform.position;
            initialRotations[i] = rb.transform.rotation;

            // Ensure stationary until launch
            rb.useGravity = false;
            rb.linearDamping = linearDamp;
            rb.angularDamping = angularDamp;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.detectCollisions = true;
        }
        AutoAssignObjects();
    }

    private void Start()
    {
        netClothMeshRend = netCloth.GetComponent<MeshRenderer>();
    }

    // Gives each cylinder a one-time impulse along its local Y-axis.
    // Un-kinematizes them first so physics applies.
    public void LaunchNet()
    {
        if (this.isActiveAndEnabled)
        {
            if (hasLaunched) return;
            hasLaunched = true;
            netClothMeshRend.enabled = true;

            foreach (Rigidbody rb in cornerCylinders)
            {
                if (rb == null) continue;

                // Enable physics motion
                rb.isKinematic = false;

                // One-time impulse
                Vector3 impulseDir = rb.transform.up.normalized;
                rb.AddForce(impulseDir * launchImpulse, ForceMode.VelocityChange);
            }
        }
        else
        {
            Debug.Log("Net is not active, you cannot launch it now.");
        }
        
    }

    private void AutoAssignObjects()
    {
        // Auto-assign NetCloth
        if (netCloth == null)
        {
            netCloth = SceneUtils.FindDeep("NetCloth");
            if (netCloth == null)
                Debug.LogError("Couldn't find and assign 'NetCloth'.");
        }
    }

    private void OnDisable()
    {
        hasLaunched = false;
        netClothMeshRend.enabled = false;
    }
}


/*
public float collisionDrag = 6f;

// Call this when cloth collision detects a corner is stopped: applies drag to halt further motion.
    // cornerIndex: 0-3 mapping to cornerCylinders array.
    public void OnCornerStopped(int cornerIndex)
    {
        if (cornerIndex < 0 || cornerIndex >= cornerCylinders.Length) return;
        Rigidbody rb = cornerCylinders[cornerIndex];
        if (rb == null) return;

        rb.linearDamping = collisionDrag;
    }
 
 */

