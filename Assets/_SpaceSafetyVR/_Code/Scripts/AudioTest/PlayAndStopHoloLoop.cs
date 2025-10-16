using UnityEngine;

public class PlayAndStopHoloLoop : MonoBehaviour
{

    public string eventStartName;
    public string eventStopName;

    public void StartLoop()
    {
        AudioManager.CallEvent(eventStartName, this.gameObject);
    }

    public void StopLoop()
    {
        AudioManager.CallEvent(eventStopName, this.gameObject);
    }
    
    
}
