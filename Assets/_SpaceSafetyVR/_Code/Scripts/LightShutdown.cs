using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShutdown : MonoBehaviour
{
    [Header("Target Lights")]
    [Tooltip("If empty, will grab all Light components in children")]
    public List<Light> targetLights = new List<Light>();

    [Header("Target Objects")]
    [Tooltip("Optional objects whose visible color should flicker too")]
    public List<Renderer> flickerRenderers = new List<Renderer>();
    [Tooltip("Also scale emission color if present")]
    public bool affectEmissionIfPresent = true;

    [Header("Timing (Shutdown sequence)")]
    public float blackoutDuration = 2f;
    [Tooltip("Minimum stagger between restore steps (seconds)")]
    [SerializeField] private float minStartupDelay = 0.0f;
    [Tooltip("Maximum stagger between restore steps (seconds)")]
    [Range(0f, 5f)] public float maxStartupDelay = 0.35f;

    [Header("Flicker (single control)")]
    [Tooltip("0 = brief + slow blips, 1 = long + rapid stutter")]
    [Range(0f, 1f)] public float flickerIntensity = 0.5f;
    [Tooltip("Lowest brightness during flicker (as a fraction of original)")]
    [Range(0f, 1f)] public float minBrightness = 0.25f;

    [Header("Flicker Only (no shutdown)")]
    [Tooltip("How long the flicker-only lasts (seconds)")]
    [Min(0f)] public float flickerOnlyDuration = 1.0f;
    [Tooltip("0 = sparse/slow, 1 = dense/rapid")]
    [Range(0f, 1f)] public float flickerOnlyIntensity = 0.7f;
    [Tooltip("Stagger between items while flicker-only runs")]
    [Range(0f, 5f)] public float flickerOnlyStaggerMax = 0.15f;
    [Tooltip("Affect Light components during flicker-only")]
    public bool flickerOnlyAffectsLights = true;
    [Tooltip("Affect listed Renderers during flicker-only")]
    public bool flickerOnlyAffectsRenderers = true;

    [Header("Behavior")]
    public bool randomOrder = true;
    public bool autoRunOnStart = true;

    // --- Optional Wwise SFX (safe if Wwise is absent) ----------------------
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
    [Header("Wwise Audio")]
    [Tooltip("One-shot when everything shuts down")]
    public AK.Wwise.Event shutdownEvent;
    [Tooltip("Pool of short ticks/buzzes used during flicker")]
    public List<AK.Wwise.Event> flickerEvents = new List<AK.Wwise.Event>();
    [Tooltip("Optional one-shot when a light/renderer settles to 'on'")]
    public AK.Wwise.Event settleEvent;
    [Tooltip("Play random ticks during the flicker phase")]
    public bool playSoundsDuringFlicker = true;
    [Tooltip("Play a sound when a light/renderer finally stays on")]
    public bool playSoundOnSettle = true;
#endif
    // -----------------------------------------------------------------------

    struct LightState
    {
        public float intensity;
        public bool wasEnabled;
        public Color color;
        public LightShadows shadows;
        public float range;
    }

    class RenderState
    {
        public Renderer r;
        public MaterialPropertyBlock mpb;

        public bool hasColor;
        public bool hasEmission;
        public int colorId;
        public int emissionId;

        public Color baseColor;
        public Color emissionColor;

        public bool[] emissionKeywordWasOn; // original keyword state per material
        public Material[] instancedMats;    // per renderer material instances we toggle keywords on
    }

    readonly List<Light> _lights = new List<Light>();
    readonly Dictionary<Light, LightState> _originalLights = new Dictionary<Light, LightState>();
    readonly List<RenderState> _renderStates = new List<RenderState>();
    Coroutine _sequence;
    Coroutine _flickerOnlySeq;

    static readonly int ID_BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int ID_Color = Shader.PropertyToID("_Color");
    static readonly int ID_Emission = Shader.PropertyToID("_EmissionColor");
    const string KW_EMISSION = "_EMISSION";

    void Awake()
    {
        GrabLights();
        SaveOriginals();
        CacheRenderers();
        ClampValues();
    }

    void OnValidate() { ClampValues(); }

    void Start()
    {
        if (autoRunOnStart) TriggerFromTimeline();
    }

    // --- Public API ---------------------------------------------------------
    public void TriggerFromTimeline()
    {
        StopAllCoroutines();
        _sequence = StartCoroutine(RunSequence());
    }

    [ContextMenu("Trigger Light Shutdown")]
    public void TriggerNow()
    {
        TriggerFromTimeline();
    }

    [ContextMenu("Restore Originals Now")]
    public void RestoreOriginalsNow()
    {
        StopAllCoroutines();

        foreach (var kvp in _originalLights)
        {
            var l = kvp.Key; var s = kvp.Value;
            if (!l) continue;
            l.enabled = s.wasEnabled;
            l.intensity = s.intensity;
            l.color = s.color;
            l.shadows = s.shadows;
            l.range = s.range;
        }

        foreach (var rs in _renderStates)
        {
            if (rs == null || rs.r == null) continue;

            var mpb = rs.mpb ?? new MaterialPropertyBlock();
            rs.r.GetPropertyBlock(mpb);
            if (rs.hasColor) mpb.SetColor(rs.colorId, rs.baseColor);
            if (rs.hasEmission && affectEmissionIfPresent) mpb.SetColor(rs.emissionId, rs.emissionColor);
            rs.r.SetPropertyBlock(mpb);

            RestoreEmissionKeywords(rs.r, rs);
        }
    }

    // Flicker Only API
    public void TriggerFlickerOnly(float duration, float intensity,
        bool affectLights = true, bool affectRenderers = true)
    {
        StopCoroutineSafe(ref _flickerOnlySeq);
        _flickerOnlySeq = StartCoroutine(FlickerOnlyRoutine(duration, intensity, affectLights, affectRenderers));
    }

    [ContextMenu("Flicker Only (No Shutdown)")]
    public void TriggerFlickerOnlyFromInspector()
    {
        TriggerFlickerOnly(flickerOnlyDuration, flickerOnlyIntensity,
            flickerOnlyAffectsLights, flickerOnlyAffectsRenderers);
    }
    // -----------------------------------------------------------------------

    void GrabLights()
    {
        _lights.Clear();
        if (targetLights.Count > 0) { foreach (var l in targetLights) if (l) _lights.Add(l); }
        else { GetComponentsInChildren(true, _lights); }
    }

    void SaveOriginals()
    {
        _originalLights.Clear();
        foreach (var l in _lights)
        {
            if (!l) continue;
            _originalLights[l] = new LightState
            {
                intensity = l.intensity,
                wasEnabled = l.enabled,
                color = l.color,
                shadows = l.shadows,
                range = l.range
            };
        }
    }

    void CacheRenderers()
    {
        _renderStates.Clear();

        foreach (var r in flickerRenderers)
        {
            if (!r) continue;

            var rs = new RenderState { r = r, mpb = new MaterialPropertyBlock() };
            r.GetPropertyBlock(rs.mpb);

            if (HasProperty(r, ID_BaseColor))
            {
                rs.colorId = ID_BaseColor;
                rs.hasColor = true;
                rs.baseColor = GetColorSafe(r, rs.colorId, Color.white);
            }
            else if (HasProperty(r, ID_Color))
            {
                rs.colorId = ID_Color;
                rs.hasColor = true;
                rs.baseColor = GetColorSafe(r, rs.colorId, Color.white);
            }
            else rs.hasColor = false;

            rs.hasEmission = affectEmissionIfPresent && HasProperty(r, ID_Emission);
            if (rs.hasEmission) rs.emissionColor = GetColorSafe(r, ID_Emission, Color.black);

            var shared = r.sharedMaterials;
            rs.emissionKeywordWasOn = new bool[shared.Length];
            for (int i = 0; i < shared.Length; i++)
            {
                var m = shared[i];
                rs.emissionKeywordWasOn[i] = m && m.IsKeywordEnabled(KW_EMISSION);
            }

            _renderStates.Add(rs);
        }
    }

    static bool HasProperty(Renderer r, int id)
    {
        var mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (!m) continue;
            if (m.HasProperty(id)) return true;
        }
        return false;
    }

    static Color GetColorSafe(Renderer r, int id, Color fallback)
    {
        var mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (!m) continue;
            if (m.HasProperty(id)) return m.GetColor(id);
        }
        return fallback;
    }

    void ClampValues()
    {
        if (maxStartupDelay < minStartupDelay) (minStartupDelay, maxStartupDelay) = (maxStartupDelay, minStartupDelay);
        minBrightness = Mathf.Clamp01(minBrightness);
        flickerIntensity = Mathf.Clamp01(flickerIntensity);
        flickerOnlyIntensity = Mathf.Clamp01(flickerOnlyIntensity);
    }

    // Map intensity -> duration and min/max tick intervals (shared feel)
    void GetFlickerParams(out float duration, out float minInterval, out float maxInterval)
    {
        // fixed ranges (change here if you want different feel)
        const float minDuration = 0.20f; // at intensity 0
        const float maxDuration = 1.20f; // at intensity 1
        const float slowInterval = 0.20f; // at intensity 0
        const float fastInterval = 0.02f; // at intensity 1
        const float jitterFraction = 0.50f; // ±50% around base interval

        duration = Mathf.Lerp(minDuration, maxDuration, flickerIntensity);
        float baseInterval = Mathf.Lerp(slowInterval, fastInterval, flickerIntensity);

        float j = jitterFraction;
        minInterval = Mathf.Max(0.001f, baseInterval * (1f - j));
        maxInterval = baseInterval * (1f + j);
        if (maxInterval < minInterval) { float t = minInterval; minInterval = maxInterval; maxInterval = t; }
    }

    // Helper for custom duration/intensity (used by Flicker Only)
    void GetParamsFor(float intensity, float durationOverride, out float duration, out float minInterval, out float maxInterval)
    {
        intensity = Mathf.Clamp01(intensity);

        const float minDuration = 0.20f;
        const float maxDuration = 1.20f;
        const float slowInterval = 0.20f;
        const float fastInterval = 0.02f;
        const float jitterFraction = 0.50f;

        duration = (durationOverride > 0f)
            ? durationOverride
            : Mathf.Lerp(minDuration, maxDuration, intensity);

        float baseInterval = Mathf.Lerp(slowInterval, fastInterval, intensity);
        float j = jitterFraction;
        minInterval = Mathf.Max(0.001f, baseInterval * (1f - j));
        maxInterval = baseInterval * (1f + j);
        if (maxInterval < minInterval) { float t = minInterval; minInterval = maxInterval; maxInterval = t; }
    }

    IEnumerator RunSequence()
    {
        // lights off
        foreach (var l in _lights) if (l) l.enabled = false;

        // hard blackout for objects
        SetAllRenderersBrightness(0f);
        ForceEmissionOffForAll();

        // Wwise shutdown thunk
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (shutdownEvent != null) PostAt(shutdownEvent, transform.position);
#endif

        if (blackoutDuration > 0f) yield return new WaitForSeconds(blackoutDuration);

        // choose order for lights and renderers separately
        var lightOrder = new List<Light>(_lights);
        lightOrder.RemoveAll(l => !l);

        var rendererOrder = new List<RenderState>(_renderStates);
        rendererOrder.RemoveAll(rs => rs == null || rs.r == null);

        if (randomOrder)
        {
            Shuffle(lightOrder);
            Shuffle(rendererOrder);
        }

        // step through both lists in lockstep: one light and one renderer per step
        int steps = Mathf.Max(lightOrder.Count, rendererOrder.Count);
        for (int i = 0; i < steps; i++)
        {
            if (i < lightOrder.Count) StartCoroutine(FlickerLightThenOn(lightOrder[i]));
            if (i < rendererOrder.Count) StartCoroutine(FlickerRendererThenOn(rendererOrder[i]));

            yield return new WaitForSeconds(Random.Range(minStartupDelay, maxStartupDelay));
        }
    }

    // Fisher–Yates
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    IEnumerator FlickerLightThenOn(Light l)
    {
        if (!l) yield break;

        if (!_originalLights.TryGetValue(l, out var state))
        {
            state = new LightState
            {
                intensity = l.intensity <= 0f ? 1f : l.intensity,
                wasEnabled = true,
                color = l.color,
                shadows = l.shadows,
                range = l.range
            };
            _originalLights[l] = state;
        }

        GetFlickerParams(out float duration, out float minInt, out float maxInt);

        float t = 0f;
        l.enabled = true;
        l.intensity = state.intensity * Random.Range(0.5f, 1.2f);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundsDuringFlicker) PostRandomFlickAt(l.transform.position, 0.9f);
