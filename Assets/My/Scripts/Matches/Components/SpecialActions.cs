using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialActions : MonoBehaviour
{
    //inspector interface
    public GameObject spawn;  //you might spawn a projectile, an area of effect, an invasion saucer, etc. Define the prefab here, and the system can handle the logical details of how exactly to instantiate it.
    public GameObject otherSpawn;
    public bool isMultiShot;
    public ReloadSpeed reloadSpeed;  //this is what you reset the timer to. Set once in the inspector

    //not shown in inspector
    [HideInInspector] public bool isPointingToOtherSpawn = false;  //toggle this to decide to spawn the "spawn" game object, or the "otherSpawn" game object 
    [HideInInspector] public int reloadCounter;  //this keeps track of time
    [HideInInspector] public int multiShotSpeed;
    [HideInInspector] public int multiShotCounter;
    [HideInInspector] public int shotsFired; //keeps track of if this is the first, second, or third shot fired for lasers and poison globs, which shoot a spray of 3 bullets each time player hits the button.

    public enum ReloadSpeed
    {
        Slow = 0,
        Medium = 1,
        Fast = 2
    }
    void Awake()
    {
        reloadCounter = 0;
        multiShotSpeed = 3;
    }
}
