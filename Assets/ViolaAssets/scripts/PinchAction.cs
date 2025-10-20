using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;      // InputActionReference
using UnityEngine.XR.Hands;        // MetaAimFlags

public class PinchAction : MonoBehaviour
{
    [Header("Use your existing actions (drag from your XRI asset)")]
    [Tooltip("e.g. XRI Left/Select Value (float 0..1 for hand pinch)")]
    public InputActionReference leftSelectValue;
    [Tooltip("e.g. XRI Right/Select Value")]
    public InputActionReference rightSelectValue;

    [Tooltip("Optional fallback: XRI Left/Meta Aim Flags (int bitmask)")]
    public InputActionReference leftAimFlags;
    [Tooltip("Optional fallback: XRI Right/Meta Aim Flags")]
    public InputActionReference rightAimFlags;

    [Header("Hold settings")]
    [Range(0f, 1f)] public float pinchThreshold = 0.80f;  // typical pinch press threshold
    public float holdSeconds = 3.0f;                      // both hands must pinch this long

    [Header("Event")]
    public UnityEvent onHeld;                              // hook your action here

    float held;

    void Update()
    {
        bool leftPinch = IsLeftPinching();
        bool rightPinch = IsRightPinching();

        if (leftPinch && rightPinch)
        {
            held += Time.deltaTime;
            if (held >= holdSeconds)
            {
                held = 0f;          // one-shot; remove if you want to repeat-fire
                onHeld?.Invoke();
            }
        }
        else
        {
            held = 0f;
        }
    }

    bool IsLeftPinching() => ReadSelect(leftSelectValue) || ReadAimFlags(leftAimFlags);
    bool IsRightPinching() => ReadSelect(rightSelectValue) || ReadAimFlags(rightAimFlags);

    bool ReadSelect(InputActionReference actionRef)
    {
        var a = actionRef ? actionRef.action : null;
        if (a == null) return false;
        if (!a.enabled) a.Enable();                        // safe if already enabled
        float v = 0f;
        try { v = a.ReadValue<float>(); } catch { }
        return v >= pinchThreshold;
    }

    bool ReadAimFlags(InputActionReference actionRef)
    {
        var a = actionRef ? actionRef.action : null;
        if (a == null) return false;
        if (!a.enabled) a.Enable();
        int raw = 0;
        try { raw = a.ReadValue<int>(); } catch { }
        var flags = (MetaAimFlags)(unchecked((ulong)raw));
        return (flags & MetaAimFlags.IndexPinching) != 0;
    }
}
