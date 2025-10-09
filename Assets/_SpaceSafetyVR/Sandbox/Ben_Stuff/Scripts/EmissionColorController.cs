using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class EmissionColorController : MonoBehaviour
{
    [Header("Emission Control")]
    [ColorUsage(true, true)]
    public Color emissionColor = Color.black;

    [Tooltip("Name of the emission color property (HDRP/URP usually '_EmissionColor')")]
    public string emissionProperty = "_EmissionColor";

    private List<Material> _materials = new List<Material>();
    private Color _lastColor;

    void Awake()
    {
        CacheAllMaterials();
        ApplyEmissionColor();
    }

    void OnValidate()
    {
        // Re-cache materials if changes happen in the editor
        CacheAllMaterials();
        ApplyEmissionColor();
    }

    void Update()
    {
        // Live updates in editor or during Timeline animation
        if (emissionColor != _lastColor)
            ApplyEmissionColor();
    }

    /// <summary>
    /// Finds all materials on Renderer components under this object.
    /// </summary>
    private void CacheAllMaterials()
    {
        _materials.Clear();

        var renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        foreach (var renderer in renderers)
        {
            Material[] mats;

            if (Application.isPlaying)
            {
                // Runtime: use instance materials (safe)
                mats = renderer.materials;
            }
            else
            {
                // Editor: use shared materials (no instancing)
                mats = renderer.sharedMaterials;
            }

            foreach (var mat in mats)
            {
                if (mat != null)
                    _materials.Add(mat);
            }
        }
    }


    /// <summary>
    /// Applies the emission color to all cached materials.
    /// </summary>
    private void ApplyEmissionColor()
    {
        foreach (var mat in _materials)
        {
            if (mat && mat.HasProperty(emissionProperty))
            {
                mat.SetColor(emissionProperty, emissionColor);
                if (!mat.IsKeywordEnabled("_EMISSION"))
                    mat.EnableKeyword("_EMISSION");
            }
        }

        _lastColor = emissionColor;
    }
}
