///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 10.09.2025
///   Created: 10.09.2025

/// Script in order to parent a sphere to Main Camera; sphere is to make everything dark gradually

using UnityEngine;

public class ReparentOnEnable : MonoBehaviour
{
    [Header("New Parent Settings")]
    private Vector3 newLocalPosition = Vector3.zero; // Target local position
    private Vector3 initialLocalPosition = Vector3.zero;

    private Transform newParent;
    private Transform initialParent;

    private void Awake()
    {
        // Save initial parent and local position
        initialParent = transform.parent;

        // Find the MainCamera object in the scene
        GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObj != null)
        {
            newParent = cameraObj.transform;
        }
        else
        {
            Debug.LogWarning("No object with tag 'MainCamera' found in scene!");
        }
    }

    private void OnEnable()
    {
        if (newParent != null)
        {
            transform.SetParent(newParent, false);
            transform.localPosition = newLocalPosition;
        }
    }

    /*
        private void OnDisable()
    {
        // Restore original parent and local position
        transform.SetParent(initialParent, false);
        transform.localPosition = initialLocalPosition;
    }
    */

}

