using UnityEngine;

[ExecuteAlways]
public class RotateSkybox : MonoBehaviour
{
    [Tooltip("Degrees per second around the world up axis")]
    public float degreesPerSecond = 5f;

    [Tooltip("Camera to follow so the sky has no parallax")]
    public Camera targetCamera;

    void Reset()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // keep the sky mesh centered on the camera
        transform.position = targetCamera.transform.position;

        // rotate around world up
        transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
    }
}
