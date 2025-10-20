using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneSwitcherOnAction : MonoBehaviour
{
    [Serializable]
    public class ActionToScene
    {
        public InputActionReference action;
        [Tooltip("Scene name or build index")] public string sceneName;
        [Tooltip("If set, overrides sceneName")] public int sceneBuildIndex = -1;
    }

    [Tooltip("Map one or more input actions to scenes")]
    public List<ActionToScene> mappings = new();

    [Tooltip("Optional cooldown to prevent double loads, seconds")]
    public float cooldown = 0.5f;

    float _lastLoadTime = -999f;

    void OnEnable()
    {
        foreach (var m in mappings)
        {
            if (m?.action != null && m.action.action != null)
            {
                m.action.action.performed += OnPerformed;
                // Make sure the action is enabled
                if (!m.action.action.enabled) m.action.action.Enable();
            }
        }
    }

    void OnDisable()
    {
        foreach (var m in mappings)
        {
            if (m?.action != null && m.action.action != null)
            {
                m.action.action.performed -= OnPerformed;
            }
        }
    }

    void OnPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - _lastLoadTime < cooldown) return;
        _lastLoadTime = Time.unscaledTime;
        
        // Stop all audio before changing scenes
        //AudioManager.StopAllEvents();

        // Find which mapping fired
        foreach (var m in mappings)
        {
            if (m?.action == null || m.action.action == null) continue;
            if (ctx.action != m.action.action) continue;

            if (m.sceneBuildIndex >= 0)
            {
                SceneManager.LoadScene(m.sceneBuildIndex, LoadSceneMode.Single);
            }
            else if (!string.IsNullOrWhiteSpace(m.sceneName))
            {
                SceneManager.LoadScene(m.sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("SceneSwitcherOnAction has a mapping without a scene assigned.");
            }
            return;
        }
    }
}
