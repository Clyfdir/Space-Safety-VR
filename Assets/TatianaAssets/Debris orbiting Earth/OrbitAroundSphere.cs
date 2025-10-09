///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   AI was used: GPT
///   Created: 08.07.2025
///   Last Change: 08.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 09.10.2025

using UnityEngine;

[RequireComponent(typeof(Transform))]
public class OrbitAroundSphere : MonoBehaviour
{
    public Transform sphereCenter;
    private Vector2 speedRange = new Vector2(4f, 9f);
    [SerializeField] private float speedDeg;
    private int directionDebrisOrbiting = 1;

    // --- cached per-instance ---
    private Vector3 _C;             // center
    private Vector3 _U, _V;         // orbit plane basis (unit)
    private float _r;               // radius
    private float _theta;           // current angle (radians)
    private float _omegaRad;        // angular speed (radians/sec)

    // If your planet never moves:
    private bool centerIsStatic = true;

    void Awake()
    {
        if (sphereCenter == null)
        {
            var go = GameObject.Find("EarthSize");
            if (go != null) sphereCenter = go.transform;
            else { enabled = false; Debug.LogError("OrbitAroundSphere: EarthSize not found."); }
        }
    }

    void OnEnable()
    {
        // 1) Gather geometry
        Vector3 C = sphereCenter.position;
        Vector3 P = transform.position;
        Vector3 R = P - C;
        if (R.sqrMagnitude < 1e-6f) { R = Vector3.right; P = C + R; transform.position = P; }

        // 2) Desired tangent = local Y projected to tangent plane at P
        Vector3 T = transform.up - Vector3.Project(transform.up, R);
        if (T.sqrMagnitude < 1e-8f)
        {
            T = Vector3.Cross(R, Vector3.right);
            if (T.sqrMagnitude < 1e-8f) T = Vector3.Cross(R, Vector3.forward);
        }
        T.Normalize();

        // 3) Orbit axis N so that (N x R) points along T
        Vector3 N = Vector3.Cross(R, T).normalized;

        // 4) Build plane basis: U along R, V = N x U
        _r = R.magnitude;
        _U = R / _r;             // unit from center to spawn point
        _V = Vector3.Cross(N, _U); // guaranteed unit & perp

        // 5) Cache center & angular speed
        _C = C;
        speedDeg = Random.Range(speedRange.x, speedRange.y);
        float sign = Mathf.Sign(directionDebrisOrbiting);
        _omegaRad = speedDeg * Mathf.Deg2Rad * sign;

        // 6) Start angle so current position = C + r * U
        _theta = 0f;
    }

    void Update()
    {
        // If the planet moves, refresh _C
        if (!centerIsStatic) _C = sphereCenter.position;

        _theta += _omegaRad * Time.deltaTime;
        float c = Mathf.Cos(_theta);
        float s = Mathf.Sin(_theta);

        // Single write to Transform.position (cheap)
        transform.position = _C + _r * (c * _U + s * _V);
    }
}



