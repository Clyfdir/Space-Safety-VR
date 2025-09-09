///   this script added by Tatiana Gvozdenko, Hochschule Darmstadt, SoSe25
///   P6, Group project: Safe Space 
///   AI was used: GPT
///   Created: 12.07.2025
///   Last Change: 12.07.2025   

///   Has the function to show the final text

using UnityEngine;

public class FinalTextAppearance : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField] private GameObject finalTextMessage;
    //[SerializeField] private bool shouldAppear = false;// just for testing in Editor

    private void Start()
    {

        if (finalTextMessage == null)
        {
            Debug.LogWarning("finalTextMessage is not assigned.");
        }
        else
        {
            // start disabled
            finalTextMessage.SetActive(false);
        }
        
         //PWEventsManager.Instance?.ShipHit.AddListener(ActivateMeshRenderer);
    }

    private void OnEnable()
    {
       
       //  PWEventsManager.Instance?.UpdateScene.AddListener(ActivateMeshRenderer);
    }
    // Call this method to enable the MeshRenderer on this GameObject.
    public void ActivateMeshRenderer()
    {
        if (finalTextMessage == null)
        {
            Debug.LogWarning("finalTextMessage is not assigned.");
            return;
        }

        finalTextMessage.SetActive(true);
    }

    /*
        private void Update()// just for testing in Editor
    {
        if (shouldAppear)
        {
            ActivateMeshRenderer();
        }
    }
    */
}