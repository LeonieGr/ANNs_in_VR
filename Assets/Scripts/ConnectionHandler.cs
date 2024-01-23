using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ConnectionHandler : MonoBehaviour
{
    public GameObject successObject;
    public GameObject failureObject;
    private string urlToCheck; //= "http://192.168.2.104:4999";


    private void Start() {

        // Fetch URL from the main script
        GetApiData apiDataScript = FindObjectOfType<GetApiData>();
        urlToCheck = apiDataScript.ApiUrl;

        StartCoroutine(CheckConnection());
    }

    private IEnumerator CheckConnection() {
        // Create a UnityWebRequest object to send a request to the URL
        UnityWebRequest www = UnityWebRequest.Get(urlToCheck);

        // Send the request and wait for it to complete
        yield return www.SendWebRequest();

        // Check if there was an error with the request
        if (www.result == UnityWebRequest.Result.Success) {
            // Connection is successful, enable the successObject
            successObject.SetActive(true);
            failureObject.SetActive(false);
        } else {
            // Connection is unsuccessful, enable the failureObject
            successObject.SetActive(false);
            failureObject.SetActive(true);
        }
    }
}
