///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 15.06.2025
///   Last Change: 06.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using UnityEngine;
using System.Collections.Generic;

public class DebrisResetter : MonoBehaviour
{
    [SerializeField] private bool autoPopulateChildren = true;

    [SerializeField] private List<Transform> debrisChildren;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Transform initialParent;
    private Vector3 initialLocalScale;

    private Dictionary<Transform, Vector3> initialLocalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> initialLocalRotations = new Dictionary<Transform, Quaternion>();
    private Dictionary<Transform, Vector3> initialLocalScales = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Transform> initialParents = new Dictionary<Transform, Transform>();

    private void Awake()
    {
        if (autoPopulateChildren)
        {
            // GetComponentsInChildren(true) returns this + all children/grandchildren/etc.
            var allTransforms = GetComponentsInChildren<Transform>(true);
            debrisChildren = new List<Transform>();

            foreach (var t in allTransforms)
                if (t != this.transform)  // skip the parent itself
                    debrisChildren.Add(t);
        }
        initialParent = transform.parent;
    }

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialLocalScale = transform.localScale;

        foreach (var child in debrisChildren)
        {
            initialLocalPositions[child] = child.localPosition;
            initialLocalRotations[child] = child.localRotation;
            initialLocalScales[child] = child.localScale; //Store scale
            initialParents[child] = child.parent;
        }
    }

    private void OnDisable()
    {
        // Register self with the global reset manager
        DebrisResetManager.Instance.QueueReset(this);
    }

    // Called by the ResetManager when it's safe
    public void PerformReset()
    {
        transform.SetParent(initialParent, false);
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialLocalScale;

        foreach (var child in debrisChildren)
        {
            if (child == null) continue;

            child.SetParent(initialParents[child], false);
            child.localPosition = initialLocalPositions[child];
            child.localRotation = initialLocalRotations[child];
            child.localScale = initialLocalScales[child];

            // Reset movement if child has MoveOnTriggerToTarget
            MoveOnTriggerToTarget mover = child.GetComponent<MoveOnTriggerToTarget>();
            if (mover != null)
            {
                mover.ResetMovementState();
            }

            // Reset rotation if child has RandomZRotator
            RandomZRotator rotator = child.GetComponent<RandomZRotator>();
            if (rotator != null)
            {
                rotator.ResetRotationState();
            }
        }
    }
}
