///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   AI was used: GPT
///   Created: 06.07.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

///   Moving debris which should be caught by the net


using System.Collections.Generic;
using UnityEngine;

public class MoveWithSpeedOnStartZ : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 9f;      // units per second
    private string colliderToDecreaseSpeed = "ColliderToTriggerUserWarning";


    private bool debrisStoppedMoving = false;
    private bool startMovingDebris = false;
    private bool speedDecreased = false;

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject debrisTarget;

    void Awake()
    {
        AutoAssignObjects();
    }
    void OnEnable()
    {
        /*
        PWEventsManager.Instance?.SecondWarningPlayed.AddListener(DebrisCanMove);

        // Если ивент мог быть вызван до подписки
        if (PWEventsManager.Instance != null && PWEventsManager.Instance.secondWarningWasPlayed)
        {
            DebrisCanMove();
        }
        */

    }
    private void OnTriggerEnter(Collider other)
    {

        // When uses is warned to press button
        if (other.CompareTag(colliderToDecreaseSpeed) && !speedDecreased && other.transform.root != other.transform)
        {
            Debug.Log($"Speed decreased.");
            moveSpeed = 5f;
            speedDecreased = true;
        }
    }
    private void Start()
    {
        //PWEventsManager.Instance?.SecondWarningPlayed.AddListener(DebrisCanMove);
    }


    void Update()
    {
        //if(startMovingDebris) MoveDebris();
        MoveDebris();//for faster testing
    }

    private void MoveDebris()
    {
        if (debrisTarget == null) return;

        // 1) update target each frame:
        Vector3 targetPos = debrisTarget.transform.position;

        // CONSTANT SPEED (no easing)
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // distance to go
        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist < 0.001f && !debrisStoppedMoving)
        {
            debrisStoppedMoving = true;

            Debug.Log("Debris stopped moving");

            // Find EVERY RandomZRotator on this GameObjects children
            RandomZRotator[] rotators = GetComponentsInChildren<RandomZRotator>();

            // Disable each one
            foreach (RandomZRotator rz in rotators)
            {
                rz.DisableRotation();
            }
        }
    }

    private void OnDisable()
    {
        debrisStoppedMoving = false;
        speedDecreased = false;
        moveSpeed = 9f;
    }

    private void DebrisCanMove()
    {
        startMovingDebris = true;
    }

    private void AutoAssignObjects()
    {
        // Auto-assign debrisTarget
        if (debrisTarget == null)
        {
            debrisTarget = SceneUtils.FindDeep("debrisTarget");
            if (debrisTarget == null)
                Debug.LogError("Couldn't find and assign 'targetObject'.");
        }
    }
}
