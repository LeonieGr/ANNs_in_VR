using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;



public class LayerInteraction : MonoBehaviour
{
    public Material hoverMaterial; // Shared hover material for all layers
    public GameObject uiWindow; // UI Window to show on trigger
    public TextMeshProUGUI typeText, indexText, outputShapeText;
    private Material originalMaterial; // To store the original material of the layer
    private Renderer layerRenderer;
    public GetApiData.LayerInfo layerInfo;


    private void Awake()
    {

        layerRenderer = GetComponent<MeshRenderer>();
        if (layerRenderer != null)
        {
            // Store the original material of this specific layer
            originalMaterial = layerRenderer.material;
        }

        var interactable = GetComponent<XRSimpleInteractable>();
        Debug.Log(interactable);
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.activated.AddListener(OnActivateEntered);

            Debug.Log("Listeners are added");
        }
    }

    public void SetLayerInfo ()
    {
        if (layerInfo != null)
        {
            typeText.text = "Type: " + layerInfo.class_name;
            indexText.text = "Index: " + layerInfo.index;
            outputShapeText.text = "Output Shape: " + ArrayToString(layerInfo.output_shape);
        }
        else
        {
            Debug.LogError("Layer info is null");
        }
    }

    private string ArrayToString(int[] array)
    {
        return "[" + string.Join(", ", array) + "]";
    }

    private void OnDestroy()
    {
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.activated.RemoveListener(OnActivateEntered);
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        // Switch to hover material
        if (layerRenderer != null)
        {
            layerRenderer.material = hoverMaterial;
        }
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        // Revert to the original material
        if (layerRenderer != null)
        {
            layerRenderer.material = originalMaterial;
        }
    }

    public void OnActivateEntered(ActivateEventArgs args)
    {
        uiWindow.SetActive(true); // Show the UI window
        SetLayerInfo();
    }

     public void CloseUIWindow()
    {
        uiWindow.SetActive(false);
    }

    
}

