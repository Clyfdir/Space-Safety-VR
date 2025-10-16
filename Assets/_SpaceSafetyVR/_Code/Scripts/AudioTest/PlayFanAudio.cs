using System;
using UnityEngine;

public class PlayFanAudio : MonoBehaviour
{

    public string eventName;
    private void Start()
    {
        AudioManager.CallEvent(eventName, this.gameObject);
    }
}
