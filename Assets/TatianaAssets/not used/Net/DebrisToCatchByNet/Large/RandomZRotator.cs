///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 10.06.2025
///   Last Change: 06.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using UnityEngine;

public class RandomZRotator : MonoBehaviour
{
    private float rotationSpeed;
    [SerializeField] private bool shouldRotate = true;

    [Header("Rotation Speed Range")]
    public float minSpeed = -90f;  // counter-clockwise
    public float maxSpeed = 90f;   // clockwise

    void Start()
    {
        // Assign a random speed within the range
        rotationSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        } 
    }

    public void ResetRotationState()
    {
        shouldRotate = true;
    }

    public void DisableRotation()
    {
        shouldRotate = false;
    }
}
