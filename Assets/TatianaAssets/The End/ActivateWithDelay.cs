///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 10.09.2025
///   Created: 10.09.2025

/// Script for testing

using UnityEngine;
using System.Collections;

public class ActivateWithDelay : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // The object to activate
    [SerializeField] private float delay = 6f;        // Delay in seconds

    private void OnEnable()
    {
        if (targetObject != null)
        {
            StartCoroutine(ActivateAfterDelay());
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        targetObject.SetActive(true);
    }
}
