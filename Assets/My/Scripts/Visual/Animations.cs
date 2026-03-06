using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{  //every game obj that has an animator cpnt should have a script like this one that "controls" it. This game is simple enough that I plan to use this one script to handle all animation state transition logic for all animated objects in the game. Therefore, any time an animator component appears, this script should go along with it, as a dependency
    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer sr;
    [SerializeField] public Timer rotationTimer;
    /*DEPRACATED if replaced by Timer class: public float rotationSpeedSeconds = 0;  //allow this time duration to be specified in the inspector in seconds (easier on the design end)
    [HideInInspector] public int rotationSpeedFrames;  //but work with this time duration in frames (easier on the implementation end)
    [HideInInspector] public int rotationCounter;*/
    
    void Awake()
    {  //not entirely sure that this follows ECS, but I feel comfortable including it for now, since I am leveraging several of Unity's visual components
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rotationTimer = new Timer();
        /*DEPRACATED if replaced by Timer class: rotationSpeedFrames =  Mathf.RoundToInt(rotationSpeedSeconds * GlobalValues.fps);
        rotationCounter = rotationSpeedFrames;*/
    }
}
