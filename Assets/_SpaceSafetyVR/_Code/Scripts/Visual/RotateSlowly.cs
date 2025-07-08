using UnityEngine;

public class RotateSlowly : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.01f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    void Start()
    {
        
    }

    
    void Update()
    {
        // Rotate the object around the specified axis at the specified speed
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}
