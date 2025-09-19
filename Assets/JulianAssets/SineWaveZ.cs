using UnityEngine;

public class SineWaveZ : MonoBehaviour
{
    [SerializeField] private float center;
    [SerializeField] private float amplitude;
    [SerializeField] private float speed = 1f;

    void Update()
    {
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            center + amplitude * Mathf.Sin(Time.time * speed)
        );
    }
}
