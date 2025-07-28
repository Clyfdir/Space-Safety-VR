using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Material baseMaterial;
    private bool highlight;

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
    }
    
    public void Highlight()
    {
        highlight = true;
    }
}
