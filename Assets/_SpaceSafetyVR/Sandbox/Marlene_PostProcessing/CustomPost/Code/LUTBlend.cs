using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Or HDRP if you’re using HDRP
public enum LUTSize
    {
        Size16 = 16,
        Size32 = 32,
        Size64 = 64
    }

[System.Serializable]
public sealed class LUTSizeParameter : VolumeParameter<LUTSize>
{
    public LUTSizeParameter(LUTSize value, bool overrideState = true) : base(value, overrideState) { }
}

[System.Serializable, VolumeComponentMenu("Custom/LUTBlend")]
public class LUTBlend : VolumeComponent, IPostProcessComponent
{
   
    // Two LUTs to blend
    public TextureParameter lutA = new TextureParameter(null);
    public TextureParameter lutB = new TextureParameter(null);

    // Main slider artists/timeline will keyframe
    public ClampedFloatParameter blend = new ClampedFloatParameter(0f, 0f, 1f);

    // Extra VR-safe artist controls
    public FloatParameter exposureOffset = new FloatParameter(0f);
    public FloatParameter contrastBoost = new FloatParameter(1f);
    public ColorParameter tint = new ColorParameter(Color.white, true, false, true);
    
    public LUTSizeParameter lutSize = new LUTSizeParameter(LUTSize.Size32);


    public bool IsActive() => true;//lutA.value != null && lutB.value != null && blend.value > 0f;
    public bool IsTileCompatible() => false;
}
