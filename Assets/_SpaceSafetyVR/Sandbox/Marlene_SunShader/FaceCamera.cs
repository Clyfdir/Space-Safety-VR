using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera mainCam;

    void Start()
    {
        //mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null) return;

        if (mainCam == null) return;

        // Get direction from this object to the camera
        Vector3 directionToCamera = mainCam.transform.position - transform.position;
        
        // Base rotation to look at the camera
        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);

        // Add 90 degrees around the Z-axis (relative to current forward)
        Quaternion extraZRotation = Quaternion.Euler(90, 0, 0);

        // Apply combined rotation
        transform.rotation = lookRotation * extraZRotation;
    }
}