using UnityEngine;

public class SignalReceiver : MonoBehaviour
{
    [SerializeField] private GameEvent shuttleApproachFinish;
    [SerializeField] private GameEvent shuttleDockFinish;
    [SerializeField] private GameEvent shuttleDebrisCollision;
    [SerializeField] private GameEvent shuttleUndockFinish;
    [SerializeField] private GameEvent sunFlashStart;

    public void ShuttleApproachFinish()
    {
        shuttleApproachFinish.Occured();
    }
    public void ShuttleDebrisCollision()
    {
        shuttleDebrisCollision.Occured();
    }
    public void ShuttleDockFinish()
    {
        shuttleDockFinish.Occured();
    }
    public void ShuttleUndockFinish()
    {
        shuttleUndockFinish.Occured();
    }
    public void SunFlashStart()
    {
        sunFlashStart.Occured();
    }
}