#endif

        while (t < duration)
        {
            if (Random.value < 0.35f)
            {
                l.enabled = !l.enabled;
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
                if (playSoundsDuringFlicker && l.enabled && Random.value < 0.6f) PostRandomFlickAt(l.transform.position, 0.7f);
#endif
            }
            else
            {
                l.enabled = true;
                float mul = Random.Range(minBrightness, 1f);
                l.intensity = state.intensity * mul;
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
                if (playSoundsDuringFlicker && Random.value < 0.4f) PostRandomFlickAt(l.transform.position, 0.6f);
#endif
            }

            float wait = Random.Range(minInt, maxInt);
            yield return new WaitForSeconds(wait);
            t += wait;
        }

        l.enabled = true;
        l.intensity = state.intensity;
        l.color = state.color;
        l.shadows = state.shadows;
        l.range = state.range;

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundOnSettle) PostSettleAt(l.transform.position);
#endif
    }

    IEnumerator FlickerRendererThenOn(RenderState rs)
    {
        if (rs == null || rs.r == null) yield break;

        GetFlickerParams(out float duration, out float minInt, out float maxInt);

        float t = 0f;

        SetRendererBrightness(rs, Random.Range(0.5f, 1.0f));
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundsDuringFlicker) PostRandomFlickAt(rs.r.bounds.center, 0.9f);
#endif

        while (t < duration)
        {
            float mul = (Random.value < 0.35f) ? 0f : Random.Range(minBrightness, 1f);
            SetRendererBrightness(rs, mul);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
            if (playSoundsDuringFlicker && Random.value < 0.4f) PostRandomFlickAt(rs.r.bounds.center, 0.6f);
#endif

            float wait = Random.Range(minInt, maxInt);
            yield return new WaitForSeconds(wait);
            t += wait;
        }

        SetRendererBrightness(rs, 1f);
        RestoreEmissionKeywords(rs.r, rs);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundOnSettle) PostSettleAt(rs.r.bounds.center);
