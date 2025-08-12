///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT
///   Created: 15.06.2025
///   Last Change: 11.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 12.08.2025

///   Manages differen stages of the 2nd event (net catches debris)

using UnityEngine;

public class DebrisTriggersImproved : MonoBehaviour
{

    [SerializeField] private string colliderTagToWarnUser = "ColliderToTriggerUserWarning"; 
    [SerializeField] private string colliderTagToAutoActivateNet = "debrisNetActivationCollider";
    [SerializeField] private string netTagToNotifyThatDebrisIsCatched = "NetParent";

    [SerializeField] private bool netMovedAutomatically = false;
    [SerializeField] private bool debrisCaughtAutomatically = false;
    [SerializeField] private bool netMovedByUser = false;
    [SerializeField] private bool userWarned = false;

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject net;

    void Awake()
    {
        AutoAssignObjects();
    }

    private void OnTriggerEnter(Collider other)
    {

        // When uses is warned to press button
        if (other.CompareTag(colliderTagToWarnUser) && !userWarned && other.transform.root != other.transform)
        {
            Debug.Log($"Debris is close!!! Preeeeees the button!!!.");
            //PWEventsManager.Instance?.OnNetDebrisWasActivated();
            net.SetActive(true);
            userWarned = true;
        }

        //If user doesn't press button, Net will move automatically
        if (other.CompareTag(colliderTagToAutoActivateNet) && NetLauncher.Instance.hasLaunched == false && other.transform.root != other.transform)
        {
            //DebrisModeController.Instance.SetModeCleanSpace1();
            netMovedAutomatically = true;
            NetLauncher.Instance.LaunchNet();

            //PWEventsManager.Instance?.OnNetWorked();
            Debug.Log($"Net was automatically moved.");
        }

        //Here automatic system catches debris by Net
        if (other.CompareTag(netTagToNotifyThatDebrisIsCatched) && netMovedAutomatically && !debrisCaughtAutomatically && other.transform.root != other.transform)
        {
            //PWEventsManager.Instance?.OnNeAutoWorked();
            Debug.Log($"Debris was automatically caught.");
            debrisCaughtAutomatically = true;
            //DebrisNetManager.Instance.AttachNetAndMove();
            DebrisModeController.Instance.SetModeCleanSpace1();
        }

        //Here user catches debris by Net
        if (other.CompareTag(netTagToNotifyThatDebrisIsCatched) && !netMovedAutomatically && !netMovedByUser && other.transform.root != other.transform)
        {
            //PWEventsManager.Instance?.OnNetWorked();
            Debug.Log($"Well Done!.");
            netMovedByUser = true;
            //DebrisNetManager.Instance.AttachNetAndMove();
        }
    }

    private void AutoAssignObjects()
    {
        // Auto-assign net
        if (net == null)
        {
            net = SceneUtils.FindDeep("NetLauncher");
            if (net == null)
                Debug.LogError("Couldn't find and assign 'NetLauncher'.");
        }
    }

    private void OnDisable()
    {
        netMovedAutomatically = false;
        debrisCaughtAutomatically = false;
        netMovedByUser = false;
        userWarned = false;
    }
}

