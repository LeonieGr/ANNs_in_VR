using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;


public class LayerInteraction : MonoBehaviour {

    public Material hoverMaterial, hoverParticleMaterial; // Shared hover material for all layers
    public GameObject uiWindow; // UI Window to show on trigger
    public TextMeshProUGUI typeText, indexText, outputShapeText, activationText;
    public GetApiData.LayerInfo layerInfo;
    public GameObject [] childObjects;
    public GameObject testObject;
    public bool isNeuronLayer;

    private Material[] originalChildMaterials; // To store the original materials of the children


    public void Initialize() {
        // Get the original material of all children of the layer
        if (childObjects != null) {
            originalChildMaterials = new Material[childObjects.Length];
            for (int i = 0; i < childObjects.Length; i++) {
                Renderer childRenderer = childObjects[i].GetComponent<Renderer>();
                if (childRenderer != null) {
                    originalChildMaterials[i] = childRenderer.material;
                }
            }
        }

        var interactable = GetComponent<XRSimpleInteractable>();
       
       // Listeners for interactable events
        if (interactable != null) {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.activated.AddListener(OnActivateEntered);
        }
    }

    // Set Texts according to Layer Information
    public void SetLayerInfo () {
        if (layerInfo != null) {
            typeText.text = "Type: " + layerInfo.class_name;
            indexText.text = "Index: " + layerInfo.index;
            outputShapeText.text = "Output Shape: " + ArrayToString(layerInfo.output_shape);
            if (layerInfo.activation != null) {
                activationText.text = "Activation Function: " + layerInfo.activation;
            }
        } else {
            Debug.LogError("Layer info is null");
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args) {

        // Switch to hover material for each child object
        if (childObjects != null) {
            foreach (GameObject child in childObjects) {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null) {
                    if (isNeuronLayer){
                        childRenderer.material = hoverParticleMaterial;
                    } else {
                        childRenderer.material = hoverMaterial;
                    }
                }
            }
        }
    }

    public void OnHoverExited(HoverExitEventArgs args) {
       // Revert to the original material for each child object
        if (childObjects != null) {
            for (int i = 0; i < childObjects.Length; i++) {
                Renderer childRenderer = childObjects[i].GetComponent<Renderer>();
                if (childRenderer != null && i < originalChildMaterials.Length) {
                    childRenderer.material = originalChildMaterials[i];
                    Debug.Log("og material back");
                }
            }
        }
    }

    public void OnActivateEntered(ActivateEventArgs args) {
        uiWindow.SetActive(true); // Show the UI window
        SetLayerInfo();
    }

    public void CloseUIWindow() {
        uiWindow.SetActive(false);
    }

    private string ArrayToString(int[] array) {
        return "[" + string.Join(", ", array) + "]";
    }

    private void OnDestroy() {
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null) {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.activated.RemoveListener(OnActivateEntered);
        }
    }
}

