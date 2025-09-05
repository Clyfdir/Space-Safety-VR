///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   script used (with my modifications):https://www.youtube.com/watch?v=8nENcDnxeVE ; https://github.com/affaxltd/rope-tutorial ;
///   Created: 05.07.2025
///   Last Change: 11.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

/// Draws a line - rope.
/// Rope is animated, using #Spring
/// This GO must have Line Renderer Component.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private Spring spring;
    private LineRenderer lr;
    private Vector3 currentPosition;

    [Header("Rope properties")]
    [SerializeField] private int quality = 500;
    [SerializeField] private float damper = 1.5f;
    [SerializeField] private float strength = 50;
    [SerializeField] private float velocity = 15;
    [SerializeField] private float waveCount = 3;
    [SerializeField] private float ropeWaveHeight = 1;
    [SerializeField] private float straightenDuration = 2;
    public AnimationCurve affectCurve;// both sides are 0, near left side is bumped up

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject ropeStartPos;
    [SerializeField] private GameObject ropeEndPos;
    [SerializeField] private NetLauncher netLauncher;



    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);

        AutoAssignObjects();
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        // If the net haven't launched yet, bail out
        if (!netLauncher.hasLaunched)
        {
            currentPosition = ropeStartPos.transform.position;
            spring.Reset();
            if (lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }

        // First time drawing the rope: initialize the spring and points
        if (lr.positionCount == 0)
        {
            // give it an initial "throw� velocity
            spring.SetVelocity(velocity);
            // tell it that its target value is 1 (so the wave builds up)
            spring.SetTarget(1f);
            // allocate enough points on the line
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var endPoint = ropeEndPos.transform.position;
        var startPosition = ropeStartPos.transform.position;
        var up = Quaternion.LookRotation((endPoint - startPosition).normalized) * Vector3.up;
        var right = Quaternion.LookRotation((endPoint - startPosition).normalized) * Vector3.right;
        currentPosition = Vector3.Lerp(currentPosition, endPoint, Time.deltaTime * 12f);

        //for combined up and right offsets:
        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = (right + up) * ropeWaveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta);
            lr.SetPosition(i, Vector3.Lerp(startPosition, currentPosition, delta) + offset);
        }
    }

    public void StraightenRope()
    {
        StartCoroutine(_StraightenCoroutine());
    }

    private IEnumerator _StraightenCoroutine()
    {
        float startHeight = ropeWaveHeight;
        float elapsed = 0f;

        // If duration is zero or negative, just snap to zero immediately:
        if (straightenDuration <= 0f)
        {
            ropeWaveHeight = 0f;
            yield break;
        }

        while (elapsed < straightenDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / straightenDuration;                      // 0 1 over time
            ropeWaveHeight = Mathf.Lerp(startHeight, 0f, t);   // interpolate
            yield return null;                                 // wait for next frame
        }

        // Ensure exact zero at the end
        ropeWaveHeight = 0f;
    }

    private void AutoAssignObjects()
    {
        // Auto-assign ropeStartPos
        if (ropeStartPos == null)
        {
            ropeStartPos = SceneUtils.FindDeep("ropeStartPosNetContainerModel");
            if (ropeStartPos == null)
                Debug.LogError("Couldn't find and assign 'ropeStartPosNetContainerModel'.");
        }
        // Auto-assign ropeTarget
        if (ropeEndPos == null)
        {
            GameObject go = SceneUtils.FindDeep("ropeEndPos");
            ropeEndPos = go;
            if (ropeEndPos == null)
                Debug.LogError("Couldn't find and assign 'ropeEndPos'.");
        }

        // Auto-assign netLauncher
        if (netLauncher == null)
        {
            GameObject go = SceneUtils.FindDeep("NetLauncher");
            netLauncher = go.GetComponent<NetLauncher>();
            if (netLauncher == null)
                Debug.LogError("Couldn't find and assign 'NetLauncher'.");
        }
    }

    private void OnDisable()
    {
        ropeWaveHeight = 1;
    }
}

/*
        // for up offset

        currentPosition = Vector3.Lerp(currentPosition, endPoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(startPosition, currentPosition, delta) + offset);
        }
         */