using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDisableOnClick : MonoBehaviour
{
    public GameObject[] enabledTexts;
    public GameObject disableText;
    public GameObject button;

    public void whenButtonClicked()
    {
        Debug.Log("Button clicked");
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        string buttonTag = clickedButton.tag;

        Debug.Log("Clicked button tag: " + buttonTag);

        for (int i = 0; i < enabledTexts.Length; i++)
        {
            Debug.Log("Checking tag for enabledTexts[" + i + "]: " + enabledTexts[i].tag);

            if (enabledTexts[i].tag == buttonTag)
            {
                // Toggle the visibility of the enabled text associated with the clicked button.
                enabledTexts[i].SetActive(true);
            }
        }

        // Disable the disableText and the button GameObject for the currently clicked button.
        disableText.SetActive(false);
        button.SetActive(true);
    }
}


