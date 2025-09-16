using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine.Playables;

public class ClearSpaceCatch : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] Transform grabPoint;                 // child on ClearSpace
    [SerializeField] Animator animator;                   // on ClearSpace

    [Header("Animator states")]
    [Tooltip("Layer qualified name, for example Base Layer.SolarOpen")]
    [SerializeField] string solarOpenStateName = "Base Layer.SolarOpen";
    [Tooltip("Layer qualified name, for example Base Layer.Grab (played early by seconds)")]
    [SerializeField] string grabStateName = "Base Layer.Grab";

    [Header("Timeline control")]
    [Tooltip("Timeline that must STOP the moment we grab")]
    [SerializeField] PlayableDirector directorToStopOnGrab;

    [Header("Chase")]
    [SerializeField] float maxMoveSpeed = 3f;
    [SerializeField] float stopDistance = 0.05f;          // grabPoint to target distance that counts as contact
    [SerializeField] float earlyBySeconds = 0.20f;        // start Grab this many seconds before contact

    [Header("Swoop feel")]
    [SerializeField] float slowRadius = 0.30f;            // start easing down within this distance
    [SerializeField] float minSpeedFactorAtContact = 0.50f; // about half speed at contact
    [SerializeField] float postGrabAccelTime = 0.35f;     // seconds to ramp back to full speed

    [Header("Grab parenting")]
    [Tooltip("If true, debris becomes child of grabPoint. Otherwise of ClearSpace root")]
    [SerializeField] bool parentDebrisToGrabPoint = true;
    [Tooltip("If true, snap debris exactly onto grabPoint on grab")]
    [SerializeField] bool snapChildToGrabPoint = false;

    [Header("Spline after grab")]
    [SerializeField] SplineContainer splineContainer;     // assign your path
    [SerializeField] int splineIndex = 0;
    [SerializeField] float driftToStartTolerance = 0.05f;
    [SerializeField] float driftToStartSpeedFactor = 0.6f;
    [SerializeField] int samplesPerUnit = 6;
    [SerializeField] float curveAdvanceTolerance = 0.03f;

    [Header("Finish cleanup")]
    [SerializeField] bool disableMeshRenderers = true;
    [SerializeField] bool disableSkinnedMeshRenderers = true;
    [SerializeField] bool disableColliders = true;
    [SerializeField] bool disableBehaviours = true;

    const float TurnSpeedDeg = 45f;

    Coroutine routine;
    bool parented;
    bool grabAnimStarted;
    bool solarOpenStarted, solarOpenHeld;
    float lastDist;

    // sampled world space points of the spline
    readonly List<Vector3> splinePoints = new List<Vector3>();
    int splinePointIndex;
    bool driftingToStart;

    void Awake()
    {
        if (animator != null) animator.applyRootMotion = false;
    }

    public void MoveTo(Transform debris)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RunSequence(debris));
    }

    IEnumerator RunSequence(Transform debris)
    {
        if (!animator || !grabPoint || !debris) yield break;

        // play SolarOpen at start and hold on last frame until Grab begins
        StartSolarOpen();
        solarOpenHeld = false;

        parented = false;
        grabAnimStarted = false;
        splinePoints.Clear();
        splinePointIndex = 0;
        driftingToStart = false;

        float postGrabTimer = 0f;
        lastDist = Vector3.Distance(grabPoint.position, debris.position);

        while (true)
        {
            // hold SolarOpen on last frame until Grab starts
            if (solarOpenStarted && !solarOpenHeld && !grabAnimStarted)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                if (st.IsName(solarOpenStateName) && st.normalizedTime >= 0.99f)
                {
                    animator.speed = 0f; // freeze pose
                    solarOpenHeld = true;
                }
            }

            if (!parented)
            {
                // where must ClearSpace be so grabPoint sits on debris
                Vector3 targetPos = debris.position;
                Vector3 desiredClearSpacePos = transform.position + (targetPos - grabPoint.position);

                // easing near contact
                float dist = Vector3.Distance(grabPoint.position, targetPos);
                float t = Mathf.Clamp01((dist - stopDistance) / Mathf.Max(0.0001f, slowRadius));
                float speedFactor = Mathf.Lerp(minSpeedFactorAtContact, 1f, Mathf.SmoothStep(0f, 1f, t));
                float speed = maxMoveSpeed * speedFactor;

                transform.position = Vector3.MoveTowards(transform.position, desiredClearSpacePos, speed * Time.deltaTime);

                // aim up axis toward debris
                Vector3 toTarget = targetPos - transform.position;
                if (toTarget.sqrMagnitude > 1e-6f)
                {
                    Quaternion desiredUp = Quaternion.FromToRotation(transform.up, toTarget.normalized) * transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredUp, TurnSpeedDeg * Time.deltaTime);
                }

                // start Grab early by seconds
                if (!grabAnimStarted && earlyBySeconds > 0f)
                {
                    float closing = (lastDist - dist) / Mathf.Max(Time.deltaTime, 1e-5f);
                    if (closing > 0f)
                    {
                        float ttc = Mathf.Max(0f, (dist - stopDistance) / closing);
                        if (ttc <= earlyBySeconds) StartGrab();
                    }
                }

                // contact
                if (dist <= stopDistance)
                {
                    // stop the other timeline right now
                    if (directorToStopOnGrab) directorToStopOnGrab.Stop();

                    // parent debris under ClearSpace or under grabPoint
                    Transform newParent = (parentDebrisToGrabPoint && grabPoint) ? grabPoint : transform;

                    if (snapChildToGrabPoint)
                    {
                        debris.SetParent(newParent, worldPositionStays: false);
                        debris.localPosition = Vector3.zero;
                        debris.localRotation = Quaternion.identity;
                        debris.localScale = Vector3.one;
                    }
                    else
                    {
                        debris.SetParent(newParent, worldPositionStays: true);
                    }

                    parented = true;
                    postGrabTimer = 0f;

                    if (!grabAnimStarted) StartGrab();

                    // prepare spline follow
                    BuildSplineSamples();
                    driftingToStart = true;
                }

                lastDist = dist;
            }
            else
            {
                if (splinePoints.Count == 0) break;

                Transform root = transform;

                if (driftingToStart)
                {
                    Vector3 startPoint = splinePoints[0];
                    float speed = maxMoveSpeed * Mathf.Clamp01(driftToStartSpeedFactor);

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
                        splinePointIndex = 1;
                        postGrabTimer = 0f;
                    }
                }
                else
                {
                    if (splinePointIndex >= splinePoints.Count) break;

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

        CleanupAssembly(transform);
        routine = null;
    }

    // animation helpers

    void StartSolarOpen()
    {
        if (!animator || string.IsNullOrEmpty(solarOpenStateName)) return;
        animator.speed = 1f;
        int hash = Animator.StringToHash(solarOpenStateName);
        animator.CrossFadeInFixedTime(hash, 0f, 0, 0f);
        solarOpenStarted = true;
    }

    void StartGrab()
    {
        if (!animator || string.IsNullOrEmpty(grabStateName)) return;
        animator.speed = 1f; // resume if we had SolarOpen held
        int hash = Animator.StringToHash(grabStateName);
        animator.CrossFadeInFixedTime(hash, 0f, 0, 0f);
        grabAnimStarted = true;
    }

    // cleanup

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

    // spline helpers

    void BuildSplineSamples()
    {
        splinePoints.Clear();
        if (splineContainer == null) return;

        var spline = splineContainer[splineIndex];
        float length = spline.GetLength();
        int count = Mathf.Max(2, Mathf.CeilToInt(length * Mathf.Max(1, samplesPerUnit)));

        for (int i = 0; i < count; i++)
        {
            float t = (count <= 1) ? 0f : (float)i / (count - 1);
            float3 wpos = splineContainer.EvaluatePosition(splineIndex, t);
            splinePoints.Add((Vector3)wpos);
        }
    }
}
