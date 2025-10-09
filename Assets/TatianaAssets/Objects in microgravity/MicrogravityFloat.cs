///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 06.10.2025
///   Created: 06.10.2025

///   simulation of microgravity

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MicrogravityFloat : MonoBehaviour
{
    private float maxLinearSpeed = 0.05f;   // 5 cm/s
    private float maxAngularSpeed = 0.3f;   // rad/s
    private float linearDecay = 0.999f;
    private float angularDecay = 0.999f;
    private float randomNudge = 0.001f;

    private Vector3 linearVelocity;
    private Vector3 angularVelocity;
    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();

        linearVelocity = new Vector3(
            Random.Range(-0.02f, 0.02f),
            Random.Range(-0.02f, 0.02f),
            Random.Range(-0.02f, 0.02f)
        );

        angularVelocity = new Vector3(
            Random.Range(-0.02f, 0.02f),
            Random.Range(-0.02f, 0.02f),
            Random.Range(-0.02f, 0.02f)
        );
    }

    private void FixedUpdate()
    {
        linearVelocity *= linearDecay;
        angularVelocity *= angularDecay;

        if (linearVelocity.magnitude > maxLinearSpeed)
            linearVelocity = linearVelocity.normalized * maxLinearSpeed;

        if (angularVelocity.magnitude > maxAngularSpeed)
            angularVelocity = angularVelocity.normalized * maxAngularSpeed;

        // Tiny random nudges
        linearVelocity += new Vector3(
            Random.Range(-randomNudge, randomNudge),
            Random.Range(-randomNudge, randomNudge),
            Random.Range(-randomNudge, randomNudge)
        );

        // Movement step
        Vector3 move = linearVelocity * Time.fixedDeltaTime;

        // Split movement into smaller steps to prevent tunneling
        int steps = Mathf.CeilToInt(move.magnitude / (col.bounds.extents.magnitude * 0.5f));
        steps = Mathf.Max(steps, 1);
        Vector3 stepMove = move / steps;

        for (int i = 0; i < steps; i++)
        {
            if (Physics.BoxCast(transform.position, col.bounds.extents * 0.5f, stepMove.normalized, out RaycastHit hit, transform.rotation, stepMove.magnitude))
            {
                transform.position += stepMove.normalized * (hit.distance - 0.001f);
                linearVelocity = Vector3.Reflect(linearVelocity, hit.normal) * 0.9f;

                angularVelocity += new Vector3(
                    Random.Range(-0.01f, 0.01f),
                    Random.Range(-0.01f, 0.01f),
                    Random.Range(-0.01f, 0.01f)
                );

                // Stop further movement in this step to prevent sticking
                break;
            }
            else
            {
                transform.position += stepMove;
            }
        }

        // Apply rotation
        transform.Rotate(angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.Self);
    }
}
