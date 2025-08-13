using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class InteractionButtonSceneSwitch : MonoBehaviour
{
    
    public GameObject GameObject;

    public PlayableDirector firstTimeline;
    public PlayableDirector nextTimeline;
    public PlayableDirector lastTimeline;
    
    private void Start()
    {
        firstTimeline.stopped += OnFirstTimelineStopped;
        nextTimeline.stopped += OnSecondTimelineStopped;
    }

    private void OnDisable()
    {
        firstTimeline.stopped -= OnFirstTimelineStopped;
        nextTimeline.stopped -= OnSecondTimelineStopped;
    }

    public void SceneSwitch()
    {
        Debug.Log("Yeeeah!");
        SceneManager.LoadScene("_SpaceSafetyVR/Blockout_v3");
    }
    
    public void SceneSwitchBack()
    {
        Debug.Log("Yeeeah!");
        SceneManager.LoadScene("Blockout_v2");
    }

    public void PlayTimeline()
    {
        firstTimeline.Play();
    }
    
    void OnFirstTimelineStopped(PlayableDirector director)
    {
        if (director == firstTimeline && nextTimeline != null)
        {
            nextTimeline.Play();
        }
    }

    void OnSecondTimelineStopped(PlayableDirector director)
    {
        if (director == nextTimeline && lastTimeline != null)
        {
            lastTimeline.Play();
        }
    }

}
