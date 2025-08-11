///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 10.06.2025
///   Last Change: 10.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    private Vector3 rotationAxis;
    private float rotationSpeed;

    void Start()
    {
        // Randomly choose an axis (X, Y, or Z)
        int axis = Random.Range(0, 3);
        switch (axis)
        {
            case 0:
                rotationAxis = Vector3.right;
                break;
            case 1:
                rotationAxis = Vector3.up;
                break;
            case 2:
                rotationAxis = Vector3.forward;
                break;
        }

        // Random speed between 30 and 180 degrees per second
        rotationSpeed = Random.Range(30f, 180f);
    }

    void Update()
    {
        // Rotate around the chosen axis
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
