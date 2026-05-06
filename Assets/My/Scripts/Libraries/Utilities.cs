using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public static class Utilities
{
    public static class Enums
    {
        public enum Trinary
        {
            Positive = 1,
            Neutral = 0,
            Negative = -1
        }
    }
    public static class Systems
    {
        public static GameObject[] Cleanse(GameObject[] entities)
        {  //use this to filter out the entities array of entities that require minimal processing but would bog down other more intensive systems. Example: remove all child object "letter" sprites from a text message after TextSystem() has run, so that these will not be considered by CollisionsSystem(), which has O(n^2) time complexity. This filtering can be achieved by many means, but the most common is to kick any objs with the empty cpnt "Cleanse" found on them.
            List<GameObject> result = new List<GameObject>(entities.Length);  // pre-allocate for performance
            foreach (GameObject e in entities)
            {
                if (e.GetComponent<Cleanse>() == null)
                {
                    result.Add(e);  // keep objects that do NOT have the Cleanse component
                }
            }
            return result.ToArray();  // only allocate a new array once
        }
    }
    public static class Searches
    {
        public static T[] FindByComponent<T>() where T : UnityEngine.Object
        {  //returns an array of all objects with a given component. If you just need to find one, such as when finding GlobalReferences, my advice is to simply use this method and assign your non array variable to the first element this method returns in its array.
            return GameObject.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
        public static GameObject FindNearestEnemy(Intelligence oIntelligence)
        {
            Intelligence[] intelligenceArray = GameObject.FindObjectsOfType<Intelligence>();
            List<Intelligence> intelligenceList = new List<Intelligence>(intelligenceArray);
            GameObject[] gameObjectArray;
            for (int i = intelligenceList.Count - 1; i >= 0; i--)
            {
                if (intelligenceList[i].team == oIntelligence.team)
                {  //remove candidates from list if they are on same team
                    intelligenceList.RemoveAt(i);
                }
            }
            gameObjectArray = new GameObject[intelligenceList.Count];
            for (int i = intelligenceList.Count - 1; i >= 0; i--)
            {  //convert the list of intelligences into an array of game objects
                gameObjectArray[i] = intelligenceList[i].gameObject;
            }
            //D\ebug.Log(oIntelligence.gameObject.name + " > " + FindNearestObject(oIntelligence.gameObject, gameObjectArray).name);
            return FindNearestObject(oIntelligence.gameObject, gameObjectArray);
        }
        public static GameObject FindNearestObject(GameObject o, GameObject[] objects)
        {
            float minDist = float.MaxValue;
            GameObject returnObj = null;
            foreach (GameObject obj in objects)
            {
                float thisDist = Math.Distance(o.transform.position.x, o.transform.position.y, obj.transform.position.x, obj.transform.position.y);
                if (thisDist < minDist)
                {
                    minDist = thisDist;
                    returnObj = obj;
                }
            }
            return returnObj;
        }
    }
    public static class Math
    {  //IMPORTANT: Most of these methods are redundant and unnecessary, as they are often defined on unity's built-in classes, like Mathf, UnityEngine.Random, Vector2, etc. I will probably keep these for now to avoid reworking a lot of my code, but in the future, I should avoid most of these math utilities unless they appear to be very game-specific.
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2));  //a^2 + b^2 = c^2
        }
        public static float WrapAngle(float angle)
        {
            angle = angle % (2 * Mathf.PI);  // Get the remainder within one full rotation
            if (angle < 0)
            {
                angle += 2 * Mathf.PI;  // If negative, wrap around to the positive range
            }
            return angle;
        }
        public static bool IsFastestAngleRouteCW(float x1, float y1, float theta1, float x2, float y2)
        {  //borrowed the idea for this from stack overflow, but apparently the idea is that you draw a line through the seeking object in the direction it is currently facing, and then see which side of that line the target it is chasing falls on
            float targetX = Mathf.Cos(theta1);
            float targetY = Mathf.Sin(theta1);
            if ((x1 - x2) * targetY > (y1 - y2) * targetX || x1 == x2 || y1 == y2)
            {
                return true;  //clockwise is the fastest angle route to take
            }
            else
            {
                return false;  //counterclockwise is the fastest angle route to take
            }
        }
        public static float AngleBetweenTwoPoints(Vector2 pointA, Vector2 pointB)
        {
            Vector2 direction = pointB - pointA;
            return Mathf.Atan2(direction.y, direction.x);
        }
        public static bool CompareAngles(float angleA, float angleB, float tolerance = 0.1f)
        {
            return Mathf.Abs(Mathf.DeltaAngle(angleA, angleB)) <= tolerance;
        }
    }

    public static class Probability
    {
        public static T PickOne<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Array cannot be null or empty");  //IMPORTANT: Don't try to return a null value or whatever in here. The rules are super weird with non-nullable T types. It is easier to find the use of this in your codebase, and ensure you only call it when a List with count > 0 exists
            }
            else
            {
                // Randomly select an index
                System.Random random = new System.Random();
                int randomIndex = random.Next(array.Length);  // Will return a number between 0 and array.Length - 1
                return array[randomIndex];
            }
        }
    }

    public static class Visual
    {
        public static void PlayAnimation(string animation, Animator animator)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
            {
                animator.Play(animation);
            }
            animator.speed = 1;
        }
        public static void PauseAnimation(Animator animator)
        {
            animator.speed = 0;
        }
        public static void DetermineReflection(SpriteRenderer sr, Kinematics kinematics)
        {
            if (kinematics != null && sr != null)
            {
                if (kinematics.direction < Mathf.PI / 2 || kinematics.direction > 3 * Mathf.PI / 2)
                {
                    sr.flipX = true;
                }
                else
                {
                    sr.flipX = false;
                }
            }
        }
    }

    public static void Heal(GameObject recipient)
    {
        Ailments rAilments = recipient.GetComponent<Ailments>();
        if (rAilments != null)
        {
            rAilments.freezeCounter = 1;  //but 0 is better here because unlike poison, the end result of freeze is a good thing. Even more complicated, I have to make this 1 for proper timing with the timing system logic! I should really have encapsulated this logic. What a maintenance nightmare to try and remember these details!
            rAilments.poisonCounter = -1;  //-1 ends the timer without causing the bad effect to happen
            rAilments.retainBountyWithPoison = false; //might be unnecessary, but I don't want an object to get healed from a bounty hunter player's poison, then get poisoned from something else and die rewarding the bounty hunter.
        }

        //remove any visual effects:
        GameObject poisonAura = null;
        Identification[] ids = recipient.GetComponentsInChildren<Identification>();
        foreach(Identification id in ids)
        { //unity quirk. GetComponentsInChildren checks the parent first, so I have to exclude the parent to get the child, since both have an Identification cpnt
            if(id.name == Identification.Name.PoisonAura)
            {
                poisonAura = id.gameObject;
            }
        }
        if (poisonAura != null)
        { //remove poison aura if healed
            GameObject.Destroy(poisonAura);
        }
    }

    public static void ResetControllerInputs(Controller controller)
    {  //assumes you already checked that this will have a Controller cpnt attached to it.
        controller.isLeftButtonPressed = false;
        controller.isRightButtonPressed = false;
    }

    public static int MapTierValue(Identification.Value value)
    {  //each enemy has an Identification.value that essentially quantifies how difficult it is to face. This maps Identification.value to a utilizable integer
        switch (value)
        {
            case Identification.Value.Weakest:
                return 1;
            case Identification.Value.Weak:
                return 3;
            case Identification.Value.Strong:
                return 5;
            case Identification.Value.Strongest:
                return 9;
        }
        return 0;
    }

    public static float MapSpeed(Kinematics kinematics)
    {
        float r = 0;
        float enemyCoefficient = .85f;  //adjust this if you want to make all enemies a bit faster or slower
        float playerCoefficient = .9f;  //adjust this if you want to make all player configurations a bit faster or slower
        switch (kinematics.speed)
        {
            case Kinematics.Speed.FastEnemy:
                r = 1.3f * enemyCoefficient;
                break;
            case Kinematics.Speed.FastPlayer:
                r = 1.5f * playerCoefficient;
                break;
            case Kinematics.Speed.MediumEnemy:
                r = .5f * enemyCoefficient;
                break;
            case Kinematics.Speed.MediumPlayer:
                r = 1.1f * playerCoefficient;
                break;
            case Kinematics.Speed.SlowEnemy:
                r = .45f * enemyCoefficient;
                break;
            case Kinematics.Speed.SlowPlayer:
                r = 1 * playerCoefficient;
                break;
            case Kinematics.Speed.Frazpow:
                r = 1.6f * enemyCoefficient;
                break;
            case Kinematics.Speed.Projectile:
                r = 2.315f;
                break;
            case Kinematics.Speed.None:
                r = 0;
                break;
        }
        return r;
    }

    public static float MapAcceleration(Kinematics kinematics)
    {
        float r = 0;
        float enemyCoefficient = .5f;  //adjust this if you want all levels of enemy to be a bit faster or slower
        float playerCoefficient = 1f;  //adjust this if you want all levels of player to be a bit faster or slower
        switch (kinematics.acceleration)
        {
            case Kinematics.Acceleration.FastEnemy:
                r = .8f * enemyCoefficient;
                break;
            case Kinematics.Acceleration.FastPlayer:
                r = 1.1f * playerCoefficient;
                break;
            case Kinematics.Acceleration.MediumEnemy:
                r = .6f * enemyCoefficient;
                break;
            case Kinematics.Acceleration.None:
                r = 0;
                break;
            case Kinematics.Acceleration.SlowEnemy:
                r = .4f * enemyCoefficient;
                break;
            case Kinematics.Acceleration.SlowPlayer:
                r = .8f * playerCoefficient;
                break;
        }
        return r / 20;
    }

    public static float MapSize(Collisions collisions)
    {
        float r = 0;
        switch (collisions.size)
        {
            case Collisions.Size.Small:
                r = 3;
                break;
            case Collisions.Size.Medium:
                r = 4;
                break;
            case Collisions.Size.Large:
                r = 10;
                break;
            case Collisions.Size.ExtraLarge:
                r = 20;
                break;
        }
        return r;
    }

    public static int MapReloadSpeed(SpecialActions sa)
    {
        int r = 0;
        switch (sa.reloadSpeed)
        {
            case SpecialActions.ReloadSpeed.Slow:
                r = 300;  //6s at 50fps
                break;
            case SpecialActions.ReloadSpeed.Medium:
            //old was 100 (2s)
                r = 125;  //2.5s at 50fps
                break;
            case SpecialActions.ReloadSpeed.Fast:
            //old was 75 (1.5s)
                r = 50;  //1s at 50fps
                break;
        }
        return r;
    }

    public static void PerformSpecialAction(GameObject e)
    {
        void PoisonException(GameObject root, GameObject spawn)
        { //poison globs usually shoot at a slower speed when originating from space squid. However, when they originate from a player that has a jet powerup equipped, they must travel faster or else they will just instantly hit the player.
            Kinematics spawnKinematics = spawn.GetComponent<Kinematics>();
            Identification parentIdentification = root.GetComponent<Identification>();
            if(spawnKinematics != null && spawnKinematics.speed == Kinematics.Speed.Frazpow && parentIdentification != null && parentIdentification.name == Identification.Name.Player)
            { //poison globs coincidentally go at same speed as frazpow missiles by default from space squid, but they must go at standard projectile speed when a player shoots them.
                spawnKinematics.speed = Kinematics.Speed.Projectile;
            }
        }
        SpecialActions eSA = e.GetComponent<SpecialActions>();
        Kinematics eKinematics = e.GetComponent<Kinematics>();
        Collisions eCollisions = e.GetComponent<Collisions>();
        Intelligence eIntelligence = e.GetComponent<Intelligence>();
        GameObject selectedSpawn;
        if (eSA.isPointingToOtherSpawn)
        {
            selectedSpawn = GameObject.Instantiate(eSA.otherSpawn);
            PoisonException(e, selectedSpawn);
        }
        else
        {
            selectedSpawn = GameObject.Instantiate(eSA.spawn);
            PoisonException(e, selectedSpawn);
        }
        if (eSA.spawn != null && eSA.otherSpawn != null)
        {
            eSA.isPointingToOtherSpawn = !eSA.isPointingToOtherSpawn;  //toggle between which special action you want to do. (Think terriloomer)
        }
        if(e.GetComponent<Copilot>() != null && e.GetComponent<Copilot>().copilot == GlobalState.PlayerConfiguration.Copilot.BountyHunter && selectedSpawn.GetComponent<Projectiles>())
        { //if this is the player, and it is shooting a projectile, make sure that projectile is able to communicate that it is from the player, so that a bounty hunter copilot can get it's reward.
            Debug.Log("shot from bounty hunter");
            selectedSpawn.GetComponent<Projectiles>().isShotFromPlayer = true;
        }
        if (selectedSpawn != null && selectedSpawn.GetComponent<Kinematics>() != null && selectedSpawn.GetComponent<Collisions>() && eKinematics != null)
        {
            Kinematics selectedSpawnKinematics = selectedSpawn.GetComponent<Kinematics>();
            Collisions selectedSpawnCollisions = selectedSpawn.GetComponent<Collisions>();
            Intelligence selectedSpawnIntelligence = selectedSpawn.GetComponent<Intelligence>();
            Identification selectedSpawnIdentification = selectedSpawn.GetComponent<Identification>();
            float collisionAvoidance = MapSize(selectedSpawnCollisions) + MapSize(eCollisions) + 1; //spawn the child ahead of whatever direction the parent is facing by a little more than the sum of their two sizes to avoid immediate collisions.
            if (collisionAvoidance > 15)
            {
                collisionAvoidance = 0;  //if this is an AOE, like freeze pulse, position it at exactly the same position as the ship.
            }
            selectedSpawnKinematics.direction = eKinematics.direction;  //have spawn face in same direction as parent
            selectedSpawn.transform.position = new Vector2(e.transform.position.x + Mathf.Cos(eKinematics.direction) * collisionAvoidance, e.transform.position.y + Mathf.Sin(eKinematics.direction) * collisionAvoidance);
            if(selectedSpawnIntelligence != null && eIntelligence != null)
            {
                selectedSpawnIntelligence.team = eIntelligence.team;
            }
            if (selectedSpawnIdentification != null && selectedSpawnIdentification.name == Identification.Name.FreezePulse || selectedSpawnIdentification.name == Identification.Name.ConvertivePulse)
            {  //if this is a freeze pulse (or perhaps include other specific types of game objects?)
                if (selectedSpawnCollisions.exceptions == null)
                {
                    //note that the log below never seems to run, which I did not anticipate. Perhaps Lists are not instantiated to null? Idk for sure, but since it ultimately works as expected by just skipping this block, I will go with it for now.
                    //D\ebug.Log("init list");
                    selectedSpawnCollisions.exceptions = new List<GameObject>();
                }
                selectedSpawnCollisions.exceptions.Add(e);  //make it so the thing you spawn will ignore you as a potential collision. Useful for things like freeze AOE
            }
        }
    }
    public static void SwitchTeam(GameObject e)
    {
        Intelligence eIntelligence = e.GetComponent<Intelligence>();
        if (eIntelligence != null)
        {
            if (eIntelligence.team == Intelligence.Team.Dark)
            {
                eIntelligence.team = Intelligence.Team.Light;
            }
            else
            {
                eIntelligence.team = Intelligence.Team.Dark;
            }
        }
    }
}