#endif
    }

    // --- Flicker Only path (no shutdown) -----------------------------------
    IEnumerator FlickerOnlyRoutine(float durationOverride, float intensity,
        bool affectLights, bool affectRenderers)
    {
        GetParamsFor(intensity, durationOverride, out float duration, out float minInt, out float maxInt);

        var lightOrder = new List<Light>(_lights);
        lightOrder.RemoveAll(l => !l);

        var rendererOrder = new List<RenderState>(_renderStates);
        rendererOrder.RemoveAll(rs => rs == null || rs.r == null);

        if (randomOrder)
        {
            Shuffle(lightOrder);
            Shuffle(rendererOrder);
        }

        int steps = Mathf.Max(affectLights ? lightOrder.Count : 0, affectRenderers ? rendererOrder.Count : 0);
        for (int i = 0; i < steps; i++)
        {
            if (affectLights && i < lightOrder.Count)
                StartCoroutine(FlickerLightCustom(lightOrder[i], duration, minInt, maxInt));

            if (affectRenderers && i < rendererOrder.Count)
                StartCoroutine(FlickerRendererCustom(rendererOrder[i], duration, minInt, maxInt, restoreEmissionKeyword: true));

            float wait = (flickerOnlyStaggerMax > 0f) ? Random.Range(0f, flickerOnlyStaggerMax) : 0f;
            if (wait > 0f) yield return new WaitForSeconds(wait);
            else yield return null;
        }
    }

    IEnumerator FlickerLightCustom(Light l, float duration, float minInt, float maxInt)
    {
        if (!l) yield break;

        if (!_originalLights.TryGetValue(l, out var state))
        {
            state = new LightState { intensity = (l.intensity <= 0f ? 1f : l.intensity), wasEnabled = l.enabled, color = l.color, shadows = l.shadows, range = l.range };
            _originalLights[l] = state;
        }

        float t = 0f;
        bool prevEnabled = l.enabled;

        l.enabled = true;
        l.intensity = state.intensity * Random.Range(0.5f, 1.2f);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundsDuringFlicker) PostRandomFlickAt(l.transform.position, 0.9f);
