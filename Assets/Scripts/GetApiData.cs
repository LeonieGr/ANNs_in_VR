using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class GetApiData : MonoBehaviour
{

    private string apiUrl = "http://172.22.27.191:4999/layer_info"; 

    // Prefabs for different types of layers
    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject TestObject;
    
    // default sizes 
    float defaultDenseHeight = 5f; 
    float defaultDenseWidth = 0.5f; 
    float defaultDenseDepth = 0.3f; 
    Vector3 defaultDenseSize = new Vector3(0.5f, 5f, 0.3f);
    Vector3 defaultFlattenSize = new Vector3(0.5f, 4f, 0.3f);


    // hold layer information from API
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
        StartCoroutine(GetLayerInfo());
        Debug.Log("GetLayerInfo method is running.");

        //Works
        //Instantiate(TestObject, transform.position, Quaternion.identity);

    }

    // fetch layer information 
    IEnumerator GetLayerInfo()
    {
        // works
        //Instantiate(TestObject, transform.position, Quaternion.identity);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            // works 
            // Instantiate(TestObject, transform.position, Quaternion.identity);

            yield return webRequest.SendWebRequest();

            //works
            //Instantiate(TestObject, transform.position, Quaternion.identity);

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                //stops working at this point
                //Instantiate(TestObject, transform.position, Quaternion.identity);

                // Parse the JSON response
                string jsonText = webRequest.downloadHandler.text;

                // Create a wrapper object for the JSON array
                LayerInfoList layerInfoList = JsonUtility.FromJson<LayerInfoList>("{\"layers\":" + jsonText + "}");

                if (layerInfoList != null && layerInfoList.layers != null)
                {
                    InstantiateLayers(layerInfoList.layers);
                }
                else
                {
                    HandleError("Invalid JSON response.");
                }
            }

            //works
            //Instantiate(TestObject, transform.position, Quaternion.identity);
        }
    }

    // Instantiate objects for each layer based on the retrieved information
    void InstantiateLayers(LayerInfo[] layers)
    {

        int i = 0;
        foreach (LayerInfo layer in layers)
        {

                Debug.Log($"Layer Name: {layer.name}, Class: {layer.class_name}, Output Shape: {string.Join(", ", layer.output_shape)}");

            if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
            {
                GameObject instantiatedObject = Instantiate(prefab, new Vector3(i * 2f, 2f, -5f), Quaternion.Euler(0f, 90f, 0f));
                Debug.Log("PrefabInstatiated");

                SetLayerSize(layer, instantiatedObject);
                i++;
            }
        }
    }

    // Set the size of the instantiated object based on layer information
    void SetLayerSize(LayerInfo layer, GameObject instantiatedObject)
    {
        Vector3 newScale;
        // Check the number of elements in the output_shape
        if (layer.output_shape != null)                  
        {
            if (layer.output_shape.Length >= 3)
            {
                // Use output_shape values to adjust the size
                newScale = new Vector3(layer.output_shape[1] * 0.2f, layer.output_shape[2] * 0.2f, 1f);
            }
            else if (layer.class_name == "Dense")
            {
                // Use the height from output_shape and default width and depth
                float height = layer.output_shape.Length > 1 ? layer.output_shape[1] * 0.01f : defaultDenseHeight;
                newScale = new Vector3(defaultDenseWidth, height, defaultDenseDepth);
            }
            else if (layer.class_name == "Flatten")
            {
                // Use default size for Flatten layer
                newScale = defaultFlattenSize;
            }
            else
            {
                // Handle other cases if needed
                newScale = new Vector3(1f, 1f, 1f);
            }

            // Set the scale of the GameObject
            instantiatedObject.transform.localScale = newScale;

        }            
    }


    void HandleError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}

