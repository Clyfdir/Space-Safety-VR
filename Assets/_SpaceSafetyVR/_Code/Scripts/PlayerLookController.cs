using UnityEngine;
using UnityEngine.AI;
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
    [Header("")]

    [SerializeField] private float startingRotation;

    private float verticalRotation = 0;

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
        Vector2 lookDelta = look.ReadValue<Vector2>();

        float mouseX = lookDelta.x;
        float mouseY = lookDelta.y;

        transform.Rotate(Vector3.up * mouseX * horSensitivity);

        verticalRotation += -mouseY * verSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCamera.transform.position, playerCamera.transform.position +  Quaternion.Euler(0, startingRotation, 0) * new Vector3(0, 0, 1));
    }
}
