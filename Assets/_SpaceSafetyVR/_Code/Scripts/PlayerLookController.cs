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
    [SerializeField] private float startingRotation;
    [SerializeField] private float moveSpeed = 5f;

    private float verticalRotation = 0f;

    void Awake()
    {
        playerControls = new PlayerInputActions();

        transform.Rotate(Vector3.up * startingRotation);
        verticalRotation = 0f;
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
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
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        Vector2 lookDelta = look.ReadValue<Vector2>();

        float mouseX = lookDelta.x;
        float mouseY = lookDelta.y;

        transform.Rotate(Vector3.up * mouseX * horSensitivity);

        verticalRotation += -mouseY * verSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;

        transform.position += move.normalized * moveSpeed * Time.deltaTime;
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

    void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCamera.transform.position,
            playerCamera.transform.position + Quaternion.Euler(0, startingRotation, 0) * Vector3.forward);
    }
}
