using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.Networking;
using System.Collections;


public class UIDebugLog : MonoBehaviour
{
    public TextMeshProUGUI DebugText;

    private Text exceptionText; // Reference to the Text component on your UI panel.

    private void Start()
    {
        // Set up the exception handler to catch unhandled exceptions.
        Application.logMessageReceived += HandleException;

        try
        {
            // Call the GetAPIData method that might throw an exception.
            StartCoroutine(SendWebRequest("http://172.22.31.194:4999/layer_info")); // Replace with your API URL.
        }
        catch (Exception e)
        {
            // Handle the exception by displaying it on the TextMeshPro Text component.
            if (exceptionText != null)
            {
                exceptionText.text = "Exception: " + e.Message;
            }
        }
    }

    private void HandleException(string logMessage, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            // Display the exception message on the TextMeshPro Text component.
            if (exceptionText != null)
            {
                exceptionText.text = logMessage + "\n" + stackTrace;
            }
        }
    }

    private IEnumerator SendWebRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                // Handle specific UnityWebRequest errors.
                string errorMessage = "UnityWebRequest error: " + request.error;
                Debug.LogError(errorMessage);

                // Display the error message in the error Text component.
                if (DebugText != null)
                {
                    DebugText.text = errorMessage;
                }
            }
            else
            {
                // Handle the response here when the request is successful.
                string responseMessage = "Web request succeeded. Response: " + request.downloadHandler.text;
                Debug.Log(responseMessage);

                // Display the response message in the response Text component.
                if (DebugText != null)
                {
                    DebugText.text = responseMessage;
                }
            }
        }
    }


    private void OnDestroy()
    {
        // Remove the exception handler when the script is destroyed.
        Application.logMessageReceived -= HandleException;
    }
}

