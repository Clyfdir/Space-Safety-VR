///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 10.09.2025
///   Created: 10.09.2025

/// Script Controls, which version of The End will be used

using UnityEngine;
using System.Collections.Generic;

public class EndVersionsManager : MonoBehaviour
{
    public static EndVersionsManager Instance { get; private set; }

    public enum Version
    {
        Version1 = 1,
        Version2 = 2,
        Version3 = 3,
        Version4 = 4,
        Version5 = 5
    }

    [Header("Version Settings")]
    public Version currentEndVersion = Version.Version3; // default version; and this makes the enum visible in Inspector

    [Header("Children (auto-filled)")]
    [SerializeField] private List<GameObject> children = new List<GameObject>();

    [Header("End Text")]
    [TextArea]
    [SerializeField] private string endText = "Default end text here.";

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Fill children list automatically
        children.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        // Activate the initial version
        //ActivateVersion(currentEndVersion);
    }

    public void TriggerTheEnd()
    {
        ActivateVersion(currentEndVersion);
    }

    public void ActivateVersion(Version version)
    {
        // Disable all children first
        foreach (var child in children)
        {
            child.SetActive(false);
        }

        // Switch-case to enable the correct one
        switch (version)
        {
            case Version.Version1:
                ActivateChild(0);
                break;
            case Version.Version2:
                ActivateChild(1);
                break;
            case Version.Version3:
                ActivateChild(2);
                break;
            case Version.Version4:
                ActivateChild(3);
                break;
            case Version.Version5:
                ActivateChild(4);
                break;
            default:
                Debug.LogWarning("Unknown version: " + version);
                break;
        }
    }

    private void ActivateChild(int index)
    {
        if (index >= 0 && index < children.Count)
        {
            children[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning("Child index out of range: " + index);
        }
    }

    public string GetEndText()
    {
        return endText;
    }
}
