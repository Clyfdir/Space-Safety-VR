///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025
///   Created: 10.09.2025

/// Temporal script just to test different ends of the experience, how they look like.
/// This script makes appear the sphere around camera, with text which informs about the end of experience.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FadeMaterialAlpha : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;              // defaults to this Renderer
    [SerializeField] private string colorProperty = "_BaseColor";  // URP/Lit. Falls back to "_Color".

    [Header("Fade Settings")]
    [SerializeField] private float duration = 30f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private MaterialPropertyBlock _mpb;
    private int _colorID;
    private Color _rgb;                // store RGB (A handled separately)
    private float _currentAlpha = 1f;  // track current alpha
    private Coroutine _routine;

    private void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        var mat = targetRenderer ? targetRenderer.sharedMaterial : null;
        if (!mat) { Debug.LogWarning("FadeMaterialAlpha: No material on renderer."); return; }

        if (!mat.HasProperty(colorProperty))
            colorProperty = mat.HasProperty("_Color") ? "_Color" : colorProperty;

        _colorID = Shader.PropertyToID(colorProperty);

        // Read initial color from material
        Color c = mat.GetColor(_colorID);
        _rgb = new Color(c.r, c.g, c.b, 1f);
        _currentAlpha = c.a;
    }

    private void OnEnable()
    {
        if (playOnEnable) FadeIn();
    }

    private void OnDisable()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        SetAlpha(0f);        // instantly transparent when disabled
    }

    // Public API -------------------------------------------------------

    /// Fade alpha 0 - 1 over 'duration'
    public void FadeIn() => StartFade(_currentAlpha, 1f, duration);

    /// Fade alpha 1 - 0 over 'duration'
    public void FadeOut() => StartFade(_currentAlpha, 0f, duration);

    /// Fade from current to target alpha (0..1) over seconds
    public void StartFadeTo(float targetAlpha01, float seconds)
        => StartFade(_currentAlpha, Mathf.Clamp01(targetAlpha01), Mathf.Max(0f, seconds));

    /// Instantly set alpha (0..1)
    public void SetAlpha(float alpha01) => ApplyAlpha(Mathf.Clamp01(alpha01));

    // Core -------------------------------------------------------------

    private void StartFade(float from, float to, float seconds)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeCo(from, to, seconds));
    }

    private IEnumerator FadeCo(float from, float to, float seconds)
    {
        if (seconds <= 0f) { ApplyAlpha(to); _routine = null; yield break; }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            float a = Mathf.Lerp(from, to, easing.Evaluate(k));
            ApplyAlpha(a);
            yield return null;
        }

        ApplyAlpha(to);
        _routine = null;
    }

    private void ApplyAlpha(float a01)
    {
        if (!targetRenderer) return;

        targetRenderer.GetPropertyBlock(_mpb);
        var c = _rgb; c.a = a01;
        _mpb.SetColor(_colorID, c);
        targetRenderer.SetPropertyBlock(_mpb);
        _currentAlpha = a01;
    }
}

