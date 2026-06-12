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
    }
}
