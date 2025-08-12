///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space
///   Udemy course was used ,for logic: https://www.udemy.com/course/design-patterns-for-game-programming/ , lessons 34-35
///   Created: 08.06.2025
///   Last Change: 12.07.2025
///   ESA PROJECT STAGE:
///   Last Change: 12.08.2025

///   Spawning debris which is orbiting around Earth (orbiting - it's another script, attached to each prefab)

using UnityEngine;
using System.Collections;

public class SpawnDebrisFromPool : MonoBehaviour
{
    public static SpawnDebrisFromPool Instance;

    public enum Mode
    {
        CleanSpace1,
        WhenRocketBody2,
        AfterRocketBody,
        WhenNet3,
        AfterNet4,
        BeforeCatastrophe5,
        EndText6,
        Custom
    }

    //DebrisMediumSolarPanel
    //DebrisMediumYellow

    [SerializeField] private GameObject DebrisDeactivatorCollider;

    [Header("Mode Settings:")]
    public Mode currentMode = Mode.CleanSpace1;
    private Mode previousMode = Mode.Custom;  // initialize to an impossible/default sentinel

    [Header("Current intervals:")]
    [SerializeField] private float debrisIntervalLarge;
    [SerializeField] private float debrisIntervalMedium;
    [SerializeField] private float debrisIntervalMediumPanel;
    [SerializeField] private float debrisIntervalMediumYellow;
    [SerializeField] private float debrisIntervalSmall;
    [SerializeField] private float debrisIntervalCloud;

    // Spawn intervals in seconds
    [Header("WhenRocketBody2 intervals:")]
    [SerializeField] private float debrisIntervalLarge2 = 6f;
    [SerializeField] private float debrisIntervalMedium2 = 5f;
    [SerializeField] private float debrisIntervalSmall2 = 4f;
    public float debrisIntervalCloud2 = 5f;

    [Header("AfterRocketBody intervals:")]
    [SerializeField] private float debrisIntervalLarge2_1 = 5f;
    [SerializeField] private float debrisIntervalMedium2_1 = 5f;
    [SerializeField] private float debrisIntervalMediumPanel2_1 = 5f;
    [SerializeField] private float debrisIntervalSmall2_1 = 3f;
    [SerializeField] private float debrisIntervalCloud2_1 = 6f;

    [Header("WhenNet3 intervals:")]
    [SerializeField] private float debrisIntervalLarge3 = 4f;
    [SerializeField] private float debrisIntervalMedium3 = 4f;
    [SerializeField] private float debrisIntervalMediumPanel3 = 4f;
    [SerializeField] private float debrisIntervalSmall3 = 2f;
    [SerializeField] private float debrisIntervalCloud3 = 5f;

    [Header("AfterNet4 intervals:")]
    [SerializeField] private float debrisIntervalLarge4 = 2f;
    [SerializeField] private float debrisIntervalMedium4 = 1.5f;
    [SerializeField] private float debrisIntervalMediumPanel4 = 1.5f;
    [SerializeField] private float debrisIntervalMediumYellow4 = 1.5f;
    [SerializeField] private float debrisIntervalSmall4 = 0.5f;
    [SerializeField] private float debrisIntervalCloud4 = 2.5f;

    [Header("BeforeCatastrophe5 intervals:")]
    [SerializeField] private float debrisIntervalLarge5 = 1f;
    [SerializeField] private float debrisIntervalMedium5 = 0.3f;
    [SerializeField] private float debrisIntervalMediumPanel5 = 0.3f;
    [SerializeField] private float debrisIntervalMediumYellow5 = 0.3f;
    [SerializeField] private float debrisIntervalSmall5 = 0.1f;
    [SerializeField] private float debrisIntervalCloud5 = 0.5f;

    [Header("Bool conditions to spawn:")]
    [SerializeField] private bool spawningDebrisLarge = true;
    [SerializeField] private bool spawningDebrisMedium = true;
    [SerializeField] private bool spawningDebrisSmall = true;
    [SerializeField] private bool spawningDebrisCloud = true;

