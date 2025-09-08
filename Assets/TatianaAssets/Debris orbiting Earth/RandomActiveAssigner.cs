///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 04.09.2025
///   Created: 04.09.2025

/// Helps orbiting debris to look more varied, but keeping using the same prefab
/// Randomly sets objects in the list active or inactive once on Awake.
/// Each object has a 50% chance of being active. 

using System.Collections.Generic;
using UnityEngine;


public class RandomActiveAssigner : MonoBehaviour
{
    [Tooltip("List of GameObjects to randomly activate/deactivate.")]
    [SerializeField] private List<GameObject> objectsToToggle = new List<GameObject>();

    private void Awake()
    {
        if (objectsToToggle == null || objectsToToggle.Count == 0)
        {
            Debug.LogWarning("RandomActiveAssigner: No objects assigned.");
            return;
        }

        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
            {
                bool activeState = Random.value > 0.5f; // 50/50 chance
                obj.SetActive(activeState);
            }
        }
    }
}
