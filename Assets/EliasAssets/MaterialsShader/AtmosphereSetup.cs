using UnityEngine;

public class AtmosphereSetup : MonoBehaviour
{
    public Material atmosphereMaterial;
    public Light directionalLight;

    void Update()
    {
        if (directionalLight != null && atmosphereMaterial != null)
        {
            Vector3 lightDir = -directionalLight.transform.forward; // Direction toward surface
            Color lightCol = directionalLight.color * directionalLight.intensity;

            atmosphereMaterial.SetVector("_MainLightDirection", lightDir);
            atmosphereMaterial.SetColor("_CustomMainLightColor", lightCol);
        }
    }
}
