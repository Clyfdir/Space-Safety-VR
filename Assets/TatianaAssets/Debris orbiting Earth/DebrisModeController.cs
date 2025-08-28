///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 11.06.2025
///   Last Change: 22.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 28.08.2025

///   to start debris/net secuence, call "DebrisModeController.Instance.SetModeWhenNet3();"

using UnityEngine;

public class DebrisModeController : MonoBehaviour
{
    public static DebrisModeController Instance;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetMode01CleanSpace();
        //SetMode02();
        //SetMode03();
        Invoke("SetMode02", 2);
        Invoke("SetMode03", 5);
    }

    public void SetMode01CleanSpace()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode01CleanSpace);
        //Debug.Log("CleanSpacemode activated");
    }

    public void SetMode02()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode02);
        //Debug.Log("WhenRocketBody activated");
    }

    public void SetMode03()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode03);
        //Debug.Log("WhenNet activated");
    }
    //Mode04CleanAllAtOnce
    public void SetMode04CleanAllAtOnce()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode04CleanAllAtOnce);
        //Debug.Log("WhenNet activated");
    }

    public void SetModeCustom()
    {
        SetMode(SpawnDebrisFromPool.Mode.Custom);
    }

    private void SetMode(SpawnDebrisFromPool.Mode mode)
    {
        if (SpawnDebrisFromPool.Instance == null)
        {
            Debug.LogWarning("SpawnDebrisFromPool.Instance is null!");
            return;
        }

        SpawnDebrisFromPool.Instance.currentMode = mode;
        //Debug.Log("Mode set to: " + mode);
    }
}
