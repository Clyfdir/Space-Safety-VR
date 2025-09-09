using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureGrayscaleConverter : EditorWindow
{
    Texture2D sourceTexture;

    // Channel weights
    float redWeight = 0.3f;
    float greenWeight = 0.59f;
    float blueWeight = 0.11f;

    // Contrast factor (1 = no change)
    float contrast = 1.0f;

    [MenuItem("Tools/Texture to Grayscale Advanced")]
    public static void ShowWindow()
    {
        GetWindow<TextureGrayscaleConverter>("Texture to Grayscale");
    }

    void OnGUI()
    {
        GUILayout.Label("Convert Texture to Grayscale", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);

        GUILayout.Space(10);
        GUILayout.Label("Channel Weights (sum doesn't need to be 1)", EditorStyles.miniBoldLabel);
        redWeight = EditorGUILayout.Slider("Red Weight", redWeight, 0f, 2f);
        greenWeight = EditorGUILayout.Slider("Green Weight", greenWeight, 0f, 2f);
        blueWeight = EditorGUILayout.Slider("Blue Weight", blueWeight, 0f, 2f);

        GUILayout.Space(5);
        GUILayout.Label("Contrast Adjustment", EditorStyles.miniBoldLabel);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0f, 3f);

        GUILayout.Space(10);
        if (sourceTexture != null && GUILayout.Button("Convert and Save"))
        {
            ConvertAndSave();
        }
    }

    void ConvertAndSave()
    {
        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        Texture2D grayTex = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

        for (int y = 0; y < sourceTexture.height; y++)
        {
            for (int x = 0; x < sourceTexture.width; x++)
            {
                Color pixel = sourceTexture.GetPixel(x, y);

                // Weighted grayscale
                float gray = (pixel.r * redWeight) + (pixel.g * greenWeight) + (pixel.b * blueWeight);

                // Apply contrast adjustment
                // Formula: ((value - 0.5) * contrast) + 0.5
                gray = Mathf.Clamp01(((gray - 0.5f) * contrast) + 0.5f);

                grayTex.SetPixel(x, y, new Color(gray, gray, gray, pixel.a));
            }
        }

        grayTex.Apply();

        string savePath = EditorUtility.SaveFilePanel("Save Grayscale Texture", "Assets", sourceTexture.name + "_gray", "png");

        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllBytes(savePath, grayTex.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log("Grayscale texture saved to: " + savePath);
        }
    }
}
