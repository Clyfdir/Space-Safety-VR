///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   GPT was used 
///   Created: 05.07.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class CornerFoldMover : MonoBehaviour
{
    [Tooltip("Name of the trigger collider that causes folding")]
    public string foldTriggerName = "FoldTriggerCollider";

    [Tooltip("Destination to move toward when triggered")]
    public Transform foldingPoint;

    [Tooltip("How long the move should take (sec)")]
    public float moveDuration = 1.2f;

    [SerializeField] private float stopDistance = 0.05f;//NEW

    private Rigidbody rb;
    public bool hasFolded = false;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;                // start kinematic
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasFolded && other.name == foldTriggerName)
        {
            if (CompareTag("NetParentStopper"))
            {
                rb.isKinematic = true;
            }
            else
            {
                hasFolded = true;
                // lock in start and end
                startPos = rb.position;
                targetPos = foldingPoint.position;
                StartCoroutine(FoldCoroutine());
            } 
        }
    }

    private IEnumerator FoldCoroutine()
    {
        float elapsed = 0f;

        // Make sure physics is OFF so we control position exactly
        rb.isKinematic = true;

        // NEW Compute the point to stop at
        float fullDistance = Vector3.Distance(startPos, targetPos);
        float travelRatio = fullDistance > stopDistance
            ? (fullDistance - stopDistance) / fullDistance
            : 0f;
        Vector3 adjustedTarget = Vector3.Lerp(startPos, targetPos, travelRatio);

        while (elapsed < moveDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float s = Mathf.SmoothStep(0f, 1f, t);

            // Lerp from the locked-in start to the locked-in target
            //Vector3 newPos = Vector3.Lerp(startPos, targetPos, s);
            Vector3 newPos = Vector3.Lerp(startPos, adjustedTarget, s);
            rb.MovePosition(newPos);

            yield return new WaitForFixedUpdate();
        }

        // Final snap
        //rb.MovePosition(targetPos);
        rb.MovePosition(adjustedTarget);

        // Leave it kinematic so it never moves again
        rb.isKinematic = true;
    }

    private void OnDisable()
    {
        hasFolded = false;
    }
}
