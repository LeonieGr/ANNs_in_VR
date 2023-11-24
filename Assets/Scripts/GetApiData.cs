using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;


// GetApiData fetches and visualizes layer information from a specified API endpoint
public class GetApiData : MonoBehaviour
{

    private string apiUrl = "http://172.22.26.200:4999/layer_info"; 

    // Prefabs for different types of layers
    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject TestObject;    

    // Conversion factor to translate layer dimensions into Unity units
    private float pixelToUnit = 0.04f;

    // Material of hovered layers
    public Material hoverMaterial;

    // Holds layer information from API
    [Serializable]
    public class LayerInfo
    {
        public string class_name;
        public int index;
        public string name;
        public int[] output_shape;
        public int parameters;
    }


    [Serializable]
    public class LayerInfoList
    {
        public LayerInfo[] layers;
    }

    //Dictionary to map class names to prefabs
    Dictionary<string, GameObject> classToPrefab = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Initialize
        classToPrefab["Conv2D"] = Conv2DPrefab;
        classToPrefab["MaxPooling2D"] = MaxPooling2DPrefab;
        classToPrefab["Dense"] = DensePrefab;
        classToPrefab["Flatten"] = FlattenPrefab;
        classToPrefab["Dropout"] = DropoutPrefab;
    }

    void Start()
    { 
        // Begin API call
        StartCoroutine(GetLayerInfo());
        Debug.Log("GetLayerInfo method is running.");

    }

    // Coroutine to fetch layer information from API
    IEnumerator GetLayerInfo()
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // On success, Parse the JSON response
                string jsonText = webRequest.downloadHandler.text;

                // Create a wrapper object for the JSON array
                LayerInfoList layerInfoList = JsonUtility.FromJson<LayerInfoList>("{\"layers\":" + jsonText + "}");

                // If response is valid, inistiate layers
                if (layerInfoList != null && layerInfoList.layers != null)
                {
                    InstantiateLayers(layerInfoList.layers);
                }
                else
                {
                    HandleError("Invalid JSON response.");
                }
            }

        }
    }

    // Instantiate the layers
   /* void InstantiateLayers(LayerInfo[] layers)
    {
        float zPosition = -2f; // Initial z position for the first layer
        float spaceBetweenLayers = 1f;
        float annDepth = 0f; // To calculate the total depth of the ANN

        // Instantiate all layers and calculate the collective depth
        List<GameObject> instantiatedLayers = new List<GameObject>();
        foreach (LayerInfo layer in layers)
        {
            GameObject layerParent = new GameObject(layer.class_name + "Layer");
            layerParent.transform.localPosition = new Vector3(-5, 2.5f, zPosition);
            instantiatedLayers.Add(layerParent);
            annDepth += layerParent.transform.localScale.z + spaceBetweenLayers;

            if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
            {

                if (layer.class_name == "Dense" || layer.class_name == "Dropout" || layer.class_name == "Flatten")
                {
                    int numberOfNeurons = layer.output_shape[1];
                    float verticalSpacing = 0.15f;
                    
                    if (numberOfNeurons>50)
                    {
                        numberOfNeurons = 50;
                    }
            
                    for (int i = 0; i < numberOfNeurons; i++)
                    {
                        GameObject neuron = Instantiate(prefab, parent: layerParent.transform);
                        // Position each neuron in a vertical line, adjusting only the y-coordinate
                        neuron.transform.localPosition = new Vector3(0, i * verticalSpacing, 0);

                    }
                }

                if (layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D")
                {
                    int pixel = layer.output_shape[1];
                    int featureMaps = layer.output_shape[3];
                    int dimension = Mathf.CeilToInt(Mathf.Sqrt(featureMaps)); // Rows and columns based on square root of feature maps
                    float spacing = pixel*0.01f;
                    float boxWidth = pixel * pixelToUnit;
                    float totalRowWidth = dimension * boxWidth + (dimension - 1) * spacing;
                    float startX = -totalRowWidth / 2 + boxWidth / 2; // Starting X position for the first box


                    for (int i = 0; i<featureMaps; i++)
                    {
                        int row = i / dimension;
                        int col = i % dimension;
                        GameObject featureMapBox = Instantiate(prefab, parent: layerParent.transform);
                        featureMapBox.transform.localScale = new Vector3(pixel * pixelToUnit, pixel * pixelToUnit, 0.3f);
                        featureMapBox.transform.localPosition = new Vector3(startX + col * (boxWidth + spacing), row * (boxWidth + spacing), 0);
                    }

                }
            }
            else
            {
                Debug.LogError($"Prefab for class {layer.class_name} not found.");
            }
        }

        float SigmoidScale(float x)
        {
            float a = 6.0f; // Adjust 'a' to control how steep the curve is
            float scaleLimit = 0.8f; // The maximum scale factor

            return scaleLimit / (1.0f + Mathf.Exp(-a * (x - 0.5f)));
        }
        
        for (int i = 0; i < instantiatedLayers.Count; i++)
        {
            GameObject layerParent = instantiatedLayers[i];
            LayerInfo layerInfo = layers[i];

            if (layerInfo.class_name == "Conv2D" || layerInfo.class_name == "MaxPooling2D")
            {
                float layerSize = CalculateLayerSize(layerParent);
                float layerScaleFactor = SigmoidScale(layerSize);
                layerParent.transform.localScale = new Vector3(layerScaleFactor, layerScaleFactor, layerScaleFactor);
            }
        }


        float CalculateLayerSize(GameObject layerParent)
        {
            Renderer[] renderers = layerParent.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return 0f;
            }

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Return the size of the bounds in the x dimension (width)
            return bounds.size.x;
        }

                

        // Subtract the last added space as there is no layer after the last one
        annDepth -= spaceBetweenLayers;

        // Check if the ANN model exceeds the boundaries and scale down if necessary
        float maxDepth = 17f; // Assuming -2 to -19 Z space boundary
        float scaleFactor = annDepth > maxDepth ? maxDepth / annDepth : 1f;
        
        // Create a parent object to hold all layers
        GameObject annParent = new GameObject("ANNModel");

        // Now position the layers and apply the scaling factor if needed
        foreach (GameObject layerObject in instantiatedLayers)
        {
            layerObject.transform.SetParent(annParent.transform);
            layerObject.transform.localScale *= scaleFactor; // Apply scaling factor
            layerObject.transform.localPosition = new Vector3(0f, 2.5f, zPosition - (layerObject.transform.localScale.z / 2f * scaleFactor));
            zPosition -= (layerObject.transform.localScale.z * scaleFactor + spaceBetweenLayers);

        }
    }*/

    void InstantiateLayers(LayerInfo[] layers)
    {
        float zPosition = 13f;
        float spaceBetweenLayers = 1f;
        float annDepth = 0f;
        GameObject annParent = new GameObject("ANNModel");
        List<GameObject> instantiatedLayers = new List<GameObject>();

        for (int i = 0; i < layers.Length; i++)
        {
            LayerInfo layer = layers[i];
            GameObject layerParent = CreateLayerParent(layer, zPosition);
            instantiatedLayers.Add(layerParent);
            bool isLastLayer = (i==layers.Length -1);
            InstantiateLayerComponents(layer, layerParent,isLastLayer);
            AddColliderToLayer(layerParent);
            UpdateLayerDepth(ref annDepth, layerParent, spaceBetweenLayers, ref zPosition);

            // Make the layer interactable
            XRSimpleInteractable interactable = layerParent.AddComponent<XRSimpleInteractable>();
            Rigidbody rigidbody = layerParent.AddComponent<Rigidbody>();
            LayerInteraction interactionScript = layerParent.AddComponent<LayerInteraction>();

            ConfigureInteractable(interactable);
            ConfigureRigidbody(rigidbody);
            ConfigureInteractionScript(interactionScript);


        }

        ApplySelectiveScaling(layers, instantiatedLayers);
        PositionLayers(instantiatedLayers, annParent, ref zPosition, spaceBetweenLayers, ref annDepth);
    }

    GameObject CreateLayerParent(LayerInfo layer, float zPosition)
    {
        GameObject layerParent = new GameObject(layer.class_name + "Layer");
        layerParent.transform.localPosition = new Vector3(-5, 1f, zPosition);
        return layerParent;
    }

    void InstantiateLayerComponents(LayerInfo layer, GameObject layerParent, bool isLastLayer)
    {
        if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
        {
            if (isLastLayer)
            {
                InstantiateLastLayerNeurons(layer, prefab, layerParent);
            }
            else if (IsDenseDropoutFlattenLayer(layer))
            {
                InstantiateNeurons(layer, prefab, layerParent);
            }
            else if (IsConvOrPoolingLayer(layer))
            {
                InstantiateFeatureMaps(layer, prefab, layerParent);
            }
        }
        else
        {
            Debug.LogError($"Prefab for class {layer.class_name} not found.");
        }
    }

    bool IsDenseDropoutFlattenLayer(LayerInfo layer)
    {
        return layer.class_name == "Dense" || layer.class_name == "Dropout" || layer.class_name == "Flatten";
    }

    bool IsConvOrPoolingLayer(LayerInfo layer)
    {
        return layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D";
    }

    void InstantiateNeurons(LayerInfo layer, GameObject prefab, GameObject layerParent)
    {
        int numberOfNeurons = Mathf.Min(layer.output_shape[1], 50);
        float verticalSpacing = 0.2f;

        for (int i = 0; i < numberOfNeurons; i++)
        {
            GameObject neuron = Instantiate(prefab, parent: layerParent.transform);
            neuron.transform.localPosition = new Vector3(0, i * verticalSpacing, 0);
        }
    }

    void InstantiateFeatureMaps(LayerInfo layer, GameObject prefab, GameObject layerParent)
    {
        int pixel = layer.output_shape[1];
        int featureMaps = layer.output_shape[3];
        int dimension = Mathf.CeilToInt(Mathf.Sqrt(featureMaps));
        float spacing = pixel * 0.01f;
        float boxWidth = pixel * pixelToUnit;
        float totalRowWidth = dimension * boxWidth + (dimension - 1) * spacing;
        float startX = -totalRowWidth / 2 + boxWidth / 2;

        for (int i = 0; i < featureMaps; i++)
        {
            int row = i / dimension;
            int col = i % dimension;
            GameObject featureMapBox = Instantiate(prefab, parent: layerParent.transform);
            featureMapBox.transform.localScale = new Vector3(boxWidth, boxWidth, 0.3f);
            featureMapBox.transform.localPosition = new Vector3(startX + col * (boxWidth + spacing), row * (boxWidth + spacing), 0);
        }
    }

    void InstantiateLastLayerNeurons(LayerInfo layer, GameObject prefab, GameObject layerParent)
    {
        int numberOfNeurons = Mathf.Min(layer.output_shape[1], 50);
        float horizontalSpacing = 1.0f; // Adjust as needed
        float neuronWidth = 0.7f; // Assuming each neuron has a width of 1 unit, adjust as needed
        float totalLayerWidth = numberOfNeurons * neuronWidth + (numberOfNeurons - 1) * horizontalSpacing;
        float startX = -totalLayerWidth / 2 + neuronWidth / 2;

        for (int i = 0; i < numberOfNeurons; i++)
        {
            GameObject neuron = Instantiate(prefab, parent: layerParent.transform);
            neuron.transform.localPosition = new Vector3(startX + i * (neuronWidth + horizontalSpacing), 0, 0);

            // Apply a different scale for the last layer neurons, if needed
            float neuronScaleFactor = 0.7f; // Adjust as needed
            neuron.transform.localScale = new Vector3(neuronScaleFactor, neuronScaleFactor, neuronScaleFactor);
        }
    }

    void AddColliderToLayer(GameObject layerObject)
    {
        if (layerObject.GetComponent<Renderer>() == null && layerObject.GetComponentsInChildren<Renderer>().Length == 0)
        {
            // No renderer found, may not need a collider or cannot calculate bounds
            return;
        }

        BoxCollider collider = layerObject.AddComponent<BoxCollider>();
        collider.size = CalculateBoundsSize(layerObject);
        collider.center = CalculateBoundsCenter(layerObject);
    }

    Vector3 CalculateBoundsSize(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size;
    }

    Vector3 CalculateBoundsCenter(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.center - layerObject.transform.position;
    }


    void ApplySelectiveScaling(LayerInfo[] layers, List<GameObject> instantiatedLayers)
    { 
        for (int i = 0; i < instantiatedLayers.Count; i++)
        {
            if (IsConvOrPoolingLayer(layers[i]))
            {
                float layerSize = CalculateLayerSize(instantiatedLayers[i]);
                float layerScaleFactor = SigmoidScale(layerSize);
                instantiatedLayers[i].transform.localScale = new Vector3(layerScaleFactor, layerScaleFactor, layerScaleFactor);
            }
        }
    }


    float CalculateLayerSize(GameObject layerParent)
    {
        Renderer[] renderers = layerParent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds.size.x;
    }

    float SigmoidScale(float x)
    {
        float a = 6.0f;
        float scaleLimit = 0.8f;
        return scaleLimit / (1.0f + Mathf.Exp(-a * (x - 0.5f)));
    }


    void UpdateLayerDepth(ref float annDepth, GameObject layerParent, float spaceBetweenLayers, ref float zPosition)
    {
        annDepth += layerParent.transform.localScale.z + spaceBetweenLayers;
        zPosition -= layerParent.transform.localScale.z + spaceBetweenLayers;
    }

    void PositionLayers(List<GameObject> instantiatedLayers, GameObject annParent, ref float zPosition, float spaceBetweenLayers, ref float annDepth)
    {
        float maxDepth = 17f;
        float scaleFactor = CalculateScaleFactor(annDepth, maxDepth);

        foreach (GameObject layerObject in instantiatedLayers)
        {
            layerObject.transform.SetParent(annParent.transform);
            layerObject.transform.localScale *= scaleFactor;
            layerObject.transform.localPosition = new Vector3(0f, 1f, zPosition - (layerObject.transform.localScale.z / 2f * scaleFactor));
            zPosition -= (layerObject.transform.localScale.z * scaleFactor + spaceBetweenLayers);
        }

        annDepth -= spaceBetweenLayers;
    }

    float CalculateScaleFactor(float annDepth, float maxDepth)
    {
        return annDepth > maxDepth ? maxDepth / annDepth : 1f;
    }

    void ConfigureInteractable(XRSimpleInteractable interactable)
    {
        // Configure interactable properties
        // Example: interactable.interactionLayerMask = ...
        // Add listeners for interaction events if needed
       // interactable.onSelectEntered.AddListener((args) => OnLayerSelected(args));
    }

    void ConfigureInteractionScript(LayerInteraction interactionScript)
    {
        interactionScript.hoverMaterial = hoverMaterial;
    }

    void OnLayerSelected(SelectEnterEventArgs args)
    {
        // Logic to execute when a layer is selected
        // For example, display information panel for this layer
    }

    void ConfigureRigidbody(Rigidbody rigidbody)
    {
        // Set Rigidbody properties
        rigidbody.isKinematic = true; // Set to true if you don't want the layer to be affected by physics forces
        rigidbody.useGravity = false; // Typically you want to disable gravity for UI elements
    }

    void HandleError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}