#endif

        while (t < duration)
        {
            if (Random.value < 0.35f)
            {
                l.enabled = !l.enabled;
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
                if (playSoundsDuringFlicker && l.enabled && Random.value < 0.6f) PostRandomFlickAt(l.transform.position, 0.7f);
#endif
            }
            else
            {
                l.enabled = true;
                float mul = Random.Range(minBrightness, 1f);
                l.intensity = state.intensity * mul;
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
                if (playSoundsDuringFlicker && Random.value < 0.4f) PostRandomFlickAt(l.transform.position, 0.6f);
#endif
            }

            float wait = Random.Range(minInt, maxInt);
            yield return new WaitForSeconds(wait);
            t += wait;
        }

        l.enabled = prevEnabled;
        l.intensity = state.intensity;
        l.color = state.color;
        l.shadows = state.shadows;
        l.range = state.range;

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundOnSettle) PostSettleAt(l.transform.position);
#endif
    }

    IEnumerator FlickerRendererCustom(RenderState rs, float duration, float minInt, float maxInt, bool restoreEmissionKeyword)
    {
        if (rs == null || rs.r == null) yield break;

        float t = 0f;

        SetRendererBrightness(rs, Random.Range(0.5f, 1.0f));
#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundsDuringFlicker) PostRandomFlickAt(rs.r.bounds.center, 0.9f);
#endif

        while (t < duration)
        {
            float mul = (Random.value < 0.35f) ? 0f : Random.Range(minBrightness, 1f);
            SetRendererBrightness(rs, mul);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
            if (playSoundsDuringFlicker && Random.value < 0.4f) PostRandomFlickAt(rs.r.bounds.center, 0.6f);
#endif

            float wait = Random.Range(minInt, maxInt);
            yield return new WaitForSeconds(wait);
            t += wait;
        }

        SetRendererBrightness(rs, 1f);
        if (restoreEmissionKeyword) RestoreEmissionKeywords(rs.r, rs);

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
        if (playSoundOnSettle) PostSettleAt(rs.r.bounds.center);
#endif
    }
    // -----------------------------------------------------------------------

    void SetRendererBrightness(RenderState rs, float mul)
    {
        if (rs == null || rs.r == null) return;

        var mpb = rs.mpb ?? new MaterialPropertyBlock();
        rs.r.GetPropertyBlock(mpb);

        if (rs.hasColor)
        {
            Color c = rs.baseColor;
            c.r *= mul; c.g *= mul; c.b *= mul;
            mpb.SetColor(rs.colorId, c);
        }

        if (rs.hasEmission && affectEmissionIfPresent)
        {
            Color e = rs.emissionColor;
            e.r *= mul; e.g *= mul; e.b *= mul;
            mpb.SetColor(rs.emissionId, e);
        }

        rs.r.SetPropertyBlock(mpb);
    }

    void SetAllRenderersBrightness(float mul)
    {
        foreach (var rs in _renderStates)
            SetRendererBrightness(rs, mul);
    }

    void ForceEmissionOffForAll()
    {
        foreach (var rs in _renderStates)
            ForceEmissionOff(rs.r, rs);
    }

    void ForceEmissionOff(Renderer r, RenderState rs)
    {
        if (r == null) return;

        if (rs.instancedMats == null || rs.instancedMats.Length != r.sharedMaterials.Length)
            rs.instancedMats = r.materials; // instantiate per renderer

        foreach (var m in rs.instancedMats)
            if (m) m.DisableKeyword(KW_EMISSION);
    }

    void RestoreEmissionKeywords(Renderer r, RenderState rs)
    {
        if (r == null) return;

        if (rs.instancedMats == null || rs.instancedMats.Length != r.sharedMaterials.Length)
            rs.instancedMats = r.materials;

        for (int i = 0; i < rs.instancedMats.Length; i++)
        {
            var m = rs.instancedMats[i];
            if (!m) continue;
            bool wasOn = (rs.emissionKeywordWasOn != null && i < rs.emissionKeywordWasOn.Length) ? rs.emissionKeywordWasOn[i] : false;
            if (wasOn) m.EnableKeyword(KW_EMISSION);
            else m.DisableKeyword(KW_EMISSION);
        }
    }

#if AK_WWISE_ADDRESSABLES || AK_WWISE || WWISE || AK_WWISE_UNITY
    // --- Wwise helpers ------------------------------------------------------
    void PostAt(AK.Wwise.Event ev, Vector3 pos, float life = 3f)
    {
        if (ev == null) return;
        var go = new GameObject("LightSFX_Wwise");
        go.transform.position = pos;
        go.AddComponent<AkGameObj>();
        ev.Post(go);
        Destroy(go, life);
    }

    void PostRandomFlickAt(Vector3 pos, float life = 2f)
    {
        if (flickerEvents == null || flickerEvents.Count == 0) return;
        var ev = flickerEvents[Random.Range(0, flickerEvents.Count)];
        PostAt(ev, pos, life);
    }

    void PostSettleAt(Vector3 pos, float life = 2f)
    {
        if (settleEvent != null) PostAt(settleEvent, pos, life);
        else PostRandomFlickAt(pos, life);
    }
    // -----------------------------------------------------------------------
#endif

    void StopCoroutineSafe(ref Coroutine c) { if (c != null) { StopCoroutine(c); c = null; } }
}
