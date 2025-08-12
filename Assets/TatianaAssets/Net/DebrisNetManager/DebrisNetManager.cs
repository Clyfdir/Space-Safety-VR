///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   also AI was used: GPT
///   Created: 06.07.2025
///   Last Change: 12.07.2025
///   ESA project stage: 
///   Last Change: 11.08.2025

///   Manages all in the 2nd event (net catches debris) after debris was caught

using System.Collections;
using UnityEngine;

public class DebrisNetManager : MonoBehaviour
{
    public static DebrisNetManager Instance { get; private set; }

    [Header("Timing & Speed")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float delayMoveBack = 1.1f;

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject net;
    [SerializeField] private GameObject debris;
    [SerializeField] private Transform debrisEndPos;
    [SerializeField] private Rope rope;
    [SerializeField] private SimulateCloth simulateCloth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        AutoAssignObjects();
    }

    public void AttachNetAndMove()
    {
        rope.StraightenRope();
        StartCoroutine(LaunchAfterDelay());
    }

    private IEnumerator LaunchAfterDelay()
    {
        // initial wait
        yield return new WaitForSeconds(delayMoveBack);

        // parent the net under the debris (preserves world pos)
        net.transform.SetParent(debris.transform, worldPositionStays: true);

        // wait one frame
        yield return null;

        // now move _towards_ netEndPos along debris's own forward
        Vector3 target = debrisEndPos.position;

        // keep going until we're basically there
        while (Vector3.Distance(debris.transform.position, target) > 0.01f)
        {
            // move by small step, clamped
            float step = speed * Time.deltaTime;
            debris.transform.position = Vector3.MoveTowards(
                debris.transform.position,
                target,
                step
            );

            yield return null;
        }


        // done! hide the net
        net.SetActive(false);
        yield return null;
        debris.SetActive(false);
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
        // Auto-assign debris
        if (debris == null)
        {
            debris = SceneUtils.FindDeep("DebrisToCatchByNet");
            if (debris == null)
                Debug.LogError("Couldn't find and assign 'DebrisToCatchByNet'.");
        }
        // Auto-assign debrisEndPos
        if (debrisEndPos == null)
        {
            GameObject go = SceneUtils.FindDeep("debrisEndPos");
            debrisEndPos = go.transform;
            if (debrisEndPos == null)
                Debug.LogError("Couldn't find and assign 'debrisEndPos'.");
        }

        // Auto-assign rope
        if (rope == null)
        {
            GameObject go = SceneUtils.FindDeep("RopeLineRenderer");
            rope = go.GetComponent<Rope>();
            if (debrisEndPos == null)
                Debug.LogError("Couldn't find and assign 'RopeLineRenderer'.");
        }

        // Auto-assign SimulateCloth
        if (simulateCloth == null)
        {
            GameObject go = SceneUtils.FindDeep("NetCloth");
            simulateCloth = go.GetComponent<SimulateCloth>();
            if (simulateCloth == null)
                Debug.LogError("Couldn't find and assign 'SimulateCloth'.");
        }
    }

    public void Net5Iterations()
    {
        simulateCloth.Iterate5timesConstraints();
    }

    public void Net10Iterations()
    {
        simulateCloth.Iterate10timesConstraints();
    }

    public void Net20Iterations()
    {
        simulateCloth.Iterate20timesConstraints();
    }
}

