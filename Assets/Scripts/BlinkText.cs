using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class BlinkText : MonoBehaviour
{
    
    public TextMeshProUGUI textToBlink;
    public float blinkInterval = 1.0f; // Time between blinks

    private void Start()
    {
        // Start the blinking coroutine when the script is enabled.
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
       while (true)
        {
            // Fade the text in
            for (float t = 0; t < 1; t += Time.deltaTime / (blinkInterval / 2))
            {
                Color newColor = textToBlink.color;
                newColor.a = Mathf.Lerp(0, 1, t);
                textToBlink.color = newColor;
                yield return null;
            }

            // Wait for the blinkInterval duration
            yield return new WaitForSeconds(blinkInterval / 2);

            // Fade the text out
            for (float t = 0; t < 1; t += Time.deltaTime / (blinkInterval / 2))
            {
                Color newColor = textToBlink.color;
                newColor.a = Mathf.Lerp(1, 0, t);
                textToBlink.color = newColor;
                yield return null;
            }

            // Wait for the blinkInterval duration
            yield return new WaitForSeconds(1);
        }
    }
}
