///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT
///   Created: 12.07.2025
///   Last Change: 12.07.2025

using System.Collections.Generic;
using UnityEngine;


/// Assigns a random material from the provided list on Awake.
/// Attach to a GameObject with a Renderer and populate the list in the Inspector.

public class RandomMaterialAssigner : MonoBehaviour
{
    [Tooltip("List of possible materials to choose from.")]
    [SerializeField] private List<Material> materials = new List<Material>();

    private Renderer objectRenderer;

    private void Awake()
    {
        // Cache the Renderer
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning("RandomMaterialAssigner: No Renderer found on this GameObject.");
            return;
        }

        if (materials == null || materials.Count == 0)
        {
            Debug.LogWarning("RandomMaterialAssigner: No materials assigned.");
            return;
        }

        // Pick a random index and assign the material
        int index = Random.Range(0, materials.Count);
        objectRenderer.material = materials[index];
    }
}
