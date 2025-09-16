using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;   // <-- needed for PlayableDirector

public class SceneReloader : MonoBehaviour
{
    public void RestartActiveScene()
    {
        Time.timeScale = 1f;

        // Stop every Timeline in all loaded scenes (active & inactive objects)
        foreach (var director in Object.FindObjectsOfType<PlayableDirector>(true))
        {
            if (director) director.Stop(); // destroys its playable graph immediately
        }

        // Fresh instance of the current scene
        int idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx, LoadSceneMode.Single);
    }
}
