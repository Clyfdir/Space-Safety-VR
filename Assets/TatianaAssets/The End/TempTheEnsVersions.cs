///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   AI was used: GPT
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025
///   Created: 08.09.2025

/// Temporal script just to test different ends of the experience, how they look like

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TempTheEndVersions : MonoBehaviour
{
    [Header("Spawn Point Objects")]
    [SerializeField] private GameObject monitorMissionComplete;
    [SerializeField] private GameObject bigUIAtTheEnd;
    [SerializeField] private GameObject window;
    [SerializeField] private GameObject sphereAroundCamera;
    [SerializeField] private GameObject canvasWithoutShutters;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isMonitorMissionComplete = false;
    private bool isBigUIAtTheEnd = false;
    private bool isSphereAroundCamera = false;

    private bool isEndTriggered = false;
    private float delay = 6;

    private void Start()
    {
        MonitorMissionComplete();
    }

    // Call directly from Button
    public void MonitorMissionComplete()
    {
        
        SetStatus("1) Shutters Closed + Monitor");
        isMonitorMissionComplete = true;
        isBigUIAtTheEnd = false;
        isSphereAroundCamera = false;
    }

    public void BigUItheEnd()
    {
        
        SetStatus("2) Shutters Closed + UI");
        isMonitorMissionComplete = false;
        isBigUIAtTheEnd = true;
        isSphereAroundCamera = false;
    }

    public void SphereAroundCamera()
    {

        SetStatus("3) UI + gradual dark");
        isMonitorMissionComplete = false;
        isBigUIAtTheEnd = false;
        isSphereAroundCamera = true;
    }

    // Toggle function for Button
    public void SwitchEndVersions()
    {
        if (isMonitorMissionComplete)
        {
            BigUItheEnd();
        }
        else if (isBigUIAtTheEnd)
        {
            SphereAroundCamera();
        }
        else if (isSphereAroundCamera)
        {
            MonitorMissionComplete();
        }
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
        else Debug.Log(msg);
    }

    public void TriggerTheEnd()
    {
        if (isEndTriggered)
        {
            window.SetActive(false);
            isEndTriggered = false;
            monitorMissionComplete.SetActive(false);
            bigUIAtTheEnd.SetActive(false);
            sphereAroundCamera.SetActive(false);
        }
        else
        {
            if (isMonitorMissionComplete)
            {
                window.SetActive(true);
                StartCoroutine(ActivateWithDelay(monitorMissionComplete, delay));
            }
            else if (isBigUIAtTheEnd)
            {
                window.SetActive(true);
                StartCoroutine(ActivateWithDelay(bigUIAtTheEnd, delay));
            }
            else if (isSphereAroundCamera)
            {
                sphereAroundCamera.SetActive(true);
                canvasWithoutShutters.SetActive(true);
            }
            
            isEndTriggered = true;
        }
    }

    private IEnumerator ActivateWithDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);
    }
}

