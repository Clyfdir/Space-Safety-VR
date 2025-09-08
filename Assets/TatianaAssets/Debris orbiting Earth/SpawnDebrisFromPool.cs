///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   Udemy course was used ,for logic: https://www.udemy.com/course/design-patterns-for-game-programming/ , lessons 34-35
///   Created: 08.06.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 08.09.2025

///   Spawning debris which is orbiting around Earth (orbiting - it's another script, attached to each prefab)

using UnityEngine;
using System.Collections;
using System.Linq;

public class SpawnDebrisFromPool : MonoBehaviour
{
    public static SpawnDebrisFromPool Instance;

    public enum Mode
    {
        Mode01CleanSpace,
        Mode02,
        Mode03,
        Mode04CleanAllAtOnce
    }

    [Header("Mode Settings:")]
    public Mode currentMode = Mode.Mode01CleanSpace;
    private Mode previousMode = Mode.Mode03;  // initialize to an impossible/default sentinel

    [Header("Current intervals:")]
    [SerializeField] private float debrisIntervalLarge;
    [SerializeField] private float debrisIntervalMedium;
    [SerializeField] private float debrisIntervalMediumPanel;
    [SerializeField] private float debrisIntervalMediumYellow;
    [SerializeField] private float debrisIntervalSmall;

    //Mode02 spawn intervals in seconds:
    private float debrisIntervalLarge2 = 4f;
    private float debrisIntervalMedium2 = 3f;
    private float debrisIntervalMediumPanel2 = 3f;
    private float debrisIntervalMediumYellow2 = 3f;
    private float debrisIntervalSmall2 = 2f;

    //Mode03 spawn intervals in seconds:
    private float debrisIntervalLarge3 = 0.6f;
    private float debrisIntervalMedium3 = 0.5f;
    private float debrisIntervalMediumPanel3 = 0.5f;
    private float debrisIntervalMediumYellow3 = 0.5f;
    private float debrisIntervalSmall3 = 0.03f;

    // Internal timers
    private float debrisTimerLarge = 0f;
    private float debrisTimerMedium = 0f;
    private float debrisTimerMediumPanel = 0f;
    private float debrisTimerMediumYellow = 0f;
    private float debrisTimerSmall = 0f;

    [Header("Spawn position ranges:")]
    [SerializeField] private float xPosMin = -5f;
    [SerializeField] private float xPosMax = 0f;
    [SerializeField] private float yPosMin = -10f;
    [SerializeField] private float yPosMax = 10f;
    [SerializeField] private float zPosMin = 0f;
    [SerializeField] private float zPosMax = 5f;

    [Header("Spawn rotation range:")]
    [SerializeField] private float zRotMin = -15f;
    [SerializeField] private float zRotMax = 15f;

    // Array to hold the spawn point transforms
    [SerializeField] private Transform[] spawnPoints;

    void Awake()
    {
        Instance = this;

        UpdateSpawnPoints();
    }

    void Start()
    {
        // make sure the very first mode gets set up
        previousMode = currentMode;
        SetupModeIntervals();
        SetupModeObjects();
    }

    void Update()
    {
        if (currentMode != previousMode)
        {
            SetupModeObjects();
            SetupModeIntervals();
            previousMode = currentMode;
        }
        HandleSpawnDebis();
    }

    private void SetupModeObjects()
    {
        switch (currentMode)
        {
            case Mode.Mode01CleanSpace:
                break;
            case Mode.Mode02:
                break;
            case Mode.Mode03:
                break;
            case Mode.Mode04CleanAllAtOnce:
                DeactivateAllDebris();
                break;
        }
    }

