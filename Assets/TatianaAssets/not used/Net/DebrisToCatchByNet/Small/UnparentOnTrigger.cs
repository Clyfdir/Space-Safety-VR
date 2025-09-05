///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 16.06.2025
///   Last Change: 16.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using UnityEngine;

public class UnparentOnTrigger : MonoBehaviour
{
    public string targetTag; // Set the tag in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            transform.parent = null; // Unparent this GameObject
        }
    }
}
