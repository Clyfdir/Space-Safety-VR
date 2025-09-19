using UnityEngine;
using System.Collections;

public class EarthController : MonoBehaviour
{
    public Material earthMat;
    public Light directionalLight;

    [Header("City Light Fade Settings")]
    public float fadeDuration = 2f;  

    private Coroutine fadeRoutine;

    void Update()
    {
        if (directionalLight != null && earthMat != null)
        {
            Vector3 lightDir = -directionalLight.transform.forward;
            Color lightCol = directionalLight.color * directionalLight.intensity;

            earthMat.SetVector("_MainLightDirection", lightDir);
            earthMat.SetColor("_CustomMainLightColor", lightCol);
        }

        if (earthMat != null)
            earthMat.SetVector("_PlanetCenter", transform.position);
    }


    [ContextMenu("Fade In City Lights")]
    public void FadeIn()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(1f, 0f));
    }

    [ContextMenu("Fade Out City Lights")]
    public void FadeOut()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f));
    }


    private IEnumerator FadeRoutine(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float value = Mathf.Lerp(from, to, t);
            earthMat.SetFloat("_BlackoutStrength", value); 
            yield return null;
        }

        earthMat.SetFloat("_BlackoutStrength", to);
        fadeRoutine = null;
    }
}
