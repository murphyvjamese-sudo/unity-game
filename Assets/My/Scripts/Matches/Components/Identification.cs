using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identification : MonoBehaviour
{
    public Name name;
    public Value value;  //how quantifies how dangerous an enemy is

    public enum Name
    {
        Player = 0,
        InvasionFighter = 1,
        FrazpowMissile = 2,
        SpaceSquid = 3,
        Terriloomer = 4,
        AsteroidLarge = 5,
        AsteroidSmall = 6,
        CometLarge = 7,
        CometSmall = 8,
        PlasmaCannon = 9,
        PoisonGlob = 10,
        LaserBeam = 11,
        FreezePulse = 12,
        ConvertivePulse = 13,
        Explosion = 14,
        Background = 15,
        ForceField = 16,
        PoisonAura = 17,
        NavigatorX = 18,
        BountyTarget = 19,
    }
    public enum Value
    {
        Weakest = 0,  //small asteroids and comets
        Weak = 1,  //large asteroids and comets
        Strong = 2,  //invasion fighter, frazpow missile, and space squid
        Strongest = 3,  //terriloomer
        Unapplicable = 4  //force field child object is a good example, as it isn't an enemy with a difficulty value, but you often need to use this cpnt to identify it.
    }
    void Start()
    {
        void ChooseShipAppearance(GlobalState.PlayerConfiguration.ShipAppearance shipAppearance)
        {
            GlobalReferences gr = FindObjectOfType<GlobalReferences>();
            RuntimeAnimatorController skin;
            if (GetComponent<Animator>() != null && gr != null)
            {
                skin = GetComponent<Animator>().runtimeAnimatorController;
                switch (shipAppearance)
                {
                    case GlobalState.PlayerConfiguration.ShipAppearance.Speed:
                        skin = gr.playerSpeedy;
                        break;
                    case GlobalState.PlayerConfiguration.ShipAppearance.Turns:
                        skin = gr.playerTurnable;
                        break;
                    case GlobalState.PlayerConfiguration.ShipAppearance.Armor:
                        skin = gr.playerArmored;
                        break;
                    case GlobalState.PlayerConfiguration.ShipAppearance.Attack:
                        skin = gr.playerAttacking;
                        break;
                    default:
                        skin = null;
                        break;
                }
                GetComponent<Animator>().runtimeAnimatorController = skin;
            }
        }
        void UpgradeShip(GlobalState.PlayerConfiguration.ShipUpgrade[] upgrades)
        {
            foreach (GlobalState.PlayerConfiguration.ShipUpgrade upgrade in upgrades)
            {
                //BUGS?: Note that I did not try every possible combination for what can be filled in for this array, so there could be issues.
                Kinematics pKinematics = GetComponent<Kinematics>();
                if (upgrade == GlobalState.PlayerConfiguration.ShipUpgrade.Armor)
                {
                    Collisions pCollisions = GetComponent<Collisions>();
                    if (pCollisions != null)
                    {
                        pCollisions.receive.defense = Collisions.Receive.Defense.Strong;
                    }
                }
                else if (upgrade == GlobalState.PlayerConfiguration.ShipUpgrade.Attack)
                {
                    SpecialActions pSpecialActions = GetComponent<SpecialActions>();
                    if (pSpecialActions != null)
                    {
                        pSpecialActions.reloadSpeed = SpecialActions.ReloadSpeed.Fast;
                    }
                }
                else if (upgrade == GlobalState.PlayerConfiguration.ShipUpgrade.Speed)
                {
                    if (pKinematics != null)
                    {
                        pKinematics.speed = Kinematics.Speed.MediumPlayer;  //not fast because you need something faster for when you collect the powerup. However, Kinematics.acceleration only has fast and slow.
                    }
                }
                else if (upgrade == GlobalState.PlayerConfiguration.ShipUpgrade.Turns)
                {
                    if (pKinematics != null)
                    {
                        pKinematics.acceleration = Kinematics.Acceleration.FastPlayer;  //unlike kinematics.speed, acceleration only has fast and slow
                    }
                }
                //note that the ShipUpgrade can also be set to None, in which case you just ignore this iteration, basically.
            }
        }

        GlobalState gs = FindObjectOfType<GlobalState>();
        GlobalReferences gr = FindObjectOfType<GlobalReferences>();
        if (gs != null && name == Name.Player)
        {  //try to initialize a player based on the player configurations established in the main menus and stored in global state
            GlobalState.PlayerConfiguration pConfig = gs.playerConfiguration;
            SpecialActions pSpecialActions = GetComponent<SpecialActions>();
            Copilot pCopilot = GetComponent<Copilot>();

            ChooseShipAppearance(pConfig.shipAppearance);
            UpgradeShip(pConfig.upgrades);  //give one or two upgrades to the ship's body (armor, speed, attack, etc.)

            //allow the player to decide which ammo to use
            if (pSpecialActions != null && gr != null)
            {
                if (pConfig.ammo == GlobalState.PlayerConfiguration.Ammo.Cannon)
                {
                    pSpecialActions.spawn = gr.PlasmaCannon;
                    pSpecialActions.isMultiShot = false;
                }
                else if (pConfig.ammo == GlobalState.PlayerConfiguration.Ammo.Conversion)
                {
                    pSpecialActions.spawn = gr.ConversionRay;
                    pSpecialActions.isMultiShot = false;
                }
                else if (pConfig.ammo == GlobalState.PlayerConfiguration.Ammo.Freeze)
                {
                    pSpecialActions.spawn = gr.FreezePulse;
                    pSpecialActions.isMultiShot = false;
                }
                else if (pConfig.ammo == GlobalState.PlayerConfiguration.Ammo.Poison)
                {
                    pSpecialActions.spawn = gr.PoisonGlob;
                    pSpecialActions.isMultiShot = true;
                }
                else if(pConfig.ammo == GlobalState.PlayerConfiguration.Ammo.Bullet)
                {
                    pSpecialActions.isMultiShot = true;
                }
            }

            //allow the player to pick a copilot
            if (pCopilot != null)
            {
                pCopilot.copilot = pConfig.copilot;
                if(pCopilot.copilot == GlobalState.PlayerConfiguration.Copilot.Princess)
                {
                    pCopilot.isLongDuration = true;
                    if (pCopilot.isLongDuration)
                    {
                        pCopilot.duration = GlobalValues.fps * 30;  //make princess wait 30s to call backup
                    }
                }
                else
                {
                    pCopilot.duration = GlobalValues.fps * 5;  //make navigator and bounty hunter wait 5s before finding a new target
                }
                pCopilot.counter = pCopilot.duration;
            }
        }
    }
}
