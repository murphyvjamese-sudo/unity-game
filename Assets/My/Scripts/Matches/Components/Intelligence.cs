using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intelligence : MonoBehaviour
{
    public Team team;
    public bool isEnemySeeking;  //when true, this will move in the direction of enemy targets (and since projectiles don't have teams, I don't need to worry about them seeking after projectiles)

    public enum Team
    {
        Light = 0,
        Dark = 1
    }
}
