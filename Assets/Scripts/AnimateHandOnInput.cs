using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;

    public Animator handAnimator;


    // anmiation of hands for the trigger and grip action
    void Update()
    {
       float triggerValue = pinchAnimationAction.action.ReadValue<float>();
       handAnimator.SetFloat("Trigger", triggerValue);
       
       float gripValue = gripAnimationAction.action.ReadValue<float>();
       handAnimator.SetFloat("Grip", gripValue);

       //Debug.Log(triggerValue);
       
    }
}