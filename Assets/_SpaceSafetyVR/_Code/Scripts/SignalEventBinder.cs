using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public struct SignalEventPair
{
    public SignalAsset signalAsset;
    public GameEvent gameEvent;
}

public class SignalEventBinder : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private List<SignalEventPair> events;

    private void OnEnable()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
            if (director == null)
            {
                Debug.LogError("No PlayableDirector assigned to " + gameObject.name);
                return;
            }
        }

        SignalReceiver unityReceiver = director.GetComponent<SignalReceiver>();
        if (unityReceiver == null) return;

        for (int i = 0; i < events.Count; i++)
        {
            UnityEvent reaction = unityReceiver.GetReaction(events[i].signalAsset);
            if (reaction == null)
            {
                unityReceiver.AddReaction(events[i].signalAsset, new UnityEvent());
                reaction = unityReceiver.GetReaction(events[i].signalAsset);
            }
            reaction.AddListener(events[i].gameEvent.Occured);
        }
    }

    private void OnDisable()
    {
        SignalReceiver unityReceiver = director.GetComponent<SignalReceiver>();
        if (unityReceiver == null) return;

        for (int i = 0; i < events.Count; i++)
        {
            UnityEvent reaction = unityReceiver.GetReaction(events[i].signalAsset);
            if (reaction != null)
            {
                reaction.RemoveListener(events[i].gameEvent.Occured);
            }
        }
    }
}