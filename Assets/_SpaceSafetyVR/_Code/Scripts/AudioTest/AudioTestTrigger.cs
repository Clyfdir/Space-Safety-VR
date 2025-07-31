using System;
using UnityEngine;

public class AudioTestTrigger : MonoBehaviour
{

    public string name;

    private void PlaySound(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }

    private void Start()
    {
        PlaySound(name);
    }
}
