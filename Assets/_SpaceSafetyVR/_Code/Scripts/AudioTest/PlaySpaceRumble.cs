using System;
using UnityEngine;

public class PlaySpaceRumble : MonoBehaviour
{

    public string playEventName;
    public string stopEventName;

    private void Start()
    {
         AudioManager.CallEvent(playEventName, this.gameObject);
    }

    private void OnEnable()
    {
        AudioManager.CallEvent(playEventName, this.gameObject);
    }

    private void OnDisable()
    {
        AudioManager.CallEvent(stopEventName, this.gameObject);
    }
}
