using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Copilot : MonoBehaviour
{  //defines passive abilities for player, essentially, based on who they select from the menu to be their copilot.

    public GlobalState.PlayerConfiguration.Copilot copilot;  //Identification.Start() will assign this value, and default it to "none" if applicable

    //x timer is long, and for princess, while y timer is short, for mechanic and bounty hunter.
    [HideInInspector] public float outerRimRadius;
    [HideInInspector] public int counter;
    [HideInInspector] public int duration;
    [HideInInspector] public bool isLongDuration;  //if true, make copilot counter long, if false, make counter short. (Princess will have a long timer for calling backup, while )
    [HideInInspector] public bool isActionReady;  //this flags true when counter hits 0, signifying to external scripts that you can perform the copilots' special passive action
    [HideInInspector] public bool isStartTimerReady;  //this flags true when you are ready to start the timer before the copilot can enact their special action.

    void Awake()
    {
        /*World world;
        try
        {
            world = Utilities.Searches.FindByComponent<World>()[0];
        }
        catch
        {
            world = null;
        }*/
        try
        {
            outerRimRadius = Utilities.Searches.FindByComponent<World>()[0].spawnRadius;
        }
        catch
        {
            Debug.LogWarning("Jim. Should know world radius to decide where to spawn copilot stuff");
        }
    }
}
