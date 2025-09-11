///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025
///   Created: 08.09.2025

/// Testing the demo debris sequence.
/// This script closes shutter of window.

using System.Collections;
using UnityEngine;

public class RotateShutterZToZero : MonoBehaviour
{
    [SerializeField] private float duration = 7f;               // seconds
    [SerializeField] private bool playOnEnable = true;          // auto-start on enable
    [SerializeField] private bool useLocalRotation = true;      // local vs world
    [SerializeField]
    private AnimationCurve easing =            // optional easing
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine routine;
    private float zOnDisable = -160f;

    private void OnEnable()
    {
        if (playOnEnable) StartRotation();
    }

    /// Call this from a button or another script to (re)start the rotation
    public void StartRotation()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RotateCo());
    }

    private IEnumerator RotateCo()
    {
        Vector3 startEuler = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        float startZ = startEuler.z;
        float targetZ = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eased = easing.Evaluate(k);

            float z = Mathf.LerpAngle(startZ, targetZ, eased);  // handles 0/360 wrap
            Vector3 e = startEuler;
            e.z = z;

            if (useLocalRotation) transform.localEulerAngles = e;
            else transform.eulerAngles = e;

            yield return null;
        }

        /*
        // snap exactly to 0 at the end
        Vector3 finalEuler = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        finalEuler.z = targetZ;
        if (useLocalRotation) transform.localEulerAngles = finalEuler;
        else transform.eulerAngles = finalEuler;
         */

        routine = null;
    }

    private void OnDisable()
    {
        // stop any in-flight tween
        if (routine != null) { StopCoroutine(routine); routine = null; }

        // snap back to desired Z while preserving X/Y
        Vector3 e = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        e.z = zOnDisable; // Unity may display as 200°, but it equals -160°
        if (useLocalRotation) transform.localEulerAngles = e;
        else transform.eulerAngles = e;
    }
}
