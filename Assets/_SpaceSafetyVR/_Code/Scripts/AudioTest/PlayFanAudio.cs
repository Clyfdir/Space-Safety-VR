using System;
using UnityEngine;

public class PlayFanAudio : MonoBehaviour
{

    public string eventName;
    public string stopEventName;
    private void Start()
    {
        AudioManager.CallEvent(eventName, this.gameObject);
    }

    private void OnDisable()
    {
        AudioManager.CallEvent(stopEventName, this.gameObject);
    }
}

