///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 11.06.2025
///   Last Change: 22.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 12.08.2025

///   to start debris/net secuence, call "DebrisModeController.Instance.SetModeWhenNet3();"

using UnityEngine;

public class DebrisModeController : MonoBehaviour
{
    public static DebrisModeController Instance;

    void Awake()
    {
        Instance = this;
    }

        //Mariia's comments added 12-07-2025
        private void Start()
    {
        // PWEventsManager.Instance?.StartSpawnDebris.AddListener(SetModeCleanSpace1);
        /*
        PWEventsManager.Instance?.StartSpawnDebris.AddListener(SetModeCleanSpace1);
        PWEventsManager.Instance?.RocketActivation.AddListener(SetModeWhenRocketBody2);
        PWEventsManager.Instance?.NetDebrisActivated.AddListener(SetModeWhenNet3);
        PWEventsManager.Instance?.DebrisAfterNetActivated.AddListener(SetModeAfterNet4);
        PWEventsManager.Instance?.DebrisBeforeCatastropheActivated.AddListener(SetModeBeforeCatastrophe5);
        PWEventsManager.Instance?.UpdateScene.AddListener(SetModeEndText6);
        */

    }

    private void OnDisable()
    {
        // PWEventsManager.Instance?.StartSpawnDebris.AddListener(SetModeCleanSpace1);
        /*
        PWEventsManager.Instance?.StartSpawnDebris.RemoveListener(SetModeCleanSpace1);
        PWEventsManager.Instance?.RocketActivation.RemoveListener(SetModeWhenRocketBody2);
        PWEventsManager.Instance?.NetDebrisActivated.RemoveListener(SetModeWhenNet3);
        PWEventsManager.Instance?.DebrisAfterNetActivated.RemoveListener(SetModeAfterNet4);
        PWEventsManager.Instance?.DebrisBeforeCatastropheActivated.AddListener(SetModeBeforeCatastrophe5);
        PWEventsManager.Instance?.UpdateScene.RemoveListener(SetModeEndText6);
        */

    }
    //
    public void SetModeCleanSpace1()
    {
        SetMode(SpawnDebrisFromPool.Mode.CleanSpace1);
        Debug.Log("CleanSpacemode activated");
    }

    public void SetModeWhenRocketBody2()
    {
        SetMode(SpawnDebrisFromPool.Mode.WhenRocketBody2);
        Debug.Log("WhenRocketBody activated");
    }

    public void SetModeAfterRocketBody()// new mode, slightly more debris after rocket body and before net
    {
        SetMode(SpawnDebrisFromPool.Mode.AfterRocketBody);
    }

    public void SetModeWhenNet3()
    {
        SetMode(SpawnDebrisFromPool.Mode.WhenNet3);
        Debug.Log("WhenNet activated");
    }

    public void SetModeAfterNet4()
    {
        SetMode(SpawnDebrisFromPool.Mode.AfterNet4);
        Debug.Log("WhenAfterNet activated");
    }

    public void SetModeBeforeCatastrophe5()
    {
        SetMode(SpawnDebrisFromPool.Mode.BeforeCatastrophe5);
        Debug.Log("WhenBeforecatastrophe activated");
    }

    public void SetModeEndText6()
    {
        SetMode(SpawnDebrisFromPool.Mode.EndText6);
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
        Debug.Log("Mode set to: " + mode);
    }
}
