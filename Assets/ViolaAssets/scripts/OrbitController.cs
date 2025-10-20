using UnityEngine;

[ExecuteAlways]
public class OrbitController : MonoBehaviour
{
    [Header("Axis and timing")]
    public Transform orbitAxisRef;          // up defines the orbit axis
    public bool useUnscaledTime = false;    // for Timeline or editor preview
    public bool runInLateUpdate = true;     // after camera moves

    [Header("Speed control")]
    [Tooltip("Integer multiplier for all angular speeds. Use negative to reverse.")]
    public int speedMultiplier = 1;

    [Header("Earth and Sun")]
    public Transform earthParent;           // stays at station  no orbit rotation applied
    public Transform earthBody;             // actual Earth mesh  spins only
    public Transform sunPivot;              // Sun position relative to station
    public Transform sunBody;               // optional sphere for the Sun position
    public Light sunLight;                  // Directional Light representing sunlight

    [Header("Skybox material instance")]
    [Tooltip("Leave empty to clone the current RenderSettings.skybox")]
    public Material sourceSkybox;
    public Vector3 skyBaseSunDir = Vector3.up; // only used if your custom shader supports it
    public bool syncSkyToSun = true;            // only used if your custom shader supports it

    [Header("Optional")]
    public bool setRenderSettingsSun = true;    // set Lighting Sun Source
    public bool updateGI = false;               // update reflection probes when rotating built in skybox
    public float giUpdateEveryDegrees = 15f;
    public Transform viewRef;                   // camera or observer for shader sun dir  defaults to this.transform

    // runtime state
    Material skyboxInstance;
    Material previousSkybox;
    float yawDeg;
    float lastGIYaw;

    // property IDs
    static readonly int ID_SyncToSun = Shader.PropertyToID("_SkySyncToSun");
    static readonly int ID_SkyYawDeg = Shader.PropertyToID("_SkyYawDeg");
    static readonly int ID_UseMainLight = Shader.PropertyToID("_UseMainLight");
    static readonly int ID_SunDir = Shader.PropertyToID("_SunDirection");
    static readonly int ID_BaseSunDir = Shader.PropertyToID("_SkyBaseSunDir");
    static readonly int ID_Rotation = Shader.PropertyToID("_Rotation"); // built in Skybox/Cubemap

    bool hasSkyYaw, hasRotation, hasSyncToSunProp, hasUseMainLightProp, hasSunDirProp, hasBaseSunDirProp;

    // hard coded scientific rates in deg per second
    // stars  360 / 9000 = +0.04
    const float W_STARS = 360f / 9000f;

    // sun apparent around the station  -360/year + 360/9000 - small parallax term
    static readonly float W_SUN = (-360f / 31536000f) + (360f / 9000f) - ((6.52f * Mathf.Cos(45f * Mathf.Deg2Rad)) / 150_000_000f);
    // equals about +0.0399886 deg per second

    // earth relative spin seen from the station  360/86164 - 360/9000  which is westward
    static readonly float W_EARTH_REL = (360f / 86164f) - (360f / 9000f);
    // equals about -0.035821921 deg per second

    // directional light  earth shadow drift w.r.t. stars  360/86164
    const float W_DL = 360f / 86164f; // ~ +0.004178079 deg per second  included for reference

    void OnEnable()
    {
        // skybox clone and activation
        var src = sourceSkybox != null ? sourceSkybox : RenderSettings.skybox;
        if (src != null)
        {
            skyboxInstance = new Material(src) { name = src.name + " (Runtime Instance)" };
            previousSkybox = RenderSettings.skybox;
            RenderSettings.skybox = skyboxInstance;

            hasSkyYaw = skyboxInstance.HasProperty(ID_SkyYawDeg);
            hasRotation = skyboxInstance.HasProperty(ID_Rotation);
            hasSyncToSunProp = skyboxInstance.HasProperty(ID_SyncToSun);
            hasUseMainLightProp = skyboxInstance.HasProperty(ID_UseMainLight);
            hasSunDirProp = skyboxInstance.HasProperty(ID_SunDir);
            hasBaseSunDirProp = skyboxInstance.HasProperty(ID_BaseSunDir);

            if (hasSyncToSunProp) skyboxInstance.SetFloat(ID_SyncToSun, syncSkyToSun ? 1f : 0f);
            if (hasBaseSunDirProp)
                skyboxInstance.SetVector(ID_BaseSunDir, new Vector4(skyBaseSunDir.x, skyBaseSunDir.y, skyBaseSunDir.z, 0f));

            yawDeg = hasSkyYaw ? skyboxInstance.GetFloat(ID_SkyYawDeg)
                               : hasRotation ? skyboxInstance.GetFloat(ID_Rotation)
                                             : 0f;
            lastGIYaw = yawDeg;
        }

        if (setRenderSettingsSun && sunLight != null)
            RenderSettings.sun = sunLight;
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
        // signed multiplier  use negative to reverse all directions if desired
        float m = speedMultiplier;

        Vector3 axis = orbitAxisRef != null ? orbitAxisRef.up.normalized : Vector3.up;

        // Earth no longer orbits the station  position stays fixed
        // Only rotate Earth texture seen from station with the net rate W_EARTH_REL
        if (earthBody != null)
            earthBody.Rotate(earthBody.up, (W_EARTH_REL * m) * dt, Space.Self);

        // Sun orbits around the station at the computed apparent rate
        if (sunPivot != null)
            sunPivot.Rotate(axis, (W_SUN * m) * dt, Space.World);

        // Directional Light points from Sun to Earth
        if (sunLight != null && sunBody != null && earthBody != null)
        {
            Vector3 dirEarthToSun = (sunBody.position - earthBody.position).normalized;
            // Directional Light shines along -forward  so set forward to Earth->Sun
            sunLight.transform.rotation = Quaternion.LookRotation(-dirEarthToSun, axis);

            // feed custom shader sun direction if present
            if (skyboxInstance != null && hasUseMainLightProp && hasSunDirProp)
            {
                Vector3 origin = viewRef != null ? viewRef.position : transform.position;
                Vector3 dirViewToSun = (sunBody.position - origin).normalized;

                skyboxInstance.SetFloat(ID_UseMainLight, 0f);
                skyboxInstance.SetVector(ID_SunDir, new Vector4(dirViewToSun.x, dirViewToSun.y, dirViewToSun.z, 0f));
                if (hasSyncToSunProp) skyboxInstance.SetFloat(ID_SyncToSun, syncSkyToSun ? 1f : 0f);
            }
        }

        // Stars  rotate skybox
        if (skyboxInstance != null)
        {
            yawDeg = Mathf.Repeat(yawDeg + (W_STARS * m) * dt, 360f);

            if (hasSkyYaw)
                skyboxInstance.SetFloat(ID_SkyYawDeg, -yawDeg);
            else if (hasRotation)
                skyboxInstance.SetFloat(ID_Rotation, -yawDeg);

            if (updateGI && hasRotation)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(lastGIYaw, -yawDeg)) >= giUpdateEveryDegrees)
                {
                    DynamicGI.UpdateEnvironment();
                    lastGIYaw = -yawDeg;
                }
            }
        }
    }
}
