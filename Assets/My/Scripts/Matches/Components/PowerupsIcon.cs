using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupsIcon : MonoBehaviour
{ //this cpnt is applied to the actual powerup you collect, whereas PowerupsApplied.cs is applied to the obj that has actually been powered up.
    public bool isDiscoveredByNavigator;  //set this to true when the navigator copilot spawns and tells you where to find this special kind of powerup
    public Name name;  //set in inspector
    [HideInInspector] public int value;  //quantifies how valuable a powerup is (and therefore how likely it is to be dropped by an enemy)

    public enum Name
    {
        Coin = 1,
        Jet = 2,
        ForceField = 3
    }

    void Awake()
    {
        if (name == Name.Coin)
        {
            value = 1;
        }
        else if(name == Name.Jet || name == Name.ForceField)
        {
            value = 3;
        }
    }
}
