using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClearSpaceCatch : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] Transform grabPoint;                // child on this follower
    [SerializeField] Animator animator;                  // animator on this follower
    [SerializeField] string arriveTrigger = "OnCatch";   // used if no state name is provided
    [Tooltip("Layer qualified state name, for example Base Layer.OnCatch")]
    [SerializeField] string arriveStateName = "Base Layer.OnCatch";

    [Header("Chase")]
    [SerializeField] float smoothTime = 0.25f;          // shapes the feel a little
    [SerializeField] float maxMoveSpeed = 3f;           // world units per second cap
    [SerializeField] float stopDistance = 0.05f;        // grabPoint to target distance that counts as contact
    [SerializeField] float earlyBySeconds = 0.20f;      // start animation this many seconds before contact

    [Header("Swoop feel")]
    [SerializeField] float slowRadius = 0.30f;          // start easing down within this distance
    [SerializeField] float minSpeedFactorAtContact = 0.50f; // fraction of max speed at contact, set to 0.5 for half speed
    [SerializeField] float postGrabAccelTime = 0.35f;   // seconds to ramp back to full speed

    [Header("Waypoints after grab")]
    [Tooltip("Assign one to three empties. Assembly moves to each in order.")]
    [SerializeField] List<Transform> waypoints = new List<Transform>();
    [SerializeField] float waypointArriveDistance = 0.05f;

    [Header("Finish cleanup")]
    [SerializeField] bool disableMeshRenderers = true;
    [SerializeField] bool disableSkinnedMeshRenderers = true;
    [SerializeField] bool disableColliders = true;
    [SerializeField] bool disableBehaviours = true;

    const float TurnSpeedDeg = 45f; // constant turn speed

    Coroutine routine;
    bool animTriggered;
    bool parented;
    int arriveStateHash;
    float lastDist;

    void Awake()
    {
        if (animator != null) animator.applyRootMotion = false;
        arriveStateHash = !string.IsNullOrEmpty(arriveStateName)
            ? Animator.StringToHash(arriveStateName)
            : 0;
    }

    public void MoveTo(Transform grabbable)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RunSequence(grabbable));
    }

    IEnumerator RunSequence(Transform grabbable)
    {
        if (grabPoint == null || grabbable == null) yield break;

        animTriggered = false;
        parented = false;

        int wpIndex = 0;
        float postGrabTimer = 0f;

        lastDist = Vector3.Distance(grabPoint.position, grabbable.position);

        while (true)
        {
            if (!parented)
            {
                // follower world position that places grabPoint on the grabbable
                Vector3 targetPos = grabbable.position;
                Vector3 desiredFollowerPos = transform.position + (targetPos - grabPoint.position);

                // distance for easing and timing
                float dist = Vector3.Distance(grabPoint.position, targetPos);

                // ease down near contact to a chosen fraction of max speed
                float t = Mathf.Clamp01((dist - stopDistance) / Mathf.Max(0.0001f, slowRadius));
                float speedFactor = Mathf.Lerp(minSpeedFactorAtContact, 1f, Mathf.SmoothStep(0f, 1f, t));
                float speed = maxMoveSpeed * speedFactor;

                // MoveTowards gives a fixed step and never stalls at the goal
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    desiredFollowerPos,
                    speed * Time.deltaTime
                );

                // rotate so this object up axis points at the target
                Vector3 toTarget = targetPos - transform.position;
                if (toTarget.sqrMagnitude > 1e-6f)
                {
                    Quaternion desiredUp = Quaternion.FromToRotation(transform.up, toTarget.normalized) * transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredUp, TurnSpeedDeg * Time.deltaTime);
                }

                // early trigger using measured closing speed of the grab distance
                if (!animTriggered && earlyBySeconds > 0f)
                {
                    float closing = (lastDist - dist) / Mathf.Max(Time.deltaTime, 1e-5f); // positive when closing
                    if (closing > 0f)
                    {
                        float ttc = Mathf.Max(0f, (dist - stopDistance) / closing);
                        if (ttc <= earlyBySeconds) TriggerArriveAnimation();
                    }
                }

                // contact, parent immediately and continue
                if (dist <= stopDistance)
                {
                    transform.SetParent(grabbable, true); // keep world pose
                    parented = true;
                    postGrabTimer = 0f;

                    if (!animTriggered) TriggerArriveAnimation();
                }

                lastDist = dist;
            }
            else
            {
                // move the entire assembly through waypoints at the same base speed
                if (waypoints == null || waypoints.Count == 0 || wpIndex >= waypoints.Count)
                    break;

                Transform root = transform.parent != null ? transform.parent : transform;
                Transform wp = waypoints[wpIndex];
                if (wp == null) { wpIndex++; continue; }

                // ramp from the contact speed back to full speed
                postGrabTimer += Time.deltaTime;
                float accel = Mathf.Clamp01(postGrabTimer / Mathf.Max(0.0001f, postGrabAccelTime));
                float speed = Mathf.Lerp(maxMoveSpeed * minSpeedFactorAtContact, maxMoveSpeed, accel);

                Vector3 p = root.position;
                Vector3 target = wp.position;
                root.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);

                // optional facing toward travel direction with the same up axis rule
                Vector3 travel = target - root.position;
                if (travel.sqrMagnitude > 1e-6f)
                {
                    Quaternion desiredUp = Quaternion.FromToRotation(transform.up, travel.normalized) * transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredUp, TurnSpeedDeg * Time.deltaTime);
                }

                if (Vector3.Distance(root.position, target) <= waypointArriveDistance)
                    wpIndex++;
            }

            yield return null;
        }

        // finished path
        CleanupAssembly(transform.parent != null ? transform.parent : transform);
        routine = null;
    }

    void TriggerArriveAnimation()
    {
        if (animator == null) return;

        // instant start, avoids transition timing issues
        if (arriveStateHash != 0)
        {
            animator.CrossFadeInFixedTime(arriveStateHash, 0f, 0, 0f);
        }
        else if (!string.IsNullOrEmpty(arriveTrigger))
        {
            animator.ResetTrigger(arriveTrigger);
            animator.SetTrigger(arriveTrigger);
        }

        animTriggered = true;
    }

    void CleanupAssembly(Transform root)
    {
        if (disableMeshRenderers)
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;

        if (disableSkinnedMeshRenderers)
            foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) r.enabled = false;

        if (disableColliders)
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) c.enabled = false;

        if (disableBehaviours)
            foreach (var b in root.GetComponentsInChildren<MonoBehaviour>(true)) b.enabled = false;
    }
}
