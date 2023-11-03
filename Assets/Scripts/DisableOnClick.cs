using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnClick : MonoBehaviour
{
    public GameObject disableText;
    public GameObject enableText;


   public void whenButtonClicked()
   {
    if(disableText.activeInHierarchy == true)
    {
        disableText.SetActive(false);
    }

    if(enableText.activeInHierarchy == false)
    {
        enableText.SetActive(true);
    }
   }
}
