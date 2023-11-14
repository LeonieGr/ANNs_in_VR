using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// GetApiData fetches and visualizes layer information from a specified API endpoint
public class GetApiData : MonoBehaviour
{

    private string apiUrl = "http://172.22.29.196:4999/layer_info"; 

    // Prefabs for different types of layers
    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject TestObject;
    

    // Conversion factors to translate layer dimensions into Unity units
    private float pixelToUnit = 0.15f;
    private float featureMapToUnit = 0.03f; 
    private float neuronToUnit = 0.0002f; 



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
    void InstantiateLayers(LayerInfo[] layers)
    {
        float zPosition = -2f; // Initial z position for the first layer
        float spaceBetweenLayers = 1f;
        float annDepth = 0f; // To calculate the total depth of the ANN

        // Instantiate all layers and calculate the collective depth
        List<GameObject> instantiatedLayers = new List<GameObject>();
        foreach (LayerInfo layer in layers)
        {
            if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
            {
                GameObject layerObject = Instantiate(prefab);
                SetLayerSize(layer, layerObject);
                annDepth += layerObject.transform.localScale.z + spaceBetweenLayers;
                instantiatedLayers.Add(layerObject);
            }
            else
            {
                Debug.LogError($"Prefab for class {layer.class_name} not found.");
            }
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
    }

    // Set layer size based on the layers output shape
    void SetLayerSize(LayerInfo layer, GameObject instantiatedObject)
        {
            Vector3 newScale;

            if (layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D")
            {
                // Scaling for Conv2D and MaxPooling2D layers
                newScale = new Vector3(
                    layer.output_shape[1] * pixelToUnit, // X
                    layer.output_shape[2] * pixelToUnit, // Y
                    layer.output_shape[3] * featureMapToUnit // Z
                );
            }
            else if (layer.class_name == "Flatten" || layer.class_name == "Dense" || layer.class_name == "Dropout")
            {
                // Scaling for Flatten, Dense, and Dropout layers
                newScale = new Vector3(
                    1f, // X
                    1f, // Y
                    layer.output_shape[1] * neuronToUnit // Z
                );
            }
            else
            {
                newScale = new Vector3(1f, 1f, 1f); // Default scale for any other type of layer
            }

            instantiatedObject.transform.localScale = newScale;
        }


    void HandleError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}

