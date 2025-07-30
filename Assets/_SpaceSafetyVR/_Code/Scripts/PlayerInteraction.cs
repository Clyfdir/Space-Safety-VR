using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private Player player;
    private PlayerInputActions playerControls;
    private InputAction interact;

    void Awake()
    {
        playerControls = new PlayerInputActions();
        player = GetComponent<Player>();
    }

    void OnEnable()
    {
        interact = playerControls.Player.Interact;
        interact.Enable();
    }
    void OnDisable()
    {
        interact.Disable();
    }
    void Update()
    {

        // Check what the player is looking at at all times
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 10f))
        {
            // Are we looking at an interactable object?
            if (hit.collider.CompareTag("Interactable"))
            {
                if (hit.collider.gameObject.GetComponent<Interactable>().GetActive() == false) return;

                hit.collider.gameObject.GetComponent<Interactable>().Highlight();

                // If interact button clicked
                if (interact.triggered)
                {
                    // 
                    if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("PlayerLocation")) != 0)
                    {
                        Debug.Log("Interacting with: " + hit.collider.gameObject.name);
                        // Here you can add interaction logic, e.g., calling a method on the interactable object
                        hit.collider.gameObject.GetComponent<Interactable>().Highlight();
                        player.ChangePlayerLocation(hit.collider.gameObject.GetComponent<Interactable>());
                    }

                    if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Button")) != 0)
                    {
                        hit.collider.gameObject.GetComponent<Interactable>().Trigger();
                    }
                }
            }
        }
    }
 
}
