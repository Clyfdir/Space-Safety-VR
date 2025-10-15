using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CMESequenceInput : MonoBehaviour
{
    [Header("Assign your Input Action (e.g. CMETrigger)")]
    [Tooltip("Drag the InputActionReference from your Input Action Asset here.")]
    public InputActionReference cmeTriggerAction;

    private void OnEnable()
    {
        if (cmeTriggerAction != null)
            cmeTriggerAction.action.performed += OnCMETriggered;
    }

    private void OnDisable()
    {
        if (cmeTriggerAction != null)
            cmeTriggerAction.action.performed -= OnCMETriggered;
    }

    private void OnCMETriggered(InputAction.CallbackContext ctx)
    {
        Debug.Log("☀ CME Sequence Triggered!");
        StartCMESequence();
    }

    private void StartCMESequence()
    {
        SceneManager.LoadScene("2_CME");
    }
}