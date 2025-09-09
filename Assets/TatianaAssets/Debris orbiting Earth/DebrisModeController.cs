///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   AI was used: GPT (free)
///   Created: 11.06.2025
///   Last Change: 22.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 03.09.2025

///   This script controls modes of debris (from #SpawnDebrisFromPool)

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
        Invoke("SetMode02", 1);
    }

    public void SetMode01CleanSpace()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode01CleanSpace);
    }

    public void SetMode02()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode02);
    }

    public void SetMode03()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode03);
        Invoke("SetMode02", 35);//temporal solution, this function "SetMode02()" should be called from timeline
        Invoke("SetMode01CleanSpace", 50);//temporal solution, this function "SetMode01CleanSpace()" should be called from timeline
    }
    
    public void SetMode04CleanAllAtOnce()
    {
        SetMode(SpawnDebrisFromPool.Mode.Mode04CleanAllAtOnce);
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
