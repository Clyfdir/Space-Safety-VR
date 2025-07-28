using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerLookController : MonoBehaviour
{
    #region Input Actions

    private PlayerInputActions playerControls;
    private InputAction look;

    #endregion

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float horSensitivity = 2f;
    [SerializeField] private float verSensitivity = 2f;

    private float verticalRotation = 0;

    void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    void OnEnable()
    {
        LockCursor();
        look = playerControls.Player.Look;
        look.Enable();
    }
    void OnDisable()
    {
        UnlockCursor();
        look.Disable();
    }

    void Update()
    {
        Vector2 lookDelta = look.ReadValue<Vector2>();

        float mouseX = lookDelta.x;
        float mouseY = lookDelta.y;

        // Example: apply rotation to camera
        transform.Rotate(Vector3.up * mouseX * horSensitivity);

        verticalRotation += -mouseY * verSensitivity; // Invert to behave like typical FPS
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f); // Clamp up/down angle

        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