    // Internal timers
    [SerializeField] private float debrisTimerLarge = 0f;
    [SerializeField] private float debrisTimerMedium = 0f;
    [SerializeField] private float debrisTimerMediumPanel = 0f;
    [SerializeField] private float debrisTimerMediumYellow = 0f;
    [SerializeField] private float debrisTimerSmall = 0f;
    [SerializeField] private float debrisTimerCloud = 0f;

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

    [Header("Objects (will be auto assigned):")]
    [SerializeField] private GameObject Net;
    [SerializeField] private GameObject DebrisToCatchByNet;
    //[SerializeField] private GameObject EndTextPanel;

    // Movement speed for spawned objects
    //[Header("Movement Settings:")]
    //public float moveSpeed = 5f;

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

        AutoAssignObjects();
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
            case Mode.CleanSpace1:
                //DeactivateObj(DebrisToCatchByNet);
                //DeactivateObj(Net);
                //DeactivateObj(EndTextPanel);
                ActivateObj(DebrisDeactivatorCollider);
                break;
            case Mode.WhenRocketBody2:
                //DeactivateObj(DebrisToCatchByNet);
                //DeactivateObj(Net);
                //DeactivateObj(EndTextPanel);
                //
                break;
            case Mode.AfterRocketBody:
                DeactivateObj(DebrisToCatchByNet);
                DeactivateObj(Net);
                //DeactivateObj(EndTextPanel);
                //
                break;
            case Mode.WhenNet3:
                //DeactivateObj(EndTextPanel);
                ActivateObj(DebrisToCatchByNet);
                DeactivateObj(Net);
                DeactivateObj(DebrisDeactivatorCollider);
                //ActivateObj(Net);//no need here, it will be done when debris is closer to spaceship (when passed colliderToTriggerUserWarning)
                //
                break;
            case Mode.AfterNet4:
                DeactivateObj(DebrisToCatchByNet);
                DeactivateObj(Net);
                //DeactivateObj(EndTextPanel);
                //
                break;
            case Mode.BeforeCatastrophe5:
                DeactivateObj(DebrisToCatchByNet);
                DeactivateObj(Net);
                //DeactivateObj(EndTextPanel);
                //
                break;
            case Mode.EndText6:
                DeactivateObj(DebrisToCatchByNet);
                DeactivateObj(Net);
                DeactivateAllDebris();
                //ActivateObj(EndTextPanel);
                //
                break;
            case Mode.Custom:
                //
                break;
        }

    }

    public void SetupModeIntervals()
    {
        switch (currentMode)
        {
            case Mode.CleanSpace1:
                break;
            case Mode.WhenRocketBody2:
                debrisIntervalLarge = debrisIntervalLarge2;
                debrisIntervalMedium = debrisIntervalMedium2;
                debrisIntervalSmall = debrisIntervalSmall2;
                //debrisIntervalCloud = debrisIntervalCloud2;
                break;
            case Mode.AfterRocketBody:
                debrisIntervalLarge = debrisIntervalLarge2_1;
                debrisIntervalMedium = debrisIntervalMedium2_1;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel2_1;
                debrisIntervalSmall = debrisIntervalSmall2_1;
                debrisIntervalCloud = debrisIntervalCloud2_1;
                break;
            case Mode.WhenNet3:
                debrisIntervalLarge = debrisIntervalLarge3;
                debrisIntervalMedium = debrisIntervalMedium3;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel3;
                debrisIntervalSmall = debrisIntervalSmall3;
                debrisIntervalCloud = debrisIntervalCloud3;
                break;
            case Mode.AfterNet4:
                debrisIntervalLarge = debrisIntervalLarge4;
                debrisIntervalMedium = debrisIntervalMedium4;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel4;
                debrisIntervalMediumYellow = debrisIntervalMediumYellow4;
                debrisIntervalSmall = debrisIntervalSmall4;
                debrisIntervalCloud = debrisIntervalCloud4;
                break;
            case Mode.BeforeCatastrophe5:
                debrisIntervalLarge = debrisIntervalLarge5;
                debrisIntervalMedium = debrisIntervalMedium5;
                debrisIntervalMediumPanel = debrisIntervalMediumPanel5;
                debrisIntervalMediumYellow = debrisIntervalMediumYellow5;
                debrisIntervalSmall = debrisIntervalSmall5;
                debrisIntervalCloud = debrisIntervalCloud5;
                break;
            case Mode.EndText6:
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
        debrisTimerCloud += deltaTime;
        debrisTimerMediumPanel += deltaTime; 
        debrisTimerMediumYellow += deltaTime;

        switch (currentMode)
        {
            case Mode.CleanSpace1:
                break;
            case Mode.WhenRocketBody2:
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    //debrisIntervalLarge = debrisIntervalLarge2;
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    //debrisIntervalMedium = debrisIntervalMedium2;
                    DebrisSpawnFromPool("DebrisMedium");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    //debrisIntervalSmall = debrisIntervalSmall2;
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                break;
            case Mode.AfterRocketBody:
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    //debrisIntervalLarge = debrisIntervalLarge2_1;
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    //debrisIntervalMedium = debrisIntervalMedium2_1;
                    DebrisSpawnFromPool("DebrisMedium");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerMediumPanel >= debrisIntervalMediumPanel)
                {
                    DebrisSpawnFromPool("DebrisMediumSolarPanel");
                    debrisTimerMediumPanel = 0f;
                }
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    //debrisIntervalSmall = debrisIntervalSmall2_1;
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                /*
                if (debrisTimerCloud >= debrisIntervalCloud)
                {
                    //debrisIntervalCloud = debrisIntervalCloud2_1;
                    DebrisSpawnFromPool("DebrisCloud");
                    debrisTimerCloud = 0f;
                }
                */
                
                break;
            case Mode.WhenNet3:
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    //debrisIntervalLarge = debrisIntervalLarge3;
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    //debrisIntervalMedium = debrisIntervalMedium3;
                    DebrisSpawnFromPool("DebrisMedium");
                    debrisTimerMedium = 0f;
                }
                if (debrisTimerMediumPanel >= debrisIntervalMediumPanel)
                {
                    DebrisSpawnFromPool("DebrisMediumSolarPanel");
                    debrisTimerMediumPanel = 0f;
                }
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    //debrisIntervalSmall = debrisIntervalSmall3;
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                /*
                if (debrisTimerCloud >= debrisIntervalCloud)
                {
                    //debrisIntervalCloud = debrisIntervalCloud3;
                    DebrisSpawnFromPool("DebrisCloud");
                    debrisTimerCloud = 0f;
                }
                */
                
                break;
            case Mode.AfterNet4:
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    //debrisIntervalLarge = debrisIntervalLarge4;
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    //debrisIntervalMedium = debrisIntervalMedium4;
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
                    //debrisIntervalSmall = debrisIntervalSmall4;
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                /*
                if (debrisTimerCloud >= debrisIntervalCloud)
                {
                    //debrisIntervalCloud = debrisIntervalCloud4;
                    DebrisSpawnFromPool("DebrisCloud");
                    debrisTimerCloud = 0f;
                }
                */
                
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    //debrisIntervalSmall = debrisIntervalSmall4;
                    DebrisSpawnFromPool("DebrisSmall2");
                    debrisTimerSmall = 0f;
                }
                break;
            case Mode.BeforeCatastrophe5:
                if (debrisTimerLarge >= debrisIntervalLarge)
                {
                    //debrisIntervalLarge = debrisIntervalLarge5;
                    DebrisSpawnFromPool("DebrisLarge");
                    debrisTimerLarge = 0f;
                }
                if (debrisTimerMedium >= debrisIntervalMedium)
                {
                    //debrisIntervalMedium = debrisIntervalMedium5;
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
                    //debrisIntervalSmall = debrisIntervalSmall5;
                    DebrisSpawnFromPool("DebrisSmall");
                    debrisTimerSmall = 0f;
                }
                /*
                if (debrisTimerCloud >= debrisIntervalCloud)
                {
                    //debrisIntervalCloud = debrisIntervalCloud5;
                    DebrisSpawnFromPool("DebrisCloud");
                    debrisTimerCloud = 0f;
                }
                */
                
                if (debrisTimerSmall >= debrisIntervalSmall)
                {
                    //debrisIntervalSmall = debrisIntervalSmall5;
                    DebrisSpawnFromPool("DebrisSmall2");
                    debrisTimerSmall = 0f;
                }
                break;
            case Mode.EndText6:
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
                if (debrisTimerCloud >= debrisIntervalCloud)
                {
                    DebrisSpawnFromPool("DebrisCloud");
                    debrisTimerCloud = 0f;
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

    /*
    private void ConfigureDebrisMovement(GameObject debris, Vector3 spawnPosition)
    {
        // Get or add the RandomSpeedMover component
        RandomSpeedMover mover = debris.GetComponent<RandomSpeedMover>();
        if (mover == null)
        {
            mover = debris.AddComponent<RandomSpeedMover>();
        }

        // Determine if the debris is to the left or right of the spawner
        bool isLeftOfSpawner = spawnPosition.x < transform.position.x;

        // Set movement direction (negative speed for left, positive for right)
        float speedSign = isLeftOfSpawner ? 1f : -1f;
        mover.moveSpeed = Mathf.Abs(mover.moveSpeed) * speedSign;
    }
    */



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
        DeactivateObjectsWithTag("DebrisSmall2");
        DeactivateObjectsWithTag("DebrisMedium");
        DeactivateObjectsWithTag("DebrisMediumSolarPanel");
        DeactivateObjectsWithTag("DebrisMediumYellow");
        DeactivateObjectsWithTag("DebrisLarge");
        DeactivateObjectsWithTag("DebrisCloud");
    }

    public void DeactivateWithDelay(GameObject obj, float delay)
    {
        StartCoroutine(DeactivateCoroutine(obj, delay));
    }

    private IEnumerator DeactivateCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.activeInHierarchy)
        {
            obj.SetActive(false);
            //Debug.Log($"{obj} is deactivated.", obj);
        }
        else
        {
            //Debug.LogWarning($"{obj} is not assigned, nothing to deactivate.", obj);
        }
    }

    private void DeactivateObj(GameObject obj)
    {
        if (obj != null && obj.activeInHierarchy)
        {
            obj.SetActive(false);
            //Debug.Log($"{obj} is deactivated.", obj);
        }
            
        else
        {
            //Debug.LogWarning($"{obj} is not assigned, nothing to deactivate.", obj);
        }
            
    }
    private void ActivateObj(GameObject obj) 
    {
        if (obj != null && !obj.activeInHierarchy)
        {
            obj.SetActive(true);
            //Debug.Log($"{obj} is activated.", obj);
        }
        //else
        //Debug.LogWarning($"{obj} is not assigned, nothing to activate.", obj);
    }

    private void AutoAssignObjects()
    {
        // Auto-assign Net
        if (Net == null)
        {
            Net = SceneUtils.FindDeep("NetLauncher");
            if (Net == null)
                Debug.LogError("Couldn't find and assign 'NetLauncher'.");
        }
        // Auto-assign DebrisToCatchByNet
        if (DebrisToCatchByNet == null)
        {
            DebrisToCatchByNet = SceneUtils.FindDeep("DebrisToCatchByNet");
            if (DebrisToCatchByNet == null)
                Debug.LogError("Couldn't find and assign 'DebrisToCatchByNet'.");
        }
        /*
        // Auto-assign EndTextPanel
        if (EndTextPanel == null)
        {
            EndTextPanel = SceneUtils.FindDeep("EndTextPanel");
            if (EndTextPanel == null)
                Debug.LogError("Couldn't find and assign 'EndTextPanel'.");
        }
        */
    }
}
