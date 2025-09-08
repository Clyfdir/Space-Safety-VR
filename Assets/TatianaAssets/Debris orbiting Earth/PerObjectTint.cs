///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 04.09.2025
///   Created: 04.09.2025

/// Assigns a random tint to material, per object

using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PerObjectTint : MonoBehaviour
{
    // Define colors
    private static readonly Color[] tintOptions = new Color[]
    {
        new Color32(0x44, 0x7A, 0xD1, 0xFF), // #447AD1 (blue)
        new Color32(0x44, 0x9D, 0xD1, 0xFF), // #449DD1 (cyan-blue)
        new Color32(0xD1, 0xB6, 0x44, 0xFF), // #D1B644 (gold)
        new Color32(0xD1, 0x8B, 0x44, 0xFF), // #D18B44 (orange-brown)
        new Color32(0x88, 0x88, 0x88, 0xFF), // #888888 (gray)
        new Color32(0xC8, 0xC7, 0xC7, 0xFF), // #C8C7C7 (light gray)
        new Color32(0x69, 0x69, 0x69, 0xFF), // #696969 (dark gray)
        new Color32(0xEE, 0xEE, 0xEE, 0xFF), // #EEEEEE (very light gray)
        new Color32(0xFF, 0xFF, 0xFF, 0xFF)  // #FFFFFF (white)
    };

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock mpb;
    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();

        // Pick random tint from the three options
        var chosenTint = tintOptions[Random.Range(0, tintOptions.Length)];

        rend.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorID, chosenTint);
        rend.SetPropertyBlock(mpb);
    }
}
