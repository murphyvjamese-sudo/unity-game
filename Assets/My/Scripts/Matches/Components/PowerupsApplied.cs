using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupsApplied : MonoBehaviour
{
    public int jetPowerupCounter = -1;
    public bool isForceFieldPoweruped;

    void Awake()
    {

    }
    public void ApplyJetPowerup()
    {
        jetPowerupCounter = GlobalValues.fps * 10;  //jet powerup will last for 10s
    }
}
