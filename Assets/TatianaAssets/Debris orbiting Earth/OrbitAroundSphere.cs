///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   Created: 08.07.2025
///   Last Change: 08.07.2025

using UnityEngine;

[RequireComponent(typeof(Transform))]
public class OrbitAroundSphere : MonoBehaviour
{
    [Tooltip("The sphere (or any Transform) to orbit around; will auto-find a GameObject named 'EarthSize' if left empty")]
    public Transform sphereCenter;

    [Tooltip("Extra distance above the sphere’s surface")]
    private float offset;

    [Tooltip("Degrees per second to orbit; random between min and max")]
    public Vector2 speedRange = new Vector2(20f, 60f);

    public float orbitingAngle = 5;

    private float _orbitRadius;
    public float angularSpeedDebris;      // degrees per second
    private Vector3 _orbitAxis;      // random axis for orbit
    public int directionDebrisOrbiting = 1;

    void Awake()
    {
        offset = Random.Range(2, 5);

        // Auto-assign EarthSize if no center set
        if (sphereCenter == null)
        {
            var go = GameObject.Find("EarthSize");
            if (go != null)
                sphereCenter = go.transform;
            else
            {
                Debug.LogError("OrbitAroundSphere: couldn't find 'EarthSize'. Disabling.");
                enabled = false;
                return;
            }
        }

        // Determine sphere base radius
        float baseRadius = 0f;
        var sph = sphereCenter.GetComponent<SphereCollider>();
        if (sph != null)
        {
            float maxScale = Mathf.Max(
                sphereCenter.lossyScale.x,
                sphereCenter.lossyScale.y,
                sphereCenter.lossyScale.z
            );
            baseRadius = sph.radius * maxScale;
        }
        else
        {
            // fallback: approximate using renderer bounds
            var rend = sphereCenter.GetComponent<Renderer>();
            if (rend != null)
                baseRadius = rend.bounds.extents.magnitude;
        }
        _orbitRadius = baseRadius + offset;
    }

    void OnEnable()
    {


        // Pick a random starting position on the sphere at given radius
        //Vector3 startDir = Random.onUnitSphere;
        //transform.position = sphereCenter.position + startDir * _orbitRadius;

        // Randomize orbit axis (any arbitrary tilt)
        //_orbitAxis = Random.onUnitSphere;//!!! random direction on 360 degree range

        // pick a random axis within ±45° of Vector3.up
        float maxTilt = orbitingAngle * Mathf.Deg2Rad;           // convert to radians
        float cosMax = Mathf.Cos(maxTilt);

        // keep sampling until we're within the cone
        Vector3 axis;
        do
        {
            axis = Random.onUnitSphere;
        } while (Vector3.Dot(axis, Vector3.up) < cosMax);

        _orbitAxis = axis;

        // pick positive speed 
        float speed = Random.Range(speedRange.x, speedRange.y);
        // then flip it if direction is -1:
        angularSpeedDebris = speed * directionDebrisOrbiting;
    }

    void Update()
    {
        // Rotate around center by speed * deltaTime
        transform.RotateAround(
            sphereCenter.position,
            _orbitAxis,
            angularSpeedDebris * Time.deltaTime
        );
    }
}


