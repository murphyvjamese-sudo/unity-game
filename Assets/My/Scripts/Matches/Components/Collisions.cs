using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{  //single location to handle all sgs collisions.
    public List<GameObject> exceptions;  //default value is null, which actually makes this easier to ignore in the collisions system.
    public Size size;
    public Receive receive = new Receive();
    public Deliver deliver = new Deliver();

    //remember these variations on delivery and recieving for switching in and out of a state of being frozen. (When an object is frozen, it will deal no physical damage, yes freeze damage, and has weak defense stat. Everything else remains the same as what was granted in the inspector.)
    [HideInInspector] public bool swapToFrozenColliderFlag; //see Awake() where I set this to false
    [HideInInspector] public Receive rememberedOriginalReceive;
    [HideInInspector] public Deliver rememberedOriginalDeliver;
    [HideInInspector] public Receive frozenReceive = new Receive();
    [HideInInspector] public Deliver frozenDeliver = new Deliver();

    [System.Serializable] public class Receive
    {
        public Defense defense;
        public bool isFreezeImmune;
        public bool isPoisonImmune;
        public bool isConvertiveImmune;
        [HideInInspector] public int temporaryImmunityDurationShort;
        [HideInInspector] public int temporaryImmunityDurationLong;
        [HideInInspector] public int temporaryImmunityCounter;

        public enum Defense
        {
            Ignore = 2,
            Weak = 0,
            Strong = 1,
            VeryWeak = 3
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
            Strong = 2,
            VeryWeak = 3
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
        swapToFrozenColliderFlag = false; //CollisionSystem() will flip this to true to call SwapToFrozenCollider() at the end of the CollisionSystem(). If you mutate a collider mid-for loop in collision system. The order of collisions matters, and can cause unpredictable results based on order of array of objects in scene. Calling it afterwards removes this bug.
        receive.temporaryImmunityDurationShort = 5; //You want this for other collisions, so you can't "pass through" an enemy without getting hit. (This was a bug I thought I observed, though admittedly I might have imagined it or solved it some other way unintentionally. Either way, I feel confident that collisions do not suffer from this "pass through" bug)
        receive.temporaryImmunityDurationLong = receive.temporaryImmunityDurationShort * 5; //you want a longer cooldown for things like two force field objects colliding with each other, and hopefully passing through each other rather than collide twice in one pass. Same idea with payloads. If you destroy a small asteroid, you don't want the ship to collect the powerup dropped in the first pass. You also don't want the two small asteroids dropped by a large asteroid to collide with each other virtually as soon as they are spawned.
        receive.temporaryImmunityCounter = 0;

        //remember, these are reference types, since you don't need two diverging copies of the original data. You just need to remember the originals as is.
        rememberedOriginalReceive = receive;
        rememberedOriginalDeliver = deliver;

        DeepCopyWithFrozenModifications();
    }
    public void SwapToFrozenCollider()
    {
        Debug.Log("freeze swap -> frozen");
        deliver = frozenDeliver;
        receive = frozenReceive;
    }
    public void ReturnToNormalCollider()
    {
        Debug.Log("freeze swap -> normal");
        deliver = rememberedOriginalDeliver;
        receive = rememberedOriginalReceive;
    }
    private void DeepCopyWithFrozenModifications()
    {
        //DEEP COPY the frozen version of the collider based on what was assigned to the normal collider's receive and deliver in the inspector, with a few noted changed in comments below.
        //receive
        frozenReceive.defense = Receive.Defense.VeryWeak; //*changed
        frozenReceive.isFreezeImmune = true;
        frozenReceive.isPoisonImmune = receive.isPoisonImmune;
        frozenReceive.isConvertiveImmune = receive.isConvertiveImmune;
        frozenReceive.temporaryImmunityDurationShort = receive.temporaryImmunityDurationShort;
        frozenReceive.temporaryImmunityDurationLong = receive.temporaryImmunityDurationLong;
        frozenReceive.temporaryImmunityCounter = receive.temporaryImmunityDurationLong;
        //deliver
        frozenDeliver.damage = Deliver.Damage.VeryWeak; //*changed
        frozenDeliver.isFreeze = true; //*changed
        frozenDeliver.isPoison = deliver.isPoison;
        frozenDeliver.isConvertive = deliver.isConvertive;
    }
}
