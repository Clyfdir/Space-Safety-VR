using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private GameEvent playerSpawn;
    [SerializeField] private Interactable playerPos;
    void Awake()
    {
        playerSpawn.Occured();

        if (playerPos == null)
        {
            Debug.LogError("No initial PlayerLocation set for Player");
            return;
        }

        ChangePlayerLocation(playerPos);

        
    }


    public void ChangePlayerLocation(Interactable newLocation)
    {
        if (newLocation == null) return;
        transform.parent = newLocation.transform;
        transform.localPosition = Vector3.zero;
    }
}
