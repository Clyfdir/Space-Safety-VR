///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   Udemy course was used ,for logic: https://www.udemy.com/course/design-patterns-for-game-programming/ , lessons 34-35
///   Created: 08.06.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 28.08.2025

///   Spawning debris which is orbiting around Earth (orbiting - it's another script, attached to each prefab)

using UnityEngine;
using System.Collections;

public class SpawnDebrisFromPool : MonoBehaviour
{
    public static SpawnDebrisFromPool Instance;

    public enum Mode
    {
        Mode01CleanSpace,
        Mode02,
        Mode03,
        Mode04CleanAllAtOnce,
        Custom
    }

    //DebrisMediumSolarPanel
    //DebrisMediumYellow

    [Header("Mode Settings:")]
    public Mode currentMode = Mode.Mode01CleanSpace;
    private Mode previousMode = Mode.Custom;  // initialize to an impossible/default sentinel

    [Header("Current intervals:")]
    [SerializeField] private float debrisIntervalLarge;
    [SerializeField] private float debrisIntervalMedium;
    [SerializeField] private float debrisIntervalMediumPanel;
    [SerializeField] private float debrisIntervalMediumYellow;
    [SerializeField] private float debrisIntervalSmall;

    // Spawn intervals in seconds
    [Header("Mode02 intervals:")]
    [SerializeField] private float debrisIntervalLarge2 = 1f;
    [SerializeField] private float debrisIntervalMedium2 = 1f;
    [SerializeField] private float debrisIntervalMediumPanel2 = 1f;
    [SerializeField] private float debrisIntervalMediumYellow2 = 1f;
    [SerializeField] private float debrisIntervalSmall2 = 1f;

    [Header("Mode03 intervals:")]
    [SerializeField] private float debrisIntervalLarge3 = 0.2f;
    [SerializeField] private float debrisIntervalMedium3 = 0.2f;
    [SerializeField] private float debrisIntervalMediumPanel3 = 0.2f;
    [SerializeField] private float debrisIntervalMediumYellow3 = 0.2f;
    [SerializeField] private float debrisIntervalSmall3 = 0.2f;

    // Internal timers
    [Header("Timers:")]
    [SerializeField] private float debrisTimerLarge = 0f;
    [SerializeField] private float debrisTimerMedium = 0f;
    [SerializeField] private float debrisTimerMediumPanel = 0f;
    [SerializeField] private float debrisTimerMediumYellow = 0f;
    [SerializeField] private float debrisTimerSmall = 0f;

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
    private Transform[] spawnPoints;

    void Awake()
    {
        Instance = this;

        // Get all child transforms (your spawn points)
        spawnPoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPoints[i] = transform.GetChild(i);
        }

        // Check if we have at least 2 spawn points
        if (spawnPoints.Length < 2)
        {
            Debug.LogWarning("SpawnDebrisFromPool needs at least 2 child GameObjects as spawn points!");
        }
    }

    void Start()
    {
        // make sure the very first mode gets set up
        previousMode = currentMode;
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
        HandleSpawnDebpis();
    }

    public void SetupModeObjects()
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
            case Mode.Custom:
                break;
        }
    }

    public void SetupModeIntervals()
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
            case Mode.Custom:
                //
                break;
        }
    }

    public void HandleSpawnDebpis()
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
                break;
            case Mode.Mode03:
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
                break;
            case Mode.Mode04CleanAllAtOnce:
                break;
            case Mode.Custom:
                // Fall back to original behavior
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    DebrisSpawnFromPool("DebrisMedium");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    DebrisSpawnFromPool("DebrisMediumSolarPanel");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    DebrisSpawnFromPool("DebrisMediumYellow");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                break;
        }
    }

    public void DebrisSpawnFromPool(string tag)
    {
        GameObject a = Pool.Instance.Get(tag);
        if (a != null)
        {
            // Select a random spawn point if available
            Transform selectedSpawnPoint = transform; // default to parent if no children
            if (spawnPoints.Length > 0)
            {
                selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

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

            // Configure movement direction based on spawn position
            //ConfigureDebrisMovement(a, spawnPosition);
            ConfigureDebrisMovementWhenOrbiting(a, spawnPosition);

            a.SetActive(true);
        }
    }

    private void ConfigureDebrisMovementWhenOrbiting(GameObject debris, Vector3 spawnPosition)
    {
        var orb = debris.GetComponent<OrbitAroundSphere>();
        if (orb == null) orb = debris.AddComponent<OrbitAroundSphere>();

        // left of spawner  CCW (-1), right  CW (1)
        bool isLeft = spawnPosition.x < transform.position.x;
        orb.directionDebrisOrbiting = isLeft ? -1 : 1;
    }

    public void DeactivateObjectsWithTag(string tag)
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

    public void DeactivateAllDebris()
    {
        DeactivateObjectsWithTag("DebrisSmall");
        DeactivateObjectsWithTag("DebrisMedium");
        DeactivateObjectsWithTag("DebrisMediumSolarPanel");
        DeactivateObjectsWithTag("DebrisMediumYellow");
        DeactivateObjectsWithTag("DebrisLarge");
    }
}
