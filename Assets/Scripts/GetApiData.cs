using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// GetApiData fetches and visualizes layer information from a specified API endpoint
public class GetApiData : MonoBehaviour
{

    private string apiUrl = "http://172.22.26.233:4999/layer_info"; 

    // Prefabs for different types of layers
    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject TestObject;
    

    // Conversion factors to translate layer dimensions into Unity units
    private float pixelToUnit = 0.04f;
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

    foreach (LayerInfo layer in layers)
    {
        GameObject layerParent = CreateLayerParent(layer, zPosition);
        instantiatedLayers.Add(layerParent);
        InstantiateLayerComponents(layer, layerParent);
        UpdateLayerDepth(ref annDepth, layerParent, spaceBetweenLayers, ref zPosition);
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

void InstantiateLayerComponents(LayerInfo layer, GameObject layerParent)
{
    if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
    {
        if (IsDenseDropoutFlattenLayer(layer))
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
    float verticalSpacing = 0.15f;

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



    // Set layer size based on the layers output shape
   /* void SetLayerSize(LayerInfo layer, GameObject instantiatedObject)
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
            if (layer.class_name == "Flatten")
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
        } */


    void HandleError(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}

