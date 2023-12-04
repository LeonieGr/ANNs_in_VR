using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnClickArray : MonoBehaviour {
    public GameObject enableText;
    public GameObject [] disabledTexts;

   public void whenButtonClicked() {
    if(enableText.activeInHierarchy == false) {
        enableText.SetActive(true);
    }
    
    for (int i = 0; i < disabledTexts.Length; i++) {
        disabledTexts[i].SetActive(false);
    }
   }
}
