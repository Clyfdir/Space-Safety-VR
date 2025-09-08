using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class SceneSwitch : MonoBehaviour
{
    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
