using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;   // Splines package

public class ClearSpaceCatch : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] Transform grabPoint;                // child on this follower
    [SerializeField] Animator animator;                  // animator on this follower
    [SerializeField] string arriveTrigger = "OnCatch";   // used if no state name is provided
    [Tooltip("Layer qualified state name, for example Base Layer.OnCatch")]
    [SerializeField] string arriveStateName = "Base Layer.OnCatch";

    [Header("Chase")]
    [SerializeField] float maxMoveSpeed = 3f;            // world units per second cap
    [SerializeField] float stopDistance = 0.05f;         // grabPoint to target distance that counts as contact
    [SerializeField] float earlyBySeconds = 0.20f;       // start animation this many seconds before contact

    [Header("Swoop feel")]
    [SerializeField] float slowRadius = 0.30f;           // start easing down within this distance
    [SerializeField] float minSpeedFactorAtContact = 0.50f; // fraction of max speed at contact, set to 0.5 for half speed
    [SerializeField] float postGrabAccelTime = 0.35f;    // seconds to ramp back to full speed

    [Header("Spline after grab")]
    [SerializeField] SplineContainer splineContainer;    // assign your path here
    [SerializeField] int splineIndex = 0;                // which spline inside the container
    [SerializeField] float driftToStartTolerance = 0.05f;
    [SerializeField] float driftToStartSpeedFactor = 0.6f;   // relative speed while drifting to the first point
    [SerializeField] int samplesPerUnit = 6;                 // sampling density for curve
    [SerializeField] float curveAdvanceTolerance = 0.03f;    // step tolerance while marching samples

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

    // sampled world space points of the spline
    readonly List<Vector3> splinePoints = new List<Vector3>();
    int splinePointIndex;
    bool driftingToStart;

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
        splinePoints.Clear();
        splinePointIndex = 0;
        driftingToStart = false;

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

                // fixed step, no stall at goal
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
                    transform.SetParent(grabbable, true);
                    parented = true;
                    postGrabTimer = 0f;

                    if (!animTriggered) TriggerArriveAnimation();

                    // build and begin spline follow
                    BuildSplineSamples();
                    driftingToStart = true;
                }

                lastDist = dist;
            }
            else
            {
                // parented and now following the spline
                if (splinePoints.Count == 0) break;

                Transform root = transform.parent != null ? transform.parent : transform;

                if (driftingToStart)
                {
                    Vector3 startPoint = splinePoints[0];
                    float speed = maxMoveSpeed * Mathf.Clamp01(driftToStartSpeedFactor);

                    // drift toward the first sample of the spline
                    root.position = Vector3.MoveTowards(root.position, startPoint, speed * Time.deltaTime);

                    Vector3 travel = startPoint - root.position;
                    if (travel.sqrMagnitude > 1e-6f)
                    {
                        Quaternion desiredUp = Quaternion.FromToRotation(transform.up, travel.normalized) * transform.rotation;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredUp, TurnSpeedDeg * Time.deltaTime);
                    }

                    if (Vector3.Distance(root.position, startPoint) <= driftToStartTolerance)
                    {
                        driftingToStart = false;
                        splinePointIndex = 1; // start advancing along the curve
                        postGrabTimer = 0f;   // begin accel back to full speed
                    }
                }
                else
                {
                    if (splinePointIndex >= splinePoints.Count) break;

                    // accelerate from contact fraction back to full speed
                    postGrabTimer += Time.deltaTime;
                    float accel = Mathf.Clamp01(postGrabTimer / Mathf.Max(0.0001f, postGrabAccelTime));
                    float speed = Mathf.Lerp(maxMoveSpeed * minSpeedFactorAtContact, maxMoveSpeed, accel);

                    Vector3 target = splinePoints[splinePointIndex];
                    root.position = Vector3.MoveTowards(root.position, target, speed * Time.deltaTime);

                    Vector3 travel = target - root.position;
                    if (travel.sqrMagnitude > 1e-6f)
                    {
                        Quaternion desiredUp = Quaternion.FromToRotation(transform.up, travel.normalized) * transform.rotation;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredUp, TurnSpeedDeg * Time.deltaTime);
                    }

                    if (Vector3.Distance(root.position, target) <= curveAdvanceTolerance)
                        splinePointIndex++;
                }
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

        if (!string.IsNullOrEmpty(arriveStateName))
        {
            // immediate start
            int hash = Animator.StringToHash(arriveStateName);
            animator.CrossFadeInFixedTime(hash, 0f, 0, 0f);
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

    // ---------- Spline helpers ----------

    void BuildSplineSamples()
    {
        splinePoints.Clear();

        if (splineContainer == null)
            return;

        // get the spline and its length
        var spline = splineContainer[splineIndex];
        float length = spline.GetLength(); // total curve length in world units
        int count = Mathf.Max(2, Mathf.CeilToInt(length * Mathf.Max(1, samplesPerUnit)));

        // sample world positions from t 0 to 1
        for (int i = 0; i < count; i++)
        {
            float t = (count <= 1) ? 0f : (float)i / (count - 1);
            float3 wpos = splineContainer.EvaluatePosition(splineIndex, t);
            splinePoints.Add((Vector3)wpos);
        }
    }
}
