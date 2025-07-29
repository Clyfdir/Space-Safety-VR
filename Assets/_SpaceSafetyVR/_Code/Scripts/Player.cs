using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private GameEvent playerSpawn;
    [SerializeField] private PlayerLocation playerPos;
    void Awake()
    {
        if (playerPos == null) return;
        transform.parent = playerPos.transform;
        transform.localPosition = Vector3.zero;

        playerSpawn.Occured();
    }


    public void ChangePlayerLocation(PlayerLocation newLocation)
    {
        if (newLocation == null) return;
        transform.parent = newLocation.transform;
        transform.localPosition = Vector3.zero;
    }
}
