using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematics : MonoBehaviour
{  //determines how objects move
    public Speed speed;  //How fast the object can move in a straight line. The actual number value for this is calculated in MatchManager.EstablishSpeed()
    public Acceleration acceleration;  //how fast the object can turn (change direction). The actual number value for this is calculated in MatchManager.EstablishAcceleration()
    [HideInInspector] public float direction;  //angle representing direction the obj is facing.

    public enum Speed
    {
        None = 0,
        SlowEnemy = 1,  //think space squid and terriloomer
        MediumEnemy = 2,  //think meteors
        FastEnemy = 3,  //think invasion saucer and frazpow missile
        SlowPlayer = 4,  //base player
        MediumPlayer = 5,  //player with speed upgrade from menu or speed boost powerup
        FastPlayer = 6,  //player with speed upgrade and powerup
        Projectile = 7,  //projectiles will be the fastest thing in the game, this does not include frazpow missile.
        Frazpow = 8
    }
    public enum Acceleration
    {
        None = 0,  //think asteroids and projectiles
        SlowEnemy = 1,  //think invasion fighter and frazpow missile?
        MediumEnemy = 2,  //think terriloomer?
        FastEnemy = 3,  //think space squid?
        SlowPlayer = 4,  //base player
        FastPlayer = 5,  //player with turning upgrade from menu
    }
}
