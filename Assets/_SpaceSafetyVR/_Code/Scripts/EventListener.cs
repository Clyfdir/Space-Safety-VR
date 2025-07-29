
using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UnityEvent response;

    public void OnEventOccured()
    {
        response.Invoke();
    }
    private void OnEnable()
    {
        gameEvent.Register(this);
    }
    private void OnDisable()
    {
        gameEvent.Unregister(this);
    }
}
