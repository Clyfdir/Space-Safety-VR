using UnityEngine;

public class EmissionBlinkController : MonoBehaviour
{
    [Tooltip("All shared materials you want to blink. Editing these affects every renderer using them.")]
    public Material[] materials;

    [Header("Blink Settings")]
    [Min(0.1f)] public float rateHz = 2f;          // blinks per second
    [Range(0f, 1f)] public float dutyCycle = 0.5f; // fraction of time ON (0..1)
    public bool randomPhasePerMaterial = true;     // stagger start times

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");   // URP
    static readonly int MainTexColorID = Shader.PropertyToID("_Color");       // Built-in

    Color[] onColors;
    float[] phase;

    void OnEnable()
    {
        if (materials == null) return;

        int n = materials.Length;
        onColors = new Color[n];
        phase = new float[n];

        for (int i = 0; i < n; i++)
        {
            var m = materials[i];
            if (!m) continue;

            m.EnableKeyword("_EMISSION");

            // Read original emission
            Color emission = m.GetColor(EmissionColorID);

            // If emission is black, use the base/albedo color as emission instead
            if (emission.maxColorComponent <= 0.001f)
            {
                if (m.HasProperty(BaseColorID))
                    emission = m.GetColor(BaseColorID);
                else if (m.HasProperty(MainTexColorID))
                    emission = m.GetColor(MainTexColorID);
                else
                    emission = Color.white; // fallback
            }

            onColors[i] = emission;
            phase[i] = randomPhasePerMaterial ? Random.value * 10f : 0f;
        }
    }

    void Update()
    {
        if (materials == null) return;

        for (int i = 0; i < materials.Length; i++)
        {
            var m = materials[i];
            if (!m) continue;

            bool on = TargetAtTime(Time.time + phase[i]);
            m.SetColor(EmissionColorID, on ? onColors[i] : Color.black);
        }
    }

    bool TargetAtTime(float t)
    {
        float frac = Mathf.Repeat(t * rateHz, 1f);
        return frac < dutyCycle;
    }
}
