using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameEvent triggerEvent;
    [SerializeField] private Material baseMaterial;
    private bool highlight;

    private bool isActive;

    void OnEnable()
    {
        if (baseMaterial == null)
        {
            baseMaterial = GetComponent<Renderer>().material;
        }

        isActive = false;
    }

    void Update()
    {
        // if highlight wasn't set true this frame -> reset to base color
        if (!highlight)
        {
            GetComponent<Renderer>().material.color = baseMaterial.color;
        }
        // Called at the end -> will overwrite prev code
        if (highlight)
        {
            GetComponent<Renderer>().material.color = baseMaterial.color * 1.5f;

            // Reset highlight at end of the frame
            highlight = false;
        }

        if (!isActive)
        {
            // If not active, lower the color brightness
            GetComponent<Renderer>().material.color = baseMaterial.color * 0.5f;
        }
    }

    public void Highlight()
    {
        highlight = true;
    }

    public void Trigger()
    {
        triggerEvent.Occured();
    }

    public void Activate()
    {
        isActive = true;
    }

    public bool GetActive()
    {
        return isActive;
    }
}
