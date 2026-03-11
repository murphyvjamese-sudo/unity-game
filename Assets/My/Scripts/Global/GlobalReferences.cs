using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalReferences : MonoBehaviour
{  //a list of constant values that persist across scene changes. Require a game object instance

    public GameObject mainCamera;  //establish this elsewhere in codebase as early as possible, so it can persist for rest of game.

    //enemies
    public GameObject InvasionFighter;
    public GameObject AsteroidLarge;
    public GameObject AsteroidSmall;
    public GameObject CometLarge;
    public GameObject CometSmall;
    public GameObject FrazpowMissile;
    public GameObject Terriloomer;
    public GameObject SpaceSquid;

    //projectiles
    public GameObject PlasmaCannon;
    public GameObject PoisonGlob;
    public GameObject ConversionRay;
    public GameObject FreezePulse;

    //powerups
    public GameObject ForceFieldPowerup;
    public GameObject JetPowerup;
    public GameObject Coin;

    //children/particles
    public GameObject PoisonAura;
    public GameObject JetCloud;
    public GameObject ForceField;
    public GameObject NavigatorIcon;
    public GameObject MechanicIcon;
    public GameObject PrincessIcon;
    public GameObject BountyHunterIcon;

    //sprites (for animations that choose a sprite based on conditions, as opposed to working from an animator controller)
    public Sprite targetReady;
    public Sprite targetLoading;
    public Sprite shrunkenJetCloud;

    //animations (choosing the animation or "skin" you want an object to use. For example, a different appearance for the player's ship depending on if they choose the armored, attacking, speedy, or good-handling ship.)
    public RuntimeAnimatorController playerSpeedy;
    public RuntimeAnimatorController playerTurnable;
    public RuntimeAnimatorController playerArmored;
    public RuntimeAnimatorController playerAttacking;

    //text, buttons, and menus
    public Sprite[] typography;  //defines the sprite for each letter in PixelBubble.png, to be used by Text.cs
    public Material pixelBubbleMaterial;
    public GameObject Text;
    public GameObject Button;
    public GameObject Points;
    public GameObject Title;

    //misc
    public GameObject BountyTarget; //the star that goes over the enemy that is marked as bounty, so you will recieve extra points for defeating it.
    public GameObject NavigatorX;  //the large x that hugs the side of the screen in the direction of the powerup found by navigator copilot until it can rest directly over said powerup
}
