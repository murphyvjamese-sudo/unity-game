using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ailments : MonoBehaviour
{  //basically keeps track of a bunch of timers that can flag consequences related to them when they are reset (-1) or run out (0)
    [HideInInspector] public int poisonDuration = 350;  //this is how long each ailment will last for.
    [HideInInspector] public int freezeDuration = 175;
    public const int RESET = -1;  //you can set it to this to end a timer without having the bad consequence happen. For example, getting healed before poison can take you out.
    [HideInInspector] public int freezeCounter = RESET;
    [HideInInspector] public int poisonCounter = RESET;
    public int lifespan = RESET;  //OR, You can set this in the inspector if you want the object to die after a certain duration. For example, an AOE
    [HideInInspector] public bool retainBountyWithPoison; //set this to true if the PLAYER was the one who poisoned the target, so that bounty award will still be given accordingly. 
}
