using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayButtonSound : MonoBehaviour
{

    public string eventName;

    public void PlayButton()
    {
        AudioManager.CallEvent(eventName, this.gameObject);
    }


    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("EEEEE");
            AudioManager.CallEvent(eventName, this.gameObject);
        }

    }
}
