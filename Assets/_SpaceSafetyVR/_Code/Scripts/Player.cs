using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private GameEvent playerSpawn;
    void Awake()
    {
        playerSpawn?.Occured();
    }
}