    private void SetupModeIntervals()
    {
        switch (currentMode)
        {
            case Mode.Mode01CleanSpace:
                break;
            case Mode.Mode02:
                debrisIntervalLarge = debrisIntervalLarge2;
                debrisIntervalMedium = debrisIntervalMedium2;
                debrisIntervalSmall = debrisIntervalSmall2;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel2;
                debrisIntervalMediumYellow = debrisIntervalMediumYellow2;
                break;
            case Mode.Mode03:
                debrisIntervalLarge = debrisIntervalLarge3;
                debrisIntervalMedium = debrisIntervalMedium3;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel3;
                debrisIntervalSmall = debrisIntervalSmall3;
                debrisIntervalMediumYellow = debrisIntervalMediumYellow3;
                break;
            case Mode.Mode04CleanAllAtOnce:
                break;
        }
    }

    private void HandleSpawnDebis()
    {
        float deltaTime = Time.deltaTime;

        debrisTimerSmall += deltaTime;
        debrisTimerMedium += deltaTime;
        debrisTimerLarge += deltaTime;
        debrisTimerMediumPanel += deltaTime; 
        debrisTimerMediumYellow += deltaTime;

        switch (currentMode)
        {
            case Mode.Mode01CleanSpace:
                break;
            case Mode.Mode02:
                SpawnDebris();
                break;
            case Mode.Mode03:
                SpawnDebris();
                break;
            case Mode.Mode04CleanAllAtOnce:
                break;
        }
    }
    private void DebrisSpawnFromPool(string tag)
    {
        GameObject a = Pool.Instance.Get(tag);
        if (a != null)
        {
            // Select a random spawn point if available
            Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Calculate spawn position with random offset
            Vector3 spawnPosition = selectedSpawnPoint.position +
                                   new Vector3(Random.Range(xPosMin, xPosMax),
                                               Random.Range(yPosMin, yPosMax),
                                               Random.Range(zPosMin, zPosMax));

            a.transform.position = spawnPosition;

            // Apply random Z rotation (-90 to 90 degrees) while keeping other rotations
            Quaternion randomRotation = selectedSpawnPoint.rotation *
                                      Quaternion.Euler(0, 0, Random.Range(zRotMin, zRotMax));
            a.transform.rotation = randomRotation;

            a.SetActive(true);
        }
    }

    private void DeactivateObjectsWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
        }
    }
    private void SpawnDebris()
    {
        if (debrisTimerLarge >= debrisIntervalLarge)
        {
            DebrisSpawnFromPool("DebrisLarge");
            debrisTimerLarge = 0f;
        }
        if (debrisTimerMedium >= debrisIntervalMedium)
        {
            DebrisSpawnFromPool("DebrisMedium");
            debrisTimerMedium = 0f;
        }
        if (debrisTimerMediumPanel >= debrisIntervalMediumPanel)
        {
            DebrisSpawnFromPool("DebrisMediumSolarPanel");
            debrisTimerMediumPanel = 0f;
        }
        if (debrisTimerMediumYellow >= debrisIntervalMediumYellow)
        {
            DebrisSpawnFromPool("DebrisMediumYellow");
            debrisTimerMediumYellow = 0f;
        }
        if (debrisTimerSmall >= debrisIntervalSmall)
        {
            DebrisSpawnFromPool("DebrisSmall");
            debrisTimerSmall = 0f;
        }
    }

    private void DeactivateAllDebris()
    {
        DeactivateObjectsWithTag("DebrisSmall");
        DeactivateObjectsWithTag("DebrisMedium");
        DeactivateObjectsWithTag("DebrisMediumSolarPanel");
        DeactivateObjectsWithTag("DebrisMediumYellow");
        DeactivateObjectsWithTag("DebrisLarge");
    }

    public void UpdateSpawnPoints()
    {
        // Get all child transforms (your spawn points)
        spawnPoints = new Transform[transform.childCount];
        spawnPoints = transform
            .Cast<Transform>()                            // direct children only
            .Where(t => t.gameObject.activeInHierarchy)   // or .activeSelf if you prefer
            .ToArray();

        // Check if we have at least 2 spawn points
        if (spawnPoints.Length < 2)
        {
            Debug.LogWarning("SpawnDebrisFromPool needs at least 2 child GameObjects as spawn points!");
        }
    }
}
