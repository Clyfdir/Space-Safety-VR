
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<EventListener> eventListeners = new();

    public void Register(EventListener listener)
    {
        eventListeners.Add(listener);
    }
    public void Unregister(EventListener listener)
    {
        eventListeners.Remove(listener);
    }

    public void Occured()
    {
        foreach (EventListener listener in eventListeners)
        {
            listener.OnEventOccured();
        }
    }
}
