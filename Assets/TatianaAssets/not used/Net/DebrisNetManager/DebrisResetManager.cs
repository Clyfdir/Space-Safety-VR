///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT (free)
///   Created: 15.06.2025
///   Last Change: 15.06.2025
///   ESA PROJECT STAGE:
///   Last Change: 11.08.2025

using System.Collections.Generic;
using UnityEngine;

public class DebrisResetManager : MonoBehaviour
{
    public static DebrisResetManager Instance { get; private set; }

    private readonly List<DebrisResetter> resetQueue = new List<DebrisResetter>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void QueueReset(DebrisResetter resetter)
    {
        resetQueue.Add(resetter);
    }

    private void LateUpdate()
    {
        foreach (var resetter in resetQueue)
        {
            resetter.PerformReset();
        }

        resetQueue.Clear();
    }
}
