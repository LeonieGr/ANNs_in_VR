using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;


// GetApiData fetches and visualizes layer information from a specified API endpoint
public class GetApiData : MonoBehaviour {

    private string apiUrl; //= "http://172.22.27.118:4999/autoencoder/layer_info"; 
    private Dictionary<string, string> modelUrls = new Dictionary<string, string>{
        {"None",""},
        {"Sequential", "http://192.168.2.104:4999/sequential/layer_info"},
        {"Autoencoder", "http://192.168.2.104:4999/autoencoder/layer_info"},
        {"VGG", "http://192.168.2.104:4999/vgg/layer_info"}
    };

    // Conversion factor to translate layer dimensions into Unity units
    private float pixelToUnit = 0.04f;

    // Prefabs for different types of layers
    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject InputPrefab;
    public GameObject ReshapePrefab;
    public GameObject UpSampling2DPrefab;
    public GameObject ConcatenatePrefab;     

    // Material of hovered layers
    public Material hoverMaterial;
    public GameObject uiWindow; // UI Window to show on trigger
    public TextMeshProUGUI typeText, indexText, outputShapeText;

    //Dropdown menu to choose model
    public TMP_Dropdown modelDropdownEN;
    public TMP_Dropdown modelDropdownDE; 

    // Dictionary to map class names to prefabs
    public Dictionary<string, GameObject> classToPrefab = new Dictionary<string, GameObject>();

    // Holds layer information from API
    [Serializable]
    public class LayerInfo {
        public string activation = null;
        public string class_name;
        public int index;
        public string name;
        public int[] output_shape ;
        public int parameters;
    }

    // Holds layers
    [Serializable]
    public class LayerInfoList {
        public LayerInfo[] layers;
    }

    void Awake() {
        // Initialize
        classToPrefab["Conv2D"] = Conv2DPrefab;
        classToPrefab["MaxPooling2D"] = MaxPooling2DPrefab;
        classToPrefab["Dense"] = DensePrefab;
        classToPrefab["Flatten"] = FlattenPrefab;
        classToPrefab["Dropout"] = DropoutPrefab;
        classToPrefab["InputLayer"] = InputPrefab;
        classToPrefab["Reshape"] = ReshapePrefab;
        classToPrefab["UpSampling2D"] = UpSampling2DPrefab;
        classToPrefab["Concatenate"] = ConcatenatePrefab;

    }

    void Start() { 
        // Begin API call
        InitializeDropdown(modelDropdownDE);
        InitializeDropdown(modelDropdownEN);
        //StartCoroutine(GetLayerInfo());
        Debug.Log("GetLayerInfo method is running.");

    }

