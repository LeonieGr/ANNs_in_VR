using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class ActivateTeleportationRay : MonoBehaviour
{
    public GameObject leftTeleportation;
    public GameObject rightTeleportation;

    public InputActionProperty leftActivate;
    public InputActionProperty rightActivate;

    //public InputActionProperty leftCancel;
   // public InputActionProperty rightCancel;

   // public XRRayInteractor leftRay;
  //  public XRRayInteractor rightRay;


    // activates the teleportation ray on controller input
    void Update()
    {
        //bool isLeftRayHovering = leftRay.TryGetHitInfo(out Vector3 leftPos, out Vector3 leftNormal, out int leftNumber, out bool leftValid);
        //leftTeleportation.SetActive(!isLeftRayHovering && leftCancel.action.ReadValue<float>() == 0 && leftActivate.action.ReadValue<float>() > 0.1f);
       
        leftTeleportation.SetActive(leftActivate.action.ReadValue<float>() > 0.1f);

       // bool isRightRayHovering = rightRay.TryGetHitInfo(out Vector3 rightPos, out Vector3 rightNormal, out int rightNumber, out bool rightValid);
       //rightTeleportation.SetActive(!isRightRayHovering && rightCancel.action.ReadValue<float>() == 0 && rightActivate.action.ReadValue<float>()> 0.1f);
       
        rightTeleportation.SetActive(rightActivate.action.ReadValue<float>()> 0.1f);

    }
}

