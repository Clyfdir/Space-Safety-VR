///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025
///   Created: 08.09.2025

/// Temporal script just to test different ends of the experience, how they look like.
/// This script toggles the frame of the monitor which informs about the end of experience.

using System.Collections;
using UnityEngine;

public class ToggleActiveInterval : MonoBehaviour
{
    [SerializeField] private GameObject target;     // if null, uses this.gameObject
    [SerializeField] private float onSeconds = 1f;  // time kept active
    [SerializeField] private float offSeconds = 1f; // time kept inactive
    [SerializeField] private bool startOnEnable = true;
    [SerializeField] private bool startActive = true;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private int maxToggles = 0;    // 0 = loop forever; >0 = number of state changes

    private Coroutine routine;

    private void OnEnable()
    {
        if (startOnEnable) StartToggling();
    }

    private void OnDisable()
    {
        StopToggling();
    }

    public void StartToggling()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ToggleCo());
    }

    public void StopToggling()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    private IEnumerator ToggleCo()
    {
        var go = target ? target : gameObject;
        go.SetActive(startActive);

        int togglesDone = 0;

        while (maxToggles <= 0 || togglesDone < maxToggles)
        {
            // wait based on current state
            float wait = go.activeSelf ? onSeconds : offSeconds;

            if (useUnscaledTime)
            {
                float end = Time.unscaledTime + wait;
                while (Time.unscaledTime < end) yield return null;
            }
            else
            {
                if (wait > 0f) yield return new WaitForSeconds(wait);
                else yield return null; // immediate next frame
            }

            // toggle
            go.SetActive(!go.activeSelf);
            togglesDone++;
        }

        routine = null;
    }
}