    private void InitializeDropdown(TMP_Dropdown dropdown) {
        dropdown.ClearOptions();
        List<string> options = new List<string>(modelUrls.Keys);
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    // Method to be called when the dropdown value changes
    void DropdownValueChanged(TMP_Dropdown dropdown) {

        if (!dropdown.gameObject.activeInHierarchy) return; // Ignore inactive dropdown

        string selectedModel = dropdown.options[dropdown.value].text;
        if (modelUrls.TryGetValue(selectedModel, out string url)) {
            apiUrl = url;
            StartCoroutine(GetLayerInfo()); // Fetch new data
        } else {
            Debug.LogError("URL for selected model not found.");
        }
    }

    // Coroutine to fetch layer information from API
    IEnumerator GetLayerInfo() {
        
        // GET request to the API
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl)) {
            // waiting for request to complete
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Error: " + webRequest.error);
            } else {
                // On success, Parse the JSON response
                string jsonText = webRequest.downloadHandler.text;

                // Create a wrapper object for the JSON array
                LayerInfoList layerInfoList = JsonUtility.FromJson<LayerInfoList>("{\"layers\":" + jsonText + "}");

                // If response is valid, inistiate layers
                if (layerInfoList != null && layerInfoList.layers != null) {
                    InstantiateLayers(layerInfoList.layers);
                } else {
                    HandleError("Invalid JSON response.");
                }
            }
        }
    }

    // Method to instantiate layer visuals
    void InstantiateLayers(LayerInfo[] layers) {
        // Initial settings for positioning layers in 3D space
        float zPosition = 0f;
        float spaceBetweenLayers = 1f;
        float annDepth = 0f;
        GameObject annParent = new GameObject("ANNModel");
        List<GameObject> instantiatedLayers = new List<GameObject>();

        // Iterate through each layer and instantiate its visual representation
        for (int i = 0; i < layers.Length; i++) {
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
            ConfigureInteractionScript(interactionScript, layer, layerParent);

        }

        // Apply scaling and positioning
         ApplySelectiveScaling(layers, instantiatedLayers);
         PositionLayers(instantiatedLayers, annParent, ref zPosition, spaceBetweenLayers, ref annDepth);
    }

    // Create a parent object for each layer for better organization in the scene
    GameObject CreateLayerParent(LayerInfo layer, float zPosition) {
        GameObject layerParent = new GameObject(layer.class_name + "Layer");
        layerParent.transform.localPosition = new Vector3(-5, 1f, zPosition);
        return layerParent;
    }

    // Instantiate specific components of a layer based on its type
    void InstantiateLayerComponents(LayerInfo layer, GameObject layerParent, bool isLastLayer) {
        // Check for the layer type and instantiate the appropriate visual components
        if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab)) {
            if (IsDenseDropoutFlattenLayer(layer)) {
                InstantiateNeurons(layer, prefab, layerParent, isLastLayer);
            }
            else if (IsConvOrPoolingLayer(layer)) {
                InstantiateFeatureMaps(layer, prefab, layerParent);
            }
        } else {
            Debug.LogError($"Prefab for class {layer.class_name} not found.");
        }
    }

    // Check if the layer is of type Dense, Dropout, or Flatten
    bool IsDenseDropoutFlattenLayer(LayerInfo layer) {
        return layer.class_name == "Dense" || layer.class_name == "Dropout" || layer.class_name == "Flatten";
    }

    // Check if the layer is of type Conv2D or MaxPooling2D
    bool IsConvOrPoolingLayer(LayerInfo layer) {
        return layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D" || layer.class_name == "Concatenate" || layer.class_name == "UpSampling2D" || layer.class_name == "Reshape" || layer.class_name == "InputLayer";
    }

    // Instantiate neurons as a particle system for a given layer
    void InstantiateNeurons(LayerInfo layer, GameObject prefab, GameObject layerParent, bool isLastLayer) {

        // Determine the number of neurons
        int numberOfNeurons = layer.output_shape[1];

        // Instantiate the prefab with the particle system
        GameObject neuronSystem = Instantiate(prefab, parent: layerParent.transform);
        neuronSystem.transform.localPosition = new Vector3(0, 3, 0);

        // Get the particle system components of the children
        ParticleSystem[] particleSystemArray = neuronSystem.GetComponentsInChildren<ParticleSystem>();

        if (particleSystemArray != null  && particleSystemArray.Length > 0) {
            ParticleSystem particleSystem = particleSystemArray[0];

            // Access to modules in particle system and configure values as needed
            var mainModule = particleSystem.main;
            var emissionModule = particleSystem.emission;
            var shapeModule = particleSystem.shape;
            var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            mainModule.maxParticles = numberOfNeurons;
            mainModule.startLifetime = Mathf.Infinity;
            mainModule.startSpeed = 0f;
            emissionModule.rateOverTime = 0f;

            // Special handeling for last layer: horizontal, increased size, lower positioning
            if (isLastLayer) {
                neuronSystem.transform.localPosition = new Vector3(0, 0, 0);
                shapeModule.rotation = new Vector3(shapeModule.rotation.x, shapeModule.rotation.y, 0f);
                particleRenderer.maxParticleSize = 0.07f;
            }

            // Configure a burst to emit the exact number of particles once
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0.0f, (short)numberOfNeurons);
            emissionModule.SetBursts(new ParticleSystem.Burst[] { burst });
        }  
    }

    // Instantiate feature maps for convolutional and pooling layers
    void InstantiateFeatureMaps(LayerInfo layer, GameObject prefab, GameObject layerParent) {
        if (layer.output_shape != null) {

            int pixel = layer.output_shape[1];
            int featureMaps = layer.output_shape[3];
            int dimension = Mathf.CeilToInt(Mathf.Sqrt(featureMaps));
            float spacing = pixel * 0.01f;
            float boxWidth = pixel * pixelToUnit;
            float totalRowWidth = dimension * boxWidth + (dimension - 1) * spacing;
            float startX = -totalRowWidth / 2 + boxWidth / 2;

            for (int i = 0; i < featureMaps; i++) {
                int row = i / dimension;
                int col = i % dimension;
                GameObject featureMapBox = Instantiate(prefab, parent: layerParent.transform);
                featureMapBox.transform.localScale = new Vector3(boxWidth, boxWidth, 0.3f);
                featureMapBox.transform.localPosition = new Vector3(startX + col * (boxWidth + spacing), row * (boxWidth + spacing), 0);
            }
        } else {
            Debug.Log("invalid outputshape");
        }
    }

    // Add a collider to the layer for interaction purposes
    void AddColliderToLayer(GameObject layerObject) {
        // Check if the object has a renderer to determine if a collider is needed
        if (layerObject.GetComponent<Renderer>() == null && layerObject.GetComponentsInChildren<Renderer>().Length == 0) {
            // No renderer found, may not need a collider or cannot calculate bounds
            return;
        }

        // Add a box collider and adjust its size and position based on the object's bounds
        BoxCollider collider = layerObject.AddComponent<BoxCollider>();
        collider.size = CalculateBoundsSize(layerObject);
        collider.center = CalculateBoundsCenter(layerObject);
    }

    // Calculate the bounds size for the collider based on renderers
    Vector3 CalculateBoundsSize(GameObject layerObject) {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);

        // Expand the bounds to include all child renderers
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>()) {
            bounds.Encapsulate(renderer.bounds);
        }

        // Return the total size of the bounds
        return bounds.size;
    }

    // Calculate the center of the bounds for the collider
    Vector3 CalculateBoundsCenter(GameObject layerObject) {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>()) {
            bounds.Encapsulate(renderer.bounds);
        }
        
        // Return the center of the bounds relative to the object's position
        return bounds.center - layerObject.transform.position;
    }


    void ApplySelectiveScaling(LayerInfo[] layers, List<GameObject> instantiatedLayers) { 
        for (int i = 0; i < instantiatedLayers.Count; i++) {
            // Apply scaling only to convolutional or pooling layers
            if (IsConvOrPoolingLayer(layers[i])) {
                // Calculate layer size and apply a sigmoid-based scaling factor
                float layerSize = CalculateLayerSize(instantiatedLayers[i]);
                Debug.Log(layerSize); // bound size
                float layerScaleFactor = SigmoidScale(layerSize); 
                float adjustmentFactor = 10.0f / layerSize;
                //layerScaleFactor *= adjustmentFactor;
                 Debug.Log("Factor:" + layerScaleFactor);


                Vector3 currentScale = instantiatedLayers[i].transform.localScale;
                instantiatedLayers[i].transform.localScale = new Vector3(  
                    currentScale.x * layerScaleFactor,
                    currentScale.y * layerScaleFactor,
                    currentScale.z * layerScaleFactor);
            }
        }
    }

    // Calculate the size of a layer based on its renderers
    float CalculateLayerSize(GameObject layerParent) {
        // Obtain all renderers and calculate the combined bounds
        Renderer[] renderers = layerParent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) {
            bounds.Encapsulate(renderer.bounds);
        }
        // Return the size in a specific dimension (x-axis in this case)
        return bounds.size.x; 

       /* Transform[] childTransforms = layerParent.GetComponentsInChildren<Transform>();
        if (childTransforms.Length == 0) {
            return 0f;
        }

        float maxSizeX = childTransforms[0].localScale.x;
        foreach (Transform childTransform in childTransforms) {
            maxSizeX = Mathf.Max(maxSizeX, childTransform.localScale.x);
        }

        // Return the maximum size in the x-axis
        return maxSizeX;*/

    }

    // Sigmoid function for scaling layer size
    float SigmoidScale(float x) {
        float a = 2.0f; // Steepness
        float scaleLimit = 1f; // Upper limit of the scale factor
        float normalizedX = x / 9.0f;
        float minValue = 0.1f;
        // Sigmoid funtion for smooth scaling
        float scaleValue = scaleLimit - (scaleLimit / (1.0f + Mathf.Exp(-a * (normalizedX - 0.5f))));
        if (scaleValue > minValue) {
            return scaleValue;
        } else {
            return minValue;
        }
    }

    // Update the depth and position of each layer in the ANN model
    void UpdateLayerDepth(ref float annDepth, GameObject layerParent, float spaceBetweenLayers, ref float zPosition) {
        // Increment the depth and adjust the zPosition for the next layer
        annDepth += layerParent.transform.localScale.z + spaceBetweenLayers;
        zPosition -= layerParent.transform.localScale.z + spaceBetweenLayers;
    }

    // Position all layers within the ANN model and apply scaling if necessary
    void PositionLayers(List<GameObject> instantiatedLayers, GameObject annParent, ref float zPosition, float spaceBetweenLayers, ref float annDepth) {
        // Maximum allowable depth for the model
        float maxDepth = 27f;
        // Calculate the scaling factor based on the total depth and max depth
        float layerScaleFactor = CalculateScaleFactor(annDepth, maxDepth);

        zPosition = -2f;

        // Apply the scaling and positioning with adjusted zPosition to each layer
        foreach (GameObject layerObject in instantiatedLayers) {
            layerObject.transform.SetParent(annParent.transform);
            layerObject.transform.localScale *= layerScaleFactor;
            layerObject.transform.localPosition = new Vector3(0f, 1f, zPosition - (layerObject.transform.localScale.z / 2f * layerScaleFactor));
            zPosition -= (layerObject.transform.localScale.z * layerScaleFactor + spaceBetweenLayers * layerScaleFactor);
        }

        // Adjust the total depth after positioning
        annDepth -= spaceBetweenLayers;
    }

    // Calculate the scaling factor based on actual and maximum depth
    float CalculateScaleFactor(float annDepth, float maxDepth) {
        // Scale down if the total depth exceeds the maximum, else use actual size
         return annDepth > maxDepth ? maxDepth / annDepth : 1f;
    }

    void ConfigureInteractable(XRSimpleInteractable interactable) {
        // Configure interactable properties
    }

    // Configure the interaction script for a layer
    void ConfigureInteractionScript(LayerInteraction interactionScript, LayerInfo layer, GameObject layerParent) {
        interactionScript.hoverMaterial = hoverMaterial;
        interactionScript.typeText =  typeText;
        interactionScript.indexText = indexText;
        interactionScript.outputShapeText = outputShapeText;
        interactionScript.uiWindow = uiWindow;
        interactionScript.layerInfo = layer;
         // Get and assign the child objects
        Transform[] children = layerParent.gameObject.GetComponentsInChildren<Transform>(true);
        interactionScript.childObjects = children.Where(child => child != layerParent.transform)
                                                .Select(child => child.gameObject)
                                                .ToArray();
    }
    
    string ArrayToString(int[] array) {
        return "[" + string.Join(", ", array) + "]";
    }


    void ConfigureRigidbody(Rigidbody rigidbody) {
        // Set Rigidbody properties
        rigidbody.isKinematic = true; // don't want the layer to be affected by physics forces
        rigidbody.useGravity = false;
    }

    void HandleError(string errorMessage){
        Debug.LogError(errorMessage);
    }
}