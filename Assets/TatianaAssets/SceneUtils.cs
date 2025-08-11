///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   AI GPT was used
///   Created: 11.07.2025
///   Last Change: 11.07.2025

/// Searching GO in scene by name, even not active objects or children

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneUtils
{
    // Walks every root (active OR inactive) and recurses into children
    public static GameObject FindDeep(string name)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            var t = FindDeepChild(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}

