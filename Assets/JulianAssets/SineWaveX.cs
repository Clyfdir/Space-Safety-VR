using UnityEngine;

public class SineWaveX : MonoBehaviour
{
    [SerializeField] private float center;
    [SerializeField] private float amplitude;
    [SerializeField] private float speed = 1f;

    void Update()
    {
        transform.localPosition = new Vector3(
            center + amplitude * Mathf.Sin(Time.time * speed),
            transform.localPosition.y,
            transform.localPosition.z
        );
    }
}
