///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   Created: 08.07.2025
///   Last Change: 08.07.2025

using UnityEngine;

public class DeactivateOnTrigger : MonoBehaviour
{
    [SerializeField] private string tagOfDeactivatorCollider = "DebrisDeactivatorCollider";
    private void OnTriggerEnter(Collider other)
    {
        // only deactivate if the trigger has a specific tag
        if (other.CompareTag(tagOfDeactivatorCollider))
        {
            // deactivate this GameObject
            gameObject.SetActive(false);
        }
    }
}