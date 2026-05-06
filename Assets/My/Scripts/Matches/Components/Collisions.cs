using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{  //single location to handle all sgs collisions.
    public List<GameObject> exceptions;  //default value is null, which actually makes this easier to ignore in the collisions system.
    public Size size;
    public Receive receive = new Receive();
    public Deliver deliver = new Deliver();

    [System.Serializable] public class Receive
    {
        public Defense defense;
        public bool isFreezeImmune;
        public bool isPoisonImmune;
        public bool isConvertiveImmune;
        [HideInInspector] public int temporaryImmunityDuration;
        [HideInInspector] public int temporaryImmunityCounter;

        public enum Defense
        {
            Ignore = 2,
            Weak = 0,
            Strong = 1
        }
    }
    [System.Serializable] public class Deliver
    {
        public Damage damage;
        public bool isFreeze;
        public bool isPoison;
        public bool isConvertive;

        public enum Damage
        {
            None = 0,
            Weak = 1,
            Strong = 2
        }
    }

    public enum Size
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        ExtraLarge = 3
    }

    void Awake()
    {
        receive.temporaryImmunityDuration = 5;
        receive.temporaryImmunityCounter = 0;
    }
}
