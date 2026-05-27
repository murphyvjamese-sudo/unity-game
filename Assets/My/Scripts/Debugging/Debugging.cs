using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugging : MonoBehaviour
{
    public bool isPlayerInvincible;  //make player invincible to damage, so you can debug a match without dying / ending the match

    void Awake()
    {

    }
    void FixedUpdate()
    {
        if (isPlayerInvincible)
        {
            Copilot player = FindObjectOfType<Copilot>();
            if (player != null)
            {
                Collisions pCollisions = player.GetComponent<Collisions>();
                if (pCollisions != null)
                {
                    pCollisions.receive.defense = Collisions.Receive.Defense.Ignore;
                    pCollisions.receive.isConvertiveImmune = true;
                    pCollisions.receive.isFreezeImmune = true;
                    pCollisions.receive.isPoisonImmune = true;
                }
            }
        }
    }
}
