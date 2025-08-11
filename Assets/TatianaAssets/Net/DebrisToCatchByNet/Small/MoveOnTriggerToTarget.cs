///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 11.06.2025
///   Last Change: 16.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using UnityEngine;

public class MoveOnTriggerToTarget : MonoBehaviour
{
    [SerializeField] private string triggerTag;     // Tag of the trigger collider
    [SerializeField] private float moveSpeed;                // Units per second
    [SerializeField] private Transform targetTransform;           // Target to move toward

    [SerializeField] private bool shouldMove = false;
    [SerializeField] private bool soundPlayed = false;
    [SerializeField] private string smallDebrisHitTriggerCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            shouldMove = true;
        }
        if (other.CompareTag(smallDebrisHitTriggerCollider))
        {
            Debug.Log("Small debris hit spaceship, play sound.");
            soundPlayed = true;
        }
    }

    private void Update()
    {
        if (shouldMove && targetTransform != null)
        {
            // Move toward target position
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);

            // Optionally stop moving when you reach the target
            if (Vector3.Distance(transform.position, targetTransform.position) < 0.01f)
            {
                shouldMove = false;
            }
        }
    }

    public void ResetMovementState()
    {
        shouldMove = false;
        soundPlayed = false;
    }
}

