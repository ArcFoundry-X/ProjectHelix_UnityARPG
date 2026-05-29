using System;
using Animancer;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public AnimationClip clip;
    
    private void Start()
    {
        GetComponent<Animator>().Play("Attack_03");
        Debug.Log("play ani");

        WindowsInputSystem windowsInputSystem = new WindowsInputSystem();
        windowsInputSystem.Gameplay.Enable();
        
        windowsInputSystem.Gameplay.Jump.performed += ctx =>
        {
            Debug.Log("jump");
        };
    }
}
