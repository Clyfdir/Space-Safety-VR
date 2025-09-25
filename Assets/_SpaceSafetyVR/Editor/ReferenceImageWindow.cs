using UnityEngine;
using UnityEditor;

public class ReferenceImageWindow : EditorWindow
{
    private Texture2D referenceImage;
    private Vector2 scrollPos;

    [MenuItem("Window/Reference Image")]
    public static void ShowWindow()
    {
        var window = GetWindow<ReferenceImageWindow>("Reference Image");
        window.minSize = new Vector2(200, 200);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Reference Image Viewer", EditorStyles.boldLabel);

        referenceImage = (Texture2D)EditorGUILayout.ObjectField(
            "Image",
            referenceImage,
            typeof(Texture2D),
            false
        );

        GUILayout.Space(10);

        if (referenceImage != null)
        {
            float aspect = (float)referenceImage.width / referenceImage.height;

            // available width inside the window
            float maxWidth = position.width - 20;
            float maxHeight = position.height - 80;

            // scale to fit while keeping aspect ratio
            float drawWidth = maxWidth;
            float drawHeight = drawWidth / aspect;

            if (drawHeight > maxHeight)
            {
                drawHeight = maxHeight;
                drawWidth = drawHeight * aspect;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            Rect rect = GUILayoutUtility.GetRect(drawWidth, drawHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            GUI.DrawTexture(rect, referenceImage, ScaleMode.ScaleToFit, true);
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Drag a texture here to view it as a reference.", MessageType.Info);
        }
    }
}