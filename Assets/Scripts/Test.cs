using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;



public class Test : MonoBehaviour
{
    public GameObject layerPrefab; // Assign a prefab in the Inspector
    public Material hoverMaterial; // Assign hover material in the Inspector
    private Material originalMaterial; // To store the original material of the layer
    private Renderer layerRenderer;
    public GameObject uiWindow; // UI Window to show on trigger


    void Start()
    {
        // Instantiate the layer object
        GameObject layerObject = Instantiate(layerPrefab, transform.position, Quaternion.identity);

        // Add components and configure them
        
        XRSimpleInteractable interactable = layerObject.AddComponent<XRSimpleInteractable>();
        Rigidbody rigidbody = layerObject.AddComponent<Rigidbody>();
        LayerInteraction interactionScript = layerObject.AddComponent<LayerInteraction>();

        ConfigureInteractable(interactable);
        ConfigureRigidbody(rigidbody);
        ConfigureInteractionScript(interactionScript);

    }

    void ConfigureInteractionScript(LayerInteraction interactionScript)
    {
        // Assign the hover material to the interaction script
        interactionScript.hoverMaterial = hoverMaterial;
        interactionScript.uiWindow = uiWindow;
    }

    void ConfigureInteractable(XRSimpleInteractable interactable)
    {
        // Configure interactable properties as needed
  
    }

    void ConfigureRigidbody(Rigidbody rigidbody)
    {
        // Configure Rigidbody properties
        rigidbody.isKinematic = true; // Set true if you don't want it to be affected by physics
        rigidbody.useGravity = false; // Typically you want to disable gravity for UI elements
    }

}

