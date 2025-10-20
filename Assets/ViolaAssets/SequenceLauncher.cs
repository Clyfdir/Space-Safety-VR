using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;         // InputActionReference
using UnityEngine.Playables;           // PlayableDirector
using UnityEngine.SceneManagement;     // SceneManager

public class SequenceLauncher : MonoBehaviour
{
    [System.Serializable]
    public class Sequence
    {
        [Tooltip("Scene name or build index as string (e.g. \"2\"). Prefer unique scene names.")]
        public string scene;
        [Tooltip("Name of the PlayableDirector GameObject (or leave empty to use first director in scene).")]
        public string directorObjectName;
        [Tooltip("Optional: PlayableAsset override (if you want to replace whatever is on the director).")]
        public PlayableAsset playableOverride;
    }

    [Header("Map buttons to sequences (fill what you need)")]
    public InputActionReference rightPrimaryButton;   // A
    public Sequence rightPrimarySequence;

    public InputActionReference rightSecondaryButton; // B
    public Sequence rightSecondarySequence;

    public InputActionReference leftPrimaryButton;    // X
    public Sequence leftPrimarySequence;

    public InputActionReference leftSecondaryButton;  // Y
    public Sequence leftSecondarySequence;

    public InputActionReference menuButton;           // Menu / Oculus button (if available)
    public Sequence menuSequence;

    public InputActionReference rightThumbstickClick; // R stick click, optional
    public Sequence rightThumbstickSequence;

    [Header("Loading")]
    [Tooltip("Use async Single load. We'll stop all directors via sceneLoaded callback before first frame.")]
    public bool loadAsync = true;

    void OnEnable()
    {
        Bind(rightPrimaryButton, () => Launch(rightPrimarySequence));
        Bind(rightSecondaryButton, () => Launch(rightSecondarySequence));
        Bind(leftPrimaryButton, () => Launch(leftPrimarySequence));
        Bind(leftSecondaryButton, () => Launch(leftSecondarySequence));
        Bind(menuButton, () => Launch(menuSequence));
        Bind(rightThumbstickClick, () => Launch(rightThumbstickSequence));

        SceneManager.sceneLoaded += OnSceneLoaded_StopAllDirectors;
    }

    void OnDisable()
    {
        Unbind(rightPrimaryButton);
        Unbind(rightSecondaryButton);
        Unbind(leftPrimaryButton);
        Unbind(leftSecondaryButton);
        Unbind(menuButton);
        Unbind(rightThumbstickClick);

        SceneManager.sceneLoaded -= OnSceneLoaded_StopAllDirectors;
    }

    void Bind(InputActionReference aref, System.Action fire)
    {
        if (!aref || aref.action == null) return;
        var a = aref.action;
        a.Enable();
        a.performed += _ => fire();
    }

    void Unbind(InputActionReference aref)
    {
        if (!aref || aref.action == null) return;
        var a = aref.action;
        a.performed -= _ => { }; // (safe no-op)
        a.Disable();
    }

    // --- Main entry: load the scene, then play the chosen timeline ---
    public void Launch(Sequence seq)
    {
        if (seq == null || string.IsNullOrEmpty(seq.scene)) return;

        // Stop any currently-running timelines right away in the current scene
        foreach (var d in FindObjectsOfType<PlayableDirector>(true))
            d.Stop(); // destroys graph so it stops writing tracks immediately. :contentReference[oaicite:1]{index=1}

        if (int.TryParse(seq.scene, out int buildIndex))
        {
            if (loadAsync) SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            else SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
        }
        else
        {
            if (loadAsync) SceneManager.LoadSceneAsync(seq.scene, LoadSceneMode.Single);
            else SceneManager.LoadScene(seq.scene, LoadSceneMode.Single);
        }

        // Start a coroutine that waits one frame after the new scene is set, then plays the wanted director
        StartCoroutine(PlayAfterLoad(seq));
    }

    IEnumerator PlayAfterLoad(Sequence seq)
    {
        // Wait until the active scene becomes the target scene (async safe)
        yield return null; // let LoadScene/Async progress. OnSceneLoaded will already have stopped directors.

        // Find the director we want
        var directors = FindObjectsOfType<PlayableDirector>(true);
        PlayableDirector chosen = null;

        if (!string.IsNullOrEmpty(seq.directorObjectName))
        {
            chosen = directors.FirstOrDefault(d => d.name == seq.directorObjectName);
        }
        if (chosen == null)
        {
            // Fallback: first director in the newly active scene
            var active = SceneManager.GetActiveScene();
            chosen = directors.FirstOrDefault(d => d.gameObject.scene == active);
        }

        if (chosen != null)
        {
            // Prevent autoplay in case the component had PlayOnAwake checked
            // (We’re after load; we also stopped all in OnSceneLoaded below.)
            chosen.playOnAwake = false;  // property exists on PlayableDirector. :contentReference[oaicite:2]{index=2}

            if (seq.playableOverride != null)
                chosen.playableAsset = seq.playableOverride;

            chosen.time = 0;
            chosen.Play();               // explicitly start only this Timeline. :contentReference[oaicite:3]{index=3}
        }
        else
        {
            Debug.LogWarning("SequenceLauncher: No PlayableDirector found to play in loaded scene.");
        }
    }

    // This runs as soon as Unity reports the scene is loaded. We immediately stop every director
    // so nothing 'plays on awake' on the first frame. Then PlayAfterLoad starts only the chosen one.
    void OnSceneLoaded_StopAllDirectors(Scene scene, LoadSceneMode mode)
    {
        foreach (var d in FindObjectsOfType<PlayableDirector>(true))
        {
            // Ensure no autoplay from any director that was set to Play On Awake.
            d.Stop(); // destroys its graph; prevents it from writing tracks further. :contentReference[oaicite:4]{index=4}
            d.playOnAwake = false; // also clear the flag at runtime going forward. :contentReference[oaicite:5]{index=5}
        }
    }
}
