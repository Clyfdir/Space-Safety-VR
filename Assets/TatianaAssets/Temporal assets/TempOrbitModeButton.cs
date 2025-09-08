///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025
///   Created: 08.09.2025

/// Temporal script just to test different orbits, how they look like

using UnityEngine;
using TMPro;

public class TempOrbitModeButton : MonoBehaviour
{
    [Header("Spawn Point Objects")]
    [SerializeField] private GameObject LeftSpawnPos;
    [SerializeField] private GameObject RightSpawnPos;
    [SerializeField] private GameObject topSpawnPos;
    [SerializeField] private GameObject bottomSpawnPos;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isHorizontal = false; // start mode

    // Call directly from Button
    public void HorizontalOrbits()
    {
        SetActiveAll(left: true, right: true, top: false, bottom: false);
        SetStatus("Horizontal Orbits");
        isHorizontal = true;
    }

    public void MultipleOrbits()
    {
        SetActiveAll(left: true, right: true, top: true, bottom: true);
        SetStatus("Multiple Orbits");
        isHorizontal = false;
    }

    // Toggle function for Button
    public void SwitchOrbits()
    {
        if (isHorizontal)
        {
            MultipleOrbits();
        }
        else
        {
            HorizontalOrbits();
        }
        SpawnDebrisFromPool.Instance.UpdateSpawnPoints();
    }

    private void SetActiveAll(bool left, bool right, bool top, bool bottom)
    {
        if (LeftSpawnPos) LeftSpawnPos.SetActive(left);
        if (RightSpawnPos) RightSpawnPos.SetActive(right);
        if (topSpawnPos) topSpawnPos.SetActive(top);
        if (bottomSpawnPos) bottomSpawnPos.SetActive(bottom);
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
        else Debug.Log(msg);
    }
}

