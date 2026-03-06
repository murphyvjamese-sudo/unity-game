using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{  //handles what happens when an object is destroyed.
    [HideInInspector] public bool isDeathFlagged;  //set this to true to indicate to Systems.Death() that this object should be destroyed.
    public int points;  //how many points are rewarded for destroying this object.
    
    //IMPORTANT: systems use this array in a pretty touchy way. Every enemy should have 4 elements in this array, where the first is for a potential explosion upon death, the middle two are for small comets/asteroids that might get spawned, and the last index is for a powerup that might get dropped.
    public GameObject[] payload;  //what gets dropped after this object is destroyed. BUGS? Using an array was a horrible design choice here. You don't know how many things will be in a death payload, so you should really use a list. However, this is integrated deeply enough, with few enough future dev to implement on top of it that I will not try to fix the underlying issue. But next time be more careful!
}
