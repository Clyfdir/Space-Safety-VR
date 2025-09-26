using UnityEngine;

[ExecuteAlways]
public class OrbitController : MonoBehaviour
{
    [Header("Achse und Zeit")]
    public Transform orbitAxisRef;                 // up ist die orbit achse
    public bool useUnscaledTime = false;           // für timeline und editor
    public bool runInLateUpdate = true;            // nach kamerabewegung

    [Header("Raten in grad pro sekunde")]
    public float wStars = 0.04f;                   // 360 durch 9000
    public float wEarthOrbit = 0.04f;              // erde um station
    public float wEarthSpin = 0.004178079f;       // 360 durch 86164
    public float wSunOrbit = 0.039988584f;       // 0.04 minus jahresdrift

    [Header("Erde und Sonne")]
    public Transform earthParent;                  // pivot am stationsort
    public Transform earthBody;                    // eigentlicher erdkörper
    public Transform sunPivot;                     // pivot am stationsort
    public Transform sunBody;                      // sonnen objekt für position
    public Light sunLight;                         // directional light der sonne

    [Header("Skybox material kopie")]
    [Tooltip("leer lassen um die aktuelle RenderSettings.skybox zu klonen")]
    public Material sourceSkybox;
    public Vector3 skyBaseSunDir = Vector3.up;     // basisrichtung im cubemap authoring
    public bool syncSkyToSun = true;               // shader erwartet das für yaw

    // runtime zustand
    Material skyboxInstance;
    Material previousSkybox;
    float yawDeg;

    // shader property ids
    static readonly int ID_SyncToSun = Shader.PropertyToID("_SkySyncToSun");
    static readonly int ID_SkyYawDeg = Shader.PropertyToID("_SkyYawDeg");
    static readonly int ID_UseMainLight = Shader.PropertyToID("_UseMainLight");
    static readonly int ID_SunDir = Shader.PropertyToID("_SunDirection");
    static readonly int ID_BaseSunDir = Shader.PropertyToID("_SkyBaseSunDir");

    void OnEnable()
    {
        // skybox klonen und aktivieren
        var src = sourceSkybox != null ? sourceSkybox : RenderSettings.skybox;
        if (src != null)
        {
            skyboxInstance = new Material(src) { name = src.name + " (Runtime Instance)" };
            previousSkybox = RenderSettings.skybox;
            RenderSettings.skybox = skyboxInstance;

            skyboxInstance.SetFloat(ID_SyncToSun, syncSkyToSun ? 1f : 0f);
            skyboxInstance.SetVector(ID_BaseSunDir, new Vector4(skyBaseSunDir.x, skyBaseSunDir.y, skyBaseSunDir.z, 0f));
            yawDeg = skyboxInstance.HasProperty(ID_SkyYawDeg) ? skyboxInstance.GetFloat(ID_SkyYawDeg) : 0f;
        }
    }

    void OnDisable()
    {
        if (previousSkybox != null)
            RenderSettings.skybox = previousSkybox;

        if (skyboxInstance != null)
        {
            if (Application.isPlaying) Destroy(skyboxInstance);
            else DestroyImmediate(skyboxInstance);
        }
        skyboxInstance = null;
        previousSkybox = null;
    }

    void Update()
    {
        if (!runInLateUpdate) Tick(TimeStep());
    }

    void LateUpdate()
    {
        if (runInLateUpdate) Tick(TimeStep());
    }

    float TimeStep()
    {
        if (Application.isPlaying)
            return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        else
            return 1f / 60f;
    }

    void Tick(float dt)
    {
        Vector3 axis = orbitAxisRef != null ? orbitAxisRef.up.normalized : Vector3.up;

        // erde um station
        if (earthParent != null)
            earthParent.Rotate(axis, wEarthOrbit * dt, Space.World);

        // erde um eigene achse
        if (earthBody != null)
            earthBody.Rotate(earthBody.up, wEarthSpin * dt, Space.Self);

        // sonne um station
        if (sunPivot != null)
            sunPivot.Rotate(axis, wSunOrbit * dt, Space.World);

        // licht richtet sich von sonnenort zu erdort
        if (sunLight != null && sunBody != null && earthBody != null)
        {
            Vector3 dirSunToEarth = (earthBody.position - sunBody.position).normalized;   // von Sonne zur Erde
            Vector3 dirEarthToSun = -dirSunToEarth;
            sunLight.transform.rotation = Quaternion.LookRotation(dirSunToEarth, axis);
            // shader sicher füttern damit sonne und himmel übereinstimmen
            if (skyboxInstance != null)
            {
                skyboxInstance.SetFloat(ID_UseMainLight, 0f);
                skyboxInstance.SetVector(ID_SunDir, new Vector4(dirEarthToSun.x, dirEarthToSun.y, dirEarthToSun.z, 0f)); 
                skyboxInstance.SetFloat(ID_SyncToSun, syncSkyToSun ? 1f : 0f);
            }
        }

        // sterne über _SkyYawDeg drehen
        if (skyboxInstance != null && skyboxInstance.HasProperty(ID_SkyYawDeg))
        {
            yawDeg = Mathf.Repeat(yawDeg + wStars * dt, 360f);
            skyboxInstance.SetFloat(ID_SkyYawDeg, yawDeg);
        }
    }
}
