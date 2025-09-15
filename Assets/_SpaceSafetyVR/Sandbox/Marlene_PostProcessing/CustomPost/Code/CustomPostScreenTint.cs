using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Or HDRP if you’re using HDRP


[System.Serializable, VolumeComponentMenu("Custom/CustomPostScreenTint")]
public class CustomPostScreenTint : VolumeComponent, IPostProcessComponent
{
    public bool IsActive() => true;//lutA.value != null && lutB.value != null && blend.value > 0f;
    public bool IsTileCompatible() => false;
}
