using UnityEngine;

public class SignalReceiver : MonoBehaviour
{
    [SerializeField] private GameEvent shuttleApproachFinish;
    [SerializeField] private GameEvent shuttleDockFinish;
    [SerializeField] private GameEvent shuttleUndockFinish;

    public void ShuttleApproachFinish()
    {
        shuttleApproachFinish.Occured();
    }
    public void ShuttleDockFinish()
    {
        shuttleDockFinish.Occured();
    }
    public void ShuttleUndockFinish()
    {
        shuttleUndockFinish.Occured();
    }
}
