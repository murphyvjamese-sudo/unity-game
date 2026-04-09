using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;

public class Systems : MonoBehaviour
{  //This script handles the logic for the actual gameplay, and facilitates the "systems" part of my ECS design, which I will only use for matches. (Menus will be a traditional OOP design). The one exception is animations. This will be handled in separate, traditional unity GOC scripts
    private GlobalReferences gr;
    private GlobalState gs;

    void Start()
    {
        gs = FindObjectOfType<GlobalState>();
        gr = FindObjectOfType<GlobalReferences>();
        gr.mainCamera = FindObjectOfType<CameraController>().gameObject;
    }
    void FixedUpdate()
    {  //NOTE: Order of system calls is likely important!
        if (gs != null && gr != null)
        {
            GameObject[] entities = FindObjectsOfType<GameObject>();  //recreating this array every frame could be costly. If so, you could save this array as a field of this class, and only update that field on frames when new game objects are created or destroyed. (For now, though, I will not incorporate that design, as it introduces a lot more boilerplate than I think is necessary for a simple game like this one, where I would like to minimize complexity of my codebase for the sake of readability and maintainability.)
            TextSystem(entities);  //in theory, this one should come first, BECAUSE I might want to remove all letter sprite child objects from the gameObjects array in order to improve performance for the subsequent systems, which can just ignore these.
            UserInputSystem(entities);
            IntelligenceSystem(entities);
            SpecialActionSystem(entities);
            KinematicSystem(entities);
            CollisionSystem(entities);
            TimerSystem(entities);
            DeathSystem(entities);
            WorldSystem(entities);
            AnimationSystem(entities);
            CopilotSystem(entities);
            MiscSystem(entities);
        }
        else
        {
            Debug.LogWarning("Jim. your systems depend on global state and global references being defined. Likely just need to add the Globals prefab into the scene");
        }
    }

    private void CopilotSystem(GameObject[] entities)
    {
        bool IsNavigatorPowerupFound()
        { //navigator checks if he has already found a powerup for you before finding another one
            PowerupsIcon[] powerups = Utilities.Searches.FindByComponent<PowerupsIcon>();  //find all powerups
            foreach(PowerupsIcon powerup in powerups)
            {  //find the one powerup that has been specially marked by the navigator
                if(powerup.isDiscoveredByNavigator)
                {
                    return true;
                }
            }
            return false;
        }
        bool SeekAndMarkIntelligentTarget()
        {  //bounty hunter searches for an intelligent target to potentially mark for double points, returning true if it finds one
            Intelligence[] intelligences = Utilities.Searches.FindByComponent<Intelligence>();  //find all intelligent objects
            List<Intelligence> enemyIntelligences = new List<Intelligence>();
            Intelligence target;
            foreach (Intelligence intelligence in intelligences)
            { //remove intelligent objects that are of the same team as you (light)
                if (intelligence.team == Intelligence.Team.Dark)
                {
                    enemyIntelligences.Add(intelligence);
                }
            }
            if(enemyIntelligences != null && enemyIntelligences.Count != 0)
            {
                target = Utilities.Probability.PickOne(enemyIntelligences.ToArray());  //pick a random intelligent enemy
                target.gameObject.AddComponent<HasBounty>();  //flag that intelligent enemy as your bounty target (the component allows you to easily identify this target anywhere and at any point in your codebase)
            }
            else
            {
                target = null;
            }

            //in addition to marking the parent game object as having a bounty, ensure you create a bounty target visual over the head of the object that has been marked.
            if(target != null)
            {
                GameObject bountyTarget = Instantiate(gr.BountyTarget);
                float size = 10;
                if(target.GetComponent<Collisions>() != null)
                { //establish how far above the enemy game object the star should be placed (higher for terriloomer than invasion fighter)
                    size = Utilities.MapSize(target.GetComponent<Collisions>());
                }
                bountyTarget.transform.parent = target.transform;
                bountyTarget.transform.localPosition = new Vector2(bountyTarget.transform.position.x, bountyTarget.transform.position.y + size + 5);
                return true;
            }
            return false;
        }
        bool IsBountyTargetFound()
        { //bounty hunter also makes sure there isn't already a bounty target out there before finding a new one.
            if(FindAnyObjectByType<HasBounty>() != null)
            {
                return true;
            }
            return false;
        }
        void SpawnRoyalBackup(float playerX, float playerY, float playerDirection, float outerRimRadius) //player direction represents which way the player is currently facing, so that allies will spawn behind the player.
        { //princess spawns 3 friendly allies behind the position she is facing at the "outer rim" every 30s
            GameObject[] Allies = { gr.SpaceSquid, gr.InvasionFighter, gr.FrazpowMissile };
            for(int i = 0; i < 3; i++)
            {  //spawn 3 allies
                GameObject ally = Instantiate(Utilities.Probability.PickOne(Allies));  //pick a random type of ally to call into battle
                float shift = 0;  //first ally will spawn directly behind player
                if (i == 1)
                {  //second ally will spawn behind and a little to one side of the player
                    shift = .5f;
                }
                else if(i ==2)
                { //third ally will spawn behind and a little to the other side of the player.
                    shift = -.5f;
                }
                ally.transform.position = new Vector2(playerX + Mathf.Cos(playerDirection - Mathf.PI + shift) * outerRimRadius, playerY + Mathf.Sin(playerDirection - Mathf.PI+ shift) * outerRimRadius);  //actually position enemy roughly behind the player, accounting for a potential shift to the left or right
                if(ally.GetComponent<Intelligence>() != null)
                {
                    Intelligence allyIntelligence = ally.GetComponent<Intelligence>();
                    allyIntelligence.team = Intelligence.Team.Light;
                }
                if(ally.GetComponent<Kinematics>() != null)
                {
                    Kinematics allyKinematics = ally.GetComponent<Kinematics>();
                    allyKinematics.direction = playerDirection;
                }
            }
        }
        void SpawnNavigatorPowerupAndX(float outerRimRadius, Vector2 playerPosition)
        { //spawn a specially marked "navigator powerup" at a random location on the "outer rim", as well as a largeish translucent x that will show on the screen in the direction where you need to go to find it, until it can rest directly over the onscreen navigator powerup
            GameObject[] powerups = { gr.ForceFieldPowerup, gr.JetPowerup };
            GameObject navigatorPowerup;
            GameObject navigatorX = Instantiate(gr.NavigatorX);

            //choose a random powerup
            navigatorPowerup = Instantiate(Utilities.Probability.PickOne(powerups));
            navigatorPowerup.GetComponent<PowerupsIcon>().isDiscoveredByNavigator = true;
            navigatorPowerup.transform.position = new Vector2(playerPosition.x + Mathf.Cos(UnityEngine.Random.Range(0, Mathf.PI * 2)) * outerRimRadius, playerPosition.y + Mathf.Sin(UnityEngine.Random.Range(0, Mathf.PI * 2)) * outerRimRadius);  //position somewhere randomly along the outer rim
        }
        void ShowIcon(GlobalState.PlayerConfiguration.Copilot copilot, GameObject pilot)
        {
            GameObject icon;
            switch (copilot)
            {
                case GlobalState.PlayerConfiguration.Copilot.BountyHunter:
                    icon = Instantiate(gr.BountyHunterIcon);
                    break;
                case GlobalState.PlayerConfiguration.Copilot.Princess:
                    icon = Instantiate(gr.PrincessIcon);
                    break;
                case GlobalState.PlayerConfiguration.Copilot.Navigator:
                    icon = Instantiate(gr.NavigatorIcon);
                    break;
                case GlobalState.PlayerConfiguration.Copilot.Mechanic:
                    icon = Instantiate(gr.MechanicIcon);
                    break;
                default:
                    icon = null;
                    break;
            }
            icon.transform.parent = pilot.transform;
            icon.transform.localPosition = new Vector2(icon.transform.position.x + 5, icon.transform.position.y + 10);
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Copilot>() != null)
            {
                Copilot eCopilot = e.GetComponent<Copilot>();
                //the following if/elses for each PlayerConfiguration.Copilot value are used to capture the "event" (conditions) that triggers a copilot's special ability (these of course are passive special abilities, not directly initiated with a controller button)
                if (eCopilot.copilot == GlobalState.PlayerConfiguration.Copilot.Princess)
                { //if x time has passed, have the princess call in backup fighters.
                    if (eCopilot.counter > 0)
                    {
                        eCopilot.counter--;
                    }
                    else if (eCopilot.counter == 0)
                    {
                        Kinematics eKinematics = e.GetComponent<Kinematics>();
                        float direction = 0;
                        if (eKinematics != null)
                        {
                            direction = eKinematics.direction;
                        }
                        ShowIcon(eCopilot.copilot, eCopilot.gameObject); //show the crown icon above the player's ship to visually indicate that their copilot princess has done something.
                        SpawnRoyalBackup(e.transform.position.x, e.transform.position.y, direction, eCopilot.outerRimRadius);
                        eCopilot.counter = eCopilot.duration;
                    }
                }
                else if (eCopilot.copilot == GlobalState.PlayerConfiguration.Copilot.Mechanic && eCopilot.GetComponent<Ailments>() != null)
                { //if a poison or freeze timer is halfway out, have the mechanic fix your ship
                    if (eCopilot.GetComponent<Ailments>() != null)
                    {
                        Ailments eAilments = eCopilot.GetComponent<Ailments>();
                        float fixDelay = eAilments.freezeDuration * .5f;  //only fix the ailment after a certain percentage (decimal) of the full duration of freeze duration has passed.
                        if ((eAilments.freezeCounter < eAilments.freezeDuration - fixDelay && eAilments.freezeCounter > 2) || (eAilments.poisonCounter < eAilments.poisonDuration - fixDelay && eAilments.poisonCounter > 2))
                        {
                            ShowIcon(eCopilot.copilot, eCopilot.gameObject);
                            Utilities.Heal(eCopilot.gameObject);
                        }
                    }
                }
                else if (eCopilot.copilot == GlobalState.PlayerConfiguration.Copilot.Navigator)
                { //the navigator spawns a powerup at the edge of the game world every y seconds. After you collect that powerup (marked visually with an x), the navigator will wait another y seconds before finding another powerup at the edge of the game world.
                    if (!IsNavigatorPowerupFound() && eCopilot.counter == -1)
                    { //if a navigator powerup is unavailable, start the timer to find one
                        eCopilot.counter = eCopilot.duration;
                    }
                    if (eCopilot.counter >= 0)
                    {
                        eCopilot.counter--;
                    }
                    if (eCopilot.counter == 0)
                    {
                        ShowIcon(eCopilot.copilot, eCopilot.gameObject);
                        SpawnNavigatorPowerupAndX(eCopilot.outerRimRadius, eCopilot.gameObject.transform.position);
                    }
                }
                else if (eCopilot.copilot == GlobalState.PlayerConfiguration.Copilot.BountyHunter)
                { //the bounty hunter will wait y seconds before marking a random intelligent enemy onscreen with a star. If this enemy is destroyed by you, you will gain double points for it. Once there is no specially marked enemy in play, the bounty hunter will wait y seconds before picking another one to mark.
                    if (!IsBountyTargetFound() && eCopilot.counter == -1)
                    { //if there is no bounty target found, start the counter
                        eCopilot.counter = eCopilot.duration;
                    }
                    if (eCopilot.counter >= 0)
                    {
                        eCopilot.counter--;
                    }
                    if (eCopilot.counter == 0)
                    { //even after counter is finished, you still might have to wait until an intelligent target appears to hunt for bounty
                        if (SeekAndMarkIntelligentTarget())
                        {
                            ShowIcon(eCopilot.copilot, eCopilot.gameObject);
                            eCopilot.counter--;
                        }
                    }
                }
            }
            if (e.GetComponent<Identification>() != null && e.GetComponent<Identification>().name == Identification.Name.NavigatorX)
            { //also handle navigator x's behavior in this system.
                GameObject player = null;
                GameObject treasureGameObject = null;
                try
                {
                    PowerupsIcon[] powerups;
                    player = Utilities.Searches.FindByComponent<Copilot>()[0].gameObject;
                    powerups = Utilities.Searches.FindByComponent<PowerupsIcon>();
                    foreach (PowerupsIcon powerup in powerups)
                    {
                        if (powerup.isDiscoveredByNavigator)
                        {
                            treasureGameObject = powerup.gameObject;
                        }
                    }
                }
                catch
                {
                    //it is ok if you don't find the special powerup or the player, which are both needed to position the x mark visual.
                }
                if (treasureGameObject != null && player != null)
                { //if the treasure and the player are both in existence, the x mark will be drawn
                    Vector2 treasure = treasureGameObject.transform.position;
                    Vector2 hunter = player.transform.position;
                    Vector2 xMark = e.transform.position;
                    float screenExtent = GlobalValues.unitsInScreen / 2;
                    float xDist = 0;  //this tells how far the xMark is from the player. Since the player is always at the exact center of the screen, anytime the xMark is a screen's width away from the player, it will get clamped.
                    float yDist = 0;
                    xMark = treasure;
                    xDist = hunter.x - xMark.x;
                    yDist = hunter.y - xMark.y;
                    xMark.x = Mathf.Clamp(xMark.x, hunter.x - screenExtent, hunter.x + screenExtent);
                    xMark.y = Mathf.Clamp(xMark.y, hunter.y - screenExtent, hunter.y + screenExtent);
                    e.transform.position = xMark;
                }
                else
                { //destroy the xMark if either the player or the treasure do not exist.
                    Destroy(e);
                }
            }
        }
    }

    private void MiscSystem(GameObject[] entities)
    {
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<RepeatingBackground>() != null)
            { //handles moving background tiles to the opposite side of the screen to create illusion of infinitely scrolling background
                GameObject camera = null;
                try
                {
                    camera = FindObjectOfType<CameraController>().gameObject;
                }
                catch
                {
                    Debug.LogWarning("Jim. Shouldn't happen");
                }
                if (camera != null)
                {
                    Vector2 ePosition = e.transform.position;
                    Vector2 cameraPosition = camera.transform.position;
                    float extent = GlobalValues.unitsInScreen / 2 + GlobalValues.unitsInTile / 2;
                    float leftBound = cameraPosition.x - extent;
                    float rightBound = cameraPosition.x + extent;
                    float topBound = cameraPosition.y + extent;
                    float bottomBound = cameraPosition.y - extent;
                    if (ePosition.x < leftBound)
                    { //cycle back over to right side of screen
                        ePosition = new Vector2(ePosition.x + extent * 2, ePosition.y);
                    }
                    else if (e.transform.position.x > rightBound)
                    { //cycle back over to left side of screen
                        ePosition = new Vector2(ePosition.x - extent * 2, ePosition.y);
                    }
                    if (e.transform.position.y > topBound)
                    { //cycle back to bottom part of screen
                        ePosition = new Vector2(ePosition.x, ePosition.y - extent * 2);
                    }
                    else if (e.transform.position.y < bottomBound)
                    { //cycle back to top part of screen
                        ePosition = new Vector2(ePosition.x, ePosition.y + extent * 2);
                    }
                    e.transform.position = ePosition;  //have to do this because position is a struct (value type) whereas transform is a class (reference type)
                }
            }
        }
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Menu" && gs.currentPage == Sgs.Pages.None)
        { //if you have loaded the menu scene, but haven't loaded a page, load the desired page
            Sgs.NewMenuPage(gs.desiredPage);
        }
    }

    private async Task WorldSystem(GameObject[] entities)
    {
        void NextLevel(World eWorld)
        {  //time to go to the next level
            eWorld.level++;  //move on to next level
            switch (eWorld.level)
            { //rough reference: after about 1.5 minutes, you can start seeing Terriloomer.
                case 1:  //introduce AsteroidLarge and FrazpowMissile
                    //D\ebug.Log("lvl 1");
                    eWorld.index = 3;  //increasing index allows you to pull from more potential enemies to spawn from the parent array spawnSelections
                    eWorld.levelCounter = GlobalValues.fps * 20;
                    break;
                case 2:  //introduce InvasionFighter and CometSmall
                    //D\ebug.Log("lvl 2");
                    eWorld.index = 5;
                    eWorld.levelCounter = GlobalValues.fps * 30;
                    break;
                case 3:  //introduce SpaceSquid and CometLarge
                    //D\ebug.Log("lvl 3");
                    eWorld.index = 7;
                    eWorld.levelCounter = GlobalValues.fps * 30;
                    break;
                case 4:  //introduce Terriloomer
                    //D\ebug.Log("LVL 4");
                    eWorld.index = 8;
                    eWorld.levelCounter = GlobalValues.fps * 30;
                    break;
                default:  //after terriloomer, just keep making enemies spawn faster and faster until player eventually overwhelmed.
                    //D\ebug.Log("inf progression");
                    eWorld.levelCounter = GlobalValues.fps * 15;
                    break;
            }
        }
        void Spawn(World eWorld)
        {  //time to spawn a new enemy

            //define methods
            float LevelCoefficient(int level)
            {
                switch (level)
                {
                    case 0:
                        return 1f;
                    case 1:
                        return .75f;
                    case 2:
                        return .5f;
                    case 3:
                        return .4f;
                    case 4:
                        return .3f;
                    default:
                        return 1f / level;  //handles every level after the introduction of terriloomer in level 4, so .4, .33, .28. etc.
                }
            }

            //define vars
            GameObject[] subArray;
            GameObject spawn;
            Identification.Value spawnValue;
            float spawnAngle;  //determines from what direction the newly spawned object will appear.

            //pick something to spawn. (Sub array defines any possible encounters based on the level you are on, whereas eWorld.spawnSelections defines all enemies in the game.)
            subArray = new GameObject[eWorld.index];
            Array.Copy(eWorld.spawnSelections, 0, subArray, 0, eWorld.index);
            spawn = Utilities.Probability.PickOne(subArray);

            //create what you have chosen to spawn
            spawn = Instantiate(spawn);
            spawnAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            spawn.transform.position = new Vector2(eWorld.transform.position.x + Mathf.Cos(spawnAngle) * eWorld.spawnRadius, eWorld.transform.position.y + Mathf.Sin(spawnAngle) * eWorld.spawnRadius);  //pick a random spot on the circumfrence of eWorld's spawn-in radius
            if (spawn.GetComponent<Kinematics>() != null)
            {
                float sDirection = spawn.GetComponent<Kinematics>().direction;
                sDirection = Mathf.Repeat(spawnAngle + Mathf.PI, 2f * Mathf.PI);  //make the object face inward from the location on the perimeter from which it spawned
                sDirection = Mathf.Repeat(UnityEngine.Random.Range(sDirection - .5f, sDirection + .5f), 2f * Mathf.PI);  //but also vary this angle a small amount.
                spawn.GetComponent<Kinematics>().direction = sDirection;
            }

            //potentially add a powerup to be dropped when the enemy is defeated
            Identification sIdentification = spawn.GetComponent<Identification>();
            if(sIdentification != null)
            {

                //define the odds for the payload lottery
                string[] lottery = new string[12];
                string winner;  //string is used to pick a prefab from a switch statement
                GameObject WinnerPrefab;  //reference to a prefab (not instance!) in global references, which will be appended to the enemy's Death cpnt, and instantiated upon time of death.
                if (sIdentification.value == Identification.Value.Weakest)
                {
                    lottery[0] = "jet";
                    lottery[1] = "ff";
                    lottery[2] = "coin";  //or invisibility
                    lottery[3] = "coin";
                    for (int i = 0; i < lottery.Length; i++)
                    { //fill the rest of the slots with nothing.
                        if (lottery[i] == null)
                        {
                            lottery[i] = "none";
                        }
                    }
                }
                else if (sIdentification.value == Identification.Value.Weak || sIdentification.value == Identification.Value.Strong)
                {
                    lottery[0] = "jet";
                    lottery[1] = "jet";
                    lottery[2] = "ff";
                    lottery[3] = "ff";
                    lottery[4] = "coin";  //or invisibility
                    lottery[5] = "coin";  //or invisibility
                    lottery[6] = "coin";
                    lottery[7] = "coin";
                    for (int i = 0; i < lottery.Length; i++)
                    { //fill the rest of the slots with coins ?but I might take two of those slots for the invisibility powerup later if I decide to implement it.
                        if (lottery[i] == null)
                        {
                            lottery[i] = "none";
                        }
                    }
                }
                else if (sIdentification.value == Identification.Value.Strongest)
                {
                    lottery[0] = "jet";
                    lottery[1] = "jet";
                    lottery[2] = "jet";
                    lottery[3] = "ff";
                    lottery[4] = "ff";
                    lottery[5] = "ff";
                    lottery[6] = "coin";  //or invisibility
                    lottery[7] = "coin";  //or invisibility
                    lottery[8] = "coin";  //or invisibility
                    lottery[9] = "coin";
                    lottery[10] = "coin";
                    lottery[11] = "coin";
                }

                //actually pick an item from the payload lottery
                winner = lottery[UnityEngine.Random.Range(0, lottery.Length)];

                //instantiate the powerup (or lack thereof) won from the lottery
                switch (winner)
                {
                    case "ff":
                        WinnerPrefab = gr.ForceFieldPowerup;
                        break;
                    case "jet":
                        WinnerPrefab = gr.JetPowerup;
                        break;
                    case "coin":
                        WinnerPrefab = gr.Coin;
                        break;
                    default:
                        WinnerPrefab = null;
                        break;
                }

                //store the powerup in the enemy's Death cpnt, so that it will instantiate the powerup prefab (or lack thereof) upon death.
                Death sDeath = spawn.GetComponent<Death>();
                if(sDeath != null && sDeath.payload.Length == 4)
                {
                    /*Run this block if you want to see when a powerup is appended to a payload
                    if(winner != "none")
                    {
                        //D\ebug.Log("gotcha");
                    }*/
                    sDeath.payload[3] = WinnerPrefab;  //rather poor design, but relies upon the 3rd element in the array always being reserved for a powerup drop. That's why I add the condition in this enclosing block. If I try to access the 3rd index of an array that is not that large, it generates an out of bounds error that makes the app behave similar to an infinite loop
                }
            }
                
            //determine how much time before next spawn. (Spawn happens every time counter reaches 0, and the reset determines how long you wait based on how powerful the enemy is.)
            if (spawn.GetComponent<Identification>() != null)
            {
                spawnValue = spawn.GetComponent<Identification>().value;
            }
            else
            {
                Debug.LogWarning("Jim. Should always have spawn value defined");
                spawnValue = Identification.Value.Strong;
            }
            eWorld.nextSpawnCounter = Mathf.RoundToInt(Utilities.MapTierValue(spawnValue) * LevelCoefficient(eWorld.level) * GlobalValues.fps);
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<World>() != null)
                {
                    Copilot player = FindObjectOfType<Copilot>();  //creative way to find the player object. My thought is this will be more performant than using the Identification cpnt, as many objs have that cpnt but only the player has a copilot
                    World eWorld = e.GetComponent<World>();
                    eWorld.camera.gameObject.transform.position = new Vector3(eWorld.gameObject.transform.position.x, eWorld.gameObject.transform.position.y, eWorld.camera.gameObject.transform.position.z);  //position the camera at the center of the world, which is also centered around the player
                    if (player == null)
                    {
                        eWorld.gameOverCounter--;
                        if (eWorld.gameOverCounter == 0)
                        {
                            




                            if(gs.gameMode == Sgs.GameModes.GameA)
                            {
                                await Leaderboards.SyncLeaderboards(gs, "gameA", gs.currentScore);
                            }
                            else if(gs.gameMode == Sgs.GameModes.GameB)
                            {
                                await Leaderboards.SyncLeaderboards(gs, "gameB", gs.currentScore);
                            }
                            else if(gs.gameMode == Sgs.GameModes.GameC)
                            {
                                await Leaderboards.SyncLeaderboards(gs, "gameC", gs.currentScore);
                            }
                            SceneManager.LoadScene("Menu");  //scene loads at end of frame
                            //set current and desired pages, so that when scene loads on next frame, and Systems.MiscSystem() detects that currentPage == None, it will load the desired page.
                            gs.desiredPage = Sgs.Pages.GameOver;
                            gs.currentPage = Sgs.Pages.None;
                            gs.currentScore = 0;
                            eWorld.camera.transform.position = new Vector3(0, 0, -10);





                            /*string leaderboardId = "gameA";
                            int highScore = 0;
                            if(gs.gameMode == Sgs.GameModes.GameA)
                            {
                                highScore = gs.highScoreA;
                            }
                            else if(gs.gameMode == Sgs.GameModes.GameB)
                            {
                                highScore = gs.highScoreB;
                            }
                            else if(gs.gameMode == Sgs.GameModes.GameC)
                            {
                                highScore = gs.highScoreC;
                            }
                            SceneManager.LoadScene("Menu");  //scene loads at end of frame
                                                             //set current and desired pages, so that when scene loads on next frame, and Systems.MiscSystem() detects that currentPage == None, it will load the desired page.
                            gs.desiredPage = Sgs.Pages.GameOver;
                            gs.currentPage = Sgs.Pages.None;
                            if (gs.currentScore > highScore)
                            {
                                //DEPRACATED: gs.highScore = gs.currentScore;
                                if(gs.gameMode == Sgs.GameModes.GameA)
                                {
                                    gs.highScoreA = gs.currentScore;
                                    leaderboardId = "gameA";
                                }
                                else if(gs.gameMode == Sgs.GameModes.GameB)
                                {
                                    gs.highScoreB = gs.currentScore;
                                    leaderboardId = "gameB";
                                }
                                else if(gs.gameMode == Sgs.GameModes.GameC)
                                {
                                    gs.highScoreC = gs.currentScore;
                                    leaderboardId = "gameC";
                                }
                            }
                            SecurePlayerPrefs.SetInt(leaderboardId, gs.currentScore);
                            if(Application.internetReachability != NetworkReachability.NotReachable)
                            {  //if you have internet
                                //submit the score to unity leaderboards, but first check if a highscore was achieved offline, and submit that instead if it was
                                if(SecurePlayerPrefs.GetInt(leaderboardId) > gs.currentScore)
                                {
                                    Debug.Log("get offline score");
                                    gs.currentScore = SecurePlayerPrefs.GetInt(leaderboardId);
                                }
                                Debug.Log("set online score");
                                LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, gs.currentScore);  //don't need to await. Only way this could hurt you is if you visit the leaderboards page within your app before the server finishes recieving this new score, and then it use your old score in determining your rank. Not crucial, and highly unlikely you could even navigate to the new menu that quickly anyways
                            }
                            gs.currentScore = 0;
                            eWorld.camera.transform.position = new Vector3(0, 0, -10);*/
                        }
                    }
                    if (eWorld.levelCounter > 0)
                    {
                        eWorld.levelCounter--;
                    }
                    else
                    {
                        NextLevel(eWorld);
                    }
                    if (eWorld.nextSpawnCounter > 0)
                    {
                        eWorld.nextSpawnCounter--;
                    }
                    else
                    {
                        Spawn(eWorld);
                    }
                    foreach (GameObject f in entities)
                    {
                        if (f.GetComponent<Death>() != null && Vector2.Distance(f.transform.position, e.transform.position) > eWorld.deathRadius)
                        {  //also check (essentially every other entity - might need to qualify with something other than the death cpnt) to see if they are too far offscreen, and should thus die.
                            //old way... I think this causes you to recieve points each time an object goes offscreen, and also causes them to drop an explosion, which can briefly play only part of the explosion noise before getting cut off abruptly:    f.GetComponent<Death>().isDeathFlagged = true;
                            GameObject.Destroy(f);
                        }
                        if (f.GetComponent<Controller>() != null && !f.GetComponent<Controller>().isNPC)
                        {
                            e.transform.position = f.transform.position;  //have the player (entity f) always be at the very center of the world (entity e)
                        }
                    }
                    break;  //there will only ever be one world object in the scene, so after you have found and processed its component, it is wasteful to go looking for a second one.
                }
        }
    }

    private void CollisionSystem(GameObject[] entities)
    {  //does not rely on Collider2D component and associated events. In this ECS approach, e is the deliverer, and f is the receiver. 
        bool IsException(Collisions eCollisions, Collisions fCollisions)
        {  //allows you to mark certain entities to not be collided with via an array of references
           //recall the convention that e is deliverer, f is receiver
            if (eCollisions.exceptions != null)
            {  //fortunately, this foreach shouldn't tax performance too badly, since the above condition is rarely true. (There are usually not exceptions that would make this foreach have to run.)
                foreach (GameObject g in eCollisions.exceptions)
                {
                    if (g == fCollisions.gameObject)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        void AsteroidTransformation(GameObject asteroid)
        { //works for large or small asteroids, which get turned into comets upon being frozen.
            GameObject comet = null;
            Identification aIdentification = asteroid.GetComponent<Identification>();
            Kinematics aKinematics = asteroid.GetComponent<Kinematics>();
            Kinematics cKinematics;
            if (aIdentification != null)
            {
                if (aIdentification.name == Identification.Name.AsteroidLarge)
                {
                    comet = Instantiate(gr.CometLarge);
                }
                else if (aIdentification.name == Identification.Name.AsteroidSmall)
                {
                    comet = Instantiate(gr.CometSmall);
                }
            }
            comet.transform.position = asteroid.transform.position;
            cKinematics = comet.GetComponent<Kinematics>();
            if (aKinematics != null && cKinematics != null)
            {
                cKinematics.direction = aKinematics.direction;
            }
            Destroy(asteroid.gameObject);
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Collisions>() != null)
            {
                Collisions eCollisions = e.GetComponent<Collisions>();
                foreach (GameObject f in entities)
                {
                    if (f != e && f.GetComponent<Collisions>() != null && f.GetComponent<Death>() != null)
                    {  //both objs have collisions cpnt, so you can check to see if they collide.
                        Collisions fCollisions = f.GetComponent<Collisions>();
                        Death fDeath = f.GetComponent<Death>();
                        Ailments fAilments = f.GetComponent<Ailments>();
                        Identification fIdentification = f.GetComponent<Identification>();
                        if (IsException(eCollisions, fCollisions))
                        {
                            continue;  //if there is an exception, ignore this collision btwn e and f
                        }
                        if (Utilities.Math.Distance(e.transform.position.x, e.transform.position.y, f.transform.position.x, f.transform.position.y) <= Utilities.MapSize(eCollisions) + Utilities.MapSize(fCollisions))
                        {  //there is a collision
                           //sgs
                           bool ConsiderForceField()
                            { //you should call this before every negative collision scenario (poison, freeze, damage, conversion, etc) so that the force field will protect you instead of the usual effect taking place.
                                if (f.GetComponent<PowerupsApplied>() != null && f.GetComponent<PowerupsApplied>().isForceFieldPoweruped)
                                { //if there is a force field involved
                                    f.GetComponent<PowerupsApplied>().isForceFieldPoweruped = false; //remove the force field on a collisions logic level
                                    foreach (Transform child in f.transform)
                                    { //iterate through all of f's children
                                        if (child.gameObject.GetComponent<Identification>() != null && child.gameObject.GetComponent<Identification>().name == Identification.Name.ForceField)
                                        { //remove force field visual
                                            Destroy(child.gameObject);
                                        }
                                    }
                                    fCollisions.receive.temporaryImmunityCounter = fCollisions.receive.temporaryImmunityDuration; //set a cooldown so you don't just get hit by the exact same thing next frame
                                    return true;
                                }
                                fCollisions.receive.temporaryImmunityCounter = fCollisions.receive.temporaryImmunityDuration; //set a cooldown so you don't just get hit by the exact same thing next frame (must perform this inside and outside the above condition, to account for both function return paths)
                                return false;
                            }
                            if(e.GetComponent<Identification>() != null)
                            {
                                Identification eIdentification = e.GetComponent<Identification>();
                                if(eIdentification.name == Identification.Name.LaserBeam || eIdentification.name == Identification.Name.PlasmaCannon || eIdentification.name == Identification.Name.PoisonGlob)
                                { //if this is a projectile (poison glob, plasma cannon, laser beam), it should destroy itself if it collides with an object that has defensive characteristics. This allows bullets to not get destroyed by other bullets or pulses, but will get destroyed by "solid" if you will, things they collide with, independent of physical damage values.
                                    if(fCollisions.receive != null && fCollisions.receive.defense != Collisions.Receive.Defense.Ignore)
                                    {
                                        Destroy(e);
                                    }
                                }
                            }
                            if (fCollisions.receive.temporaryImmunityCounter == 0)
                            { //if there is no collision cooldown, consider all collision scenarios
                                if (fCollisions.receive.defense != Collisions.Receive.Defense.Ignore && (eCollisions.deliver.damage == Collisions.Deliver.Damage.Strong || eCollisions.deliver.damage == Collisions.Deliver.Damage.Weak && fCollisions.receive.defense != Collisions.Receive.Defense.Strong))
                                {  //if delivering damage, the receiver will be destroyed UNLESS it has strong defense and is facing a weak delivery. (Or has a force field)
                                    if (ConsiderForceField())
                                    {
                                        //if there was a force field, do nothing except what is defined in ConsiderForceField()
                                    }
                                    else
                                    { //else, perform the usual consequence for physical damage.
                                        fDeath.isDeathFlagged = true;
                                    }
                                }
                                else if(eCollisions.deliver.damage == Collisions.Deliver.Damage.Weak && fCollisions.receive.defense == Collisions.Receive.Defense.Strong)
                                { //related to the logic above. You still want to destroy force fields even if a strongly defended object collides with a weak attack.
                                    ConsiderForceField();
                                }
                                if (fAilments != null && fAilments.poisonCounter <= 0 && eCollisions.deliver.isPoison && !fCollisions.receive.isPoisonImmune)
                                {  //try to poison a target. It won't work if the target is immune or already poisoned.
                                    if (ConsiderForceField())
                                    {
                                        //if there was a force field, do nothing except what is defined in ConsiderForceField()
                                    }
                                    else
                                    { //else, perform the usual consequence for poison attacks
                                        GameObject poisonAura = Instantiate(gr.PoisonAura);  //create a poison aura to surround the target visually
                                        poisonAura.transform.parent = fAilments.transform;  //attach that poison aura's position to that of the target
                                        poisonAura.transform.localPosition = new Vector2(0, 0);
                                        fAilments.poisonCounter = fAilments.poisonDuration;
                                    }
                                }
                                if (eCollisions.deliver.isFreeze && !fCollisions.receive.isFreezeImmune)
                                {
                                    if (ConsiderForceField())
                                    {
                                        //if there was a force field, do nothing except what is defined in ConsiderForceField()
                                    }
                                    else
                                    { //else perform the usual consequence for freeze attacks
                                        if (fIdentification != null)
                                        {
                                            if (fIdentification.name == Identification.Name.AsteroidLarge)
                                            {
                                                //replace ?or transform? large asteroid into large comet
                                                AsteroidTransformation(fIdentification.gameObject);
                                            }
                                            else if (fIdentification.name == Identification.Name.AsteroidSmall)
                                            {
                                                //replace ?or transform? small asteroid into small comet
                                                AsteroidTransformation(fIdentification.gameObject);
                                            }
                                            else if (fAilments != null && fAilments.freezeCounter <= 0)
                                            {  //just do basic freeze functionality, unless already frozen.
                                                fAilments.freezeCounter = fAilments.freezeDuration;
                                            }
                                        }
                                    }
                                }
                                if (eCollisions.deliver.isConvertive && !fCollisions.receive.isConvertiveImmune)
                                {
                                    if (ConsiderForceField())
                                    {
                                        //if there was a force field, do nothing except what is defined in ConsiderForceField()
                                    }
                                    else
                                    { //else perform the usual consequence for conversion attacks
                                        Utilities.SwitchTeam(f);
                                    }
                                }
                                if (e.GetComponent<PowerupsIcon>() != null && f.GetComponent<PowerupsApplied>() && fCollisions.receive.temporaryImmunityCounter == 0)
                                { //even though outer block already checks for collision counter, the conditions above this can change that, so I must check again.
                                    PowerupsIcon ePowerupsIcon = e.GetComponent<PowerupsIcon>();
                                    PowerupsApplied fPowerupsApplied = f.GetComponent<PowerupsApplied>();
                                    if (ePowerupsIcon.name == PowerupsIcon.Name.Jet)
                                    {
                                        fPowerupsApplied.ApplyJetPowerup();
                                        Utilities.Heal(f);
                                    }
                                    else if (ePowerupsIcon.name == PowerupsIcon.Name.ForceField)
                                    {
                                        fPowerupsApplied.isForceFieldPoweruped = true;
                                        GameObject forceField = Instantiate(gr.ForceField);
                                        forceField.transform.parent = f.transform;
                                        forceField.transform.localPosition = new Vector2(0, -1);
                                        Utilities.Heal(f);
                                    }
                                    else if (ePowerupsIcon.name == PowerupsIcon.Name.Coin)
                                    {
                                        Utilities.Heal(f);
                                    }
                                    if (f.GetComponent<Identification>() != null && f.GetComponent<Identification>().name == Identification.Name.Player)
                                    { //if powerup collided with a player, print points. If it collides with an enemy, no points will be printed.
                                        Death death = e.GetComponent<Death>();
                                        if(death != null)
                                        {
                                            //print points
                                            GameObject points = Instantiate(gr.Points);
                                            points.transform.position = death.gameObject.transform.position;  //position points message where the thing that was destroyed died
                                            points.GetComponent<Text>().message = death.points.ToString();  //print however many points the object was worth
                                        
                                            //add points to highscore
                                            gs.currentScore += death.points;
                                        }
                                    }
                                    Destroy(e);
                                }
                            }
                        }
                        if (e.GetComponent<Identification>() != null)
                        { //this block is meant to ensure that pulses can only deliver collisions for one frame. Since I don't have a good cooldown mechanism, (and don't want to implement one) this allows me to preserve the visual of the AOE surviving for a few frames without quickly converting and unconverting targets, resulting in a scenario where you could hit an enemy with a convertive pulse and it will end up remaining on its original team.
                            Identification eIdentification = e.GetComponent<Identification>();
                            if (eIdentification.name == Identification.Name.ConvertivePulse || eIdentification.name == Identification.Name.FreezePulse)
                            {
                                Destroy(e.GetComponent<Collisions>());
                            }
                        }
                    }
                }
            }
        }
    }

    private void KinematicSystem(GameObject[] entities)
    {  //speed is how fast you go in a straight line, acceleration is how quickly you can turn, and steering determines if you are accelerating in a clockwise or counter-clockwise direction.
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Kinematics>() != null && e.GetComponent<Controller>() != null)
            {
                Kinematics eKinematics = e.GetComponent<Kinematics>();
                Controller eController = e.GetComponent<Controller>();
                float speed = Utilities.MapSpeed(eKinematics);
                float acceleration = Utilities.MapAcceleration(eKinematics);
                Utilities.Enums.Trinary steering = Utilities.Enums.Trinary.Positive;
                if (eController.isRightButtonPressed)
                {
                    steering = Utilities.Enums.Trinary.Positive;
                }
                else if (!eController.isRightButtonPressed)
                {
                    steering = Utilities.Enums.Trinary.Negative;
                }
                if (e.GetComponent<Ailments>() != null && e.GetComponent<Ailments>().freezeCounter > 0)
                {  //if this is frozen, it cannot steer
                    steering = Utilities.Enums.Trinary.Neutral;
                }
                if(e.GetComponent<PowerupsApplied>() != null)
                {
                    PowerupsApplied ePowerupsApplied = e.GetComponent<PowerupsApplied>();
                    if(ePowerupsApplied.jetPowerupCounter > 0)
                    { //increase speed as long as the jet powerup's counter indicates it is active.
                        speed *= 1.5f;
                    }
                }
                eKinematics.direction += acceleration * (int)steering;
                eKinematics.direction = Utilities.Math.WrapAngle(eKinematics.direction);
                e.transform.position = new Vector2(e.transform.position.x + Mathf.Cos(eKinematics.direction) * speed, e.transform.position.y + Mathf.Sin(eKinematics.direction) * speed);
                if (e.GetComponent<Controller>() != null && !e.GetComponent<Controller>().isNPC)
                {  //if this kinematic object is a playable character
                    if (e.gameObject.GetComponentInChildren<Target>() != null)
                    {
                        int targetDistance = 15;  //how far out from the parent the target should be
                        Target eChildTarget = e.gameObject.GetComponentInChildren<Target>();
                        eChildTarget.transform.localPosition = new Vector2(Mathf.Cos(eKinematics.direction) * targetDistance, Mathf.Sin(eKinematics.direction) * targetDistance);
                    }
                }
            }
        }
    }

    private void IntelligenceSystem(GameObject[] entities)
    {  //while UserInputSystem updates the controller cpnt for PCs, IntelligenceSystem updates the controller cpnt for NPCs
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Intelligence>() != null && e.GetComponent<Controller>() != null && e.GetComponent<Kinematics>() != null)
            {
                Intelligence eIntelligence = e.GetComponent<Intelligence>();
                Controller eController = e.GetComponent<Controller>();
                Kinematics eKinematics = e.GetComponent<Kinematics>();
                GameObject target;
                if (eIntelligence.isEnemySeeking)
                {
                    target = Utilities.Searches.FindNearestEnemy(eIntelligence);
                    if (target != null)
                    {
                        if (Utilities.Math.IsFastestAngleRouteCW(e.transform.position.x, e.transform.position.y, eKinematics.direction, target.transform.position.x, target.transform.position.y))
                        {  //move CCW
                            eController.isRightButtonPressed = true;
                        }
                        else
                        {  //move CW
                            eController.isRightButtonPressed = false;
                        }
                        if (e.GetComponent<SpecialActions>() != null && target.GetComponent<Kinematics>())
                        {
                            SpecialActions eSA = e.GetComponent<SpecialActions>();
                            float angle = Utilities.Math.AngleBetweenTwoPoints(e.transform.position, target.transform.position);
                            if (Utilities.Math.CompareAngles(eKinematics.direction, target.GetComponent<Kinematics>().direction, 1f))
                            {
                                eController.isLeftButtonPressed = true;
                            }
                        }
                    }
                    else
                    {  //when no target is found, just have them "do nothing" by not pressing the button
                        eController.isRightButtonPressed = false;
                    }
                }
            }
        }
    }

    private void TimerSystem(GameObject[] entities)
    {  //this essentially manages counters, as well as the consequence that unfolds after the timer finishes or is reset. (-1 means it is inactive, zero means your time has run out.)
        foreach (GameObject e in entities)
        { //NOTE: At least one timer (Copilot class) is handled in the CopilotSystem. Inconsistent design, but can improve on next game. Necessary because the timer only runs under certain circumstances, as opposed to these, which basically just run whenever they are not at zero
            if(e.GetComponent<Collisions>() != null && e.GetComponent<Collisions>().receive != null)
            { //handle collision cooldown timer. It will be reset in the collisions system
                Collisions.Receive eReceive = e.GetComponent<Collisions>().receive;
                if (eReceive.temporaryImmunityCounter == eReceive.temporaryImmunityDuration)
                {
                    //D\ebug.Log("start cd");
                }
                if (eReceive.temporaryImmunityCounter == 1)
                {
                    //D\ebug.Log("end cd");
                }
                if(eReceive.temporaryImmunityCounter > 0)
                {
                    eReceive.temporaryImmunityCounter--;
                }
            }
            if (e.GetComponent<Ailments>() != null)
            {
                Ailments eAilments = e.GetComponent<Ailments>();
                //remember, -1 is the "reset", meaning the effect is no longer taking place.
                if (eAilments.freezeCounter >= 0)
                {
                    eAilments.freezeCounter--;
                }
                if (eAilments.poisonCounter >= 0)
                {
                    eAilments.poisonCounter--;
                }
                if (eAilments.lifespan >= 0)
                {
                    eAilments.lifespan--;
                }
                if (eAilments.poisonCounter == 0 || eAilments.lifespan == 0)
                {
                    if (e.GetComponent<Death>() != null)
                    {
                        //Actually, I think the bug was me just using the wrong variableeAilments.poison++; //this solves a bug where by the end of this system, poison counter reduced to -1 like it was never poisoned, so this poison can't transfer over into payload spawn, ie a large poisoned asteroid spawning two smaller, also poisoned asteroids.
                        e.GetComponent<Death>().isDeathFlagged = true;
                    }
                }
            }
            if (e.GetComponent<PowerupsApplied>() != null)
            {
                PowerupsApplied ePowerupsApplied = e.GetComponent<PowerupsApplied>();
                if (ePowerupsApplied.jetPowerupCounter >= 0)
                {
                    JetTrailVisual eJetTrailVisual = GetComponent<JetTrailVisual>();
                    if (eJetTrailVisual == null)
                    { //add a component that will create the visual of a jet trail when you are going fast.
                        eJetTrailVisual = e.AddComponent<JetTrailVisual>();
                    }
                    if (ePowerupsApplied.jetPowerupCounter % eJetTrailVisual.moduloInterval == 0)
                    { //every so often, leave behind a jetcloud that will make your ship look like it is moving quickly.
                        eJetTrailVisual.SpawnJetCloud();
                    }
                    ePowerupsApplied.jetPowerupCounter--;
                }
                if (ePowerupsApplied.jetPowerupCounter == 0)
                { //remove the jet trail visual if you have it applied.
                    Destroy(e.GetComponent<JetTrailVisual>());
                }
            }
            if (e.GetComponent<JetCloud>() != null)
            {
                JetCloud eJetCloud = e.GetComponent<JetCloud>();
                if (eJetCloud.jetCloudCounter > 0)
                {
                    eJetCloud.jetCloudCounter--;
                    if (eJetCloud.jetCloudCounter * 2 < eJetCloud.jetCloudDuration && eJetCloud.sr != null)
                    { //if the counter is halfway done, make the jet cloud become a little smaller.
                        eJetCloud.sr.sprite = gr.shrunkenJetCloud;
                    }
                }
                if (eJetCloud.jetCloudCounter == 0)
                {
                    Destroy(eJetCloud.gameObject);
                }
            }
        }
    }

    private void DeathSystem(GameObject[] entities)
    {
        void DropPayload(Death death)
        {
            int i = 0;
            foreach (GameObject payload in death.payload)
            { //note, the logic is designed so that every enemy has 3 payload slots, (0 and 1 for small asteroids/comets, and 2 for powerup. Each of those can be null, and this loop is designed to respect that.)
                if (payload != null)
                {
                    GameObject instance = Instantiate(payload);
                    instance.transform.position = death.gameObject.transform.position;
                    Kinematics iKinematics = instance.GetComponent<Kinematics>();
                    Kinematics parentKinematics = death.GetComponent<Kinematics>();
                    if(death.GetComponent<Ailments>() != null && instance.GetComponent<Ailments>() != null)
                    {
                        Ailments rootAilments = death.GetComponent<Ailments>();
                        Ailments spawnAilments = instance.GetComponent<Ailments>();
                        if(rootAilments.poisonCounter == 0)
                        {
                            spawnAilments.poisonCounter = spawnAilments.poisonDuration;
                            GameObject poisonAura = Instantiate(gr.PoisonAura);  //create a poison aura to surround the target visually
                            poisonAura.transform.parent = instance.transform;  //attach that poison aura's position to that of the target
                            poisonAura.transform.localPosition = new Vector2(0, 0);
                        }
                    }
                    if (iKinematics != null && parentKinematics != null)
                    {
                        float directionShift = 0;
                        if (i == 0)
                        {
                            //do nothing (included for clarity / reminder)
                        }
                        else if (i == 1)
                        { //i1 is asteroid/comet child
                            directionShift = .5f;
                        }
                        else if (i == 2)
                        { //i2 is asteroid/comet child as well
                            directionShift = -.5f;
                        }
                        else if (i == 3)
                        { //i3 is powerup
                          //do nothing (included for clarity/reminder)
                        }
                        iKinematics.direction = parentKinematics.direction + directionShift;
                    }
                    try
                    { //try to set cooldown if object has collisions cpnt
                        Collisions.Receive receive = instance.GetComponent<Collisions>().receive;
                        receive.temporaryImmunityCounter = receive.temporaryImmunityDuration;
                    }
                    catch
                    {
                        Debug.LogWarning("Jim. No collision cpnt?");
                    }
                    i++;
                }
            }
        }
        void PrintPoints(Death death)
        {
            int pointsValue = 0;
            GameObject points = Instantiate(gr.Points);
            points.transform.position = death.gameObject.transform.position;  //position points message where the thing that was destroyed died
            pointsValue = death.points;
            if(death.GetComponent<HasBounty>() != null)
            {
                pointsValue *= 2;
            }
            points.GetComponent<Text>().message = pointsValue.ToString();  //print however many points the object was worth
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Death>() != null && e.GetComponent<Death>().isDeathFlagged)
            {
                Death eDeath = e.GetComponent<Death>();
                DropPayload(eDeath);
                if (eDeath.points != 0)
                {
                    PrintPoints(eDeath);
                }
                gs.currentScore += eDeath.points;
                Destroy(e);
            }
        }
    }

    private void SpecialActionSystem(GameObject[] entities)
    {
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<SpecialActions>() != null && e.GetComponent<Controller>() != null && (e.GetComponent<Ailments>() == null || e.GetComponent<Ailments>() != null && e.GetComponent<Ailments>().freezeCounter == -1))
            {
                SpecialActions eSA = e.GetComponent<SpecialActions>();
                Controller eController = e.GetComponent<Controller>();
                if (eSA.reloadCounter == Utilities.MapReloadSpeed(eSA))
                {  //shoot
                    eSA.shotsFired = 1;  //this is the first shot fired
                    eSA.reloadCounter--;
                    Utilities.PerformSpecialAction(e);
                }
                else if (eSA.reloadCounter == 0)
                {  //can reload
                    if (eController.isLeftButtonPressed)
                    {  //actually reload (which triggers action next frame)
                        eSA.reloadCounter = Utilities.MapReloadSpeed(eSA);
                        eSA.multiShotCounter = eSA.multiShotSpeed;
                    }
                    //else just wait
                }
                else
                {
                    if (eSA.multiShotCounter > 0)
                    {
                        eSA.multiShotCounter--;
                    }
                    if (eSA.multiShotCounter == 1 && eSA.isMultiShot && eSA.shotsFired <= 3)
                    {  //multi shoot
                        eSA.shotsFired++;
                        Utilities.PerformSpecialAction(e);
                        if(eSA.shotsFired < 3)
                        {
                            eSA.multiShotCounter = eSA.multiShotSpeed;
                        }
                    }
                    eSA.reloadCounter--;
                }
            }
        }
    }

    private void UserInputSystem(GameObject[] entities)
    {  //find player, detect user input, and update controller cpnt accordingly.
        void HandleKeyboardInput(Controller controller)
        {
            if (Input.GetKey(KeyCode.BackQuote))
            {  //left button pressed (keyboard)
                controller.isLeftButtonPressed = true;
            }
            if (Input.GetKey(KeyCode.Alpha1))
            {  //right button pressed (keyboard)
                controller.isRightButtonPressed = true;
            }
        }
        void HandleTouchInput(Controller controller)
        {
            Touch[] touches = Input.touches;
            int screenMidpoint = Screen.width / 2;
            foreach (Touch touch in touches)
            {
                if (touch.position.x <= screenMidpoint)
                {  //left button pressed (touchscreen)
                    controller.isLeftButtonPressed = true;
                }
                else if (touch.position.x > screenMidpoint)
                {  //right button pressed (touchscreen)
                    controller.isRightButtonPressed = true;
                }
            }
        }
        void IsTouchscreenButtonPressed(Button button)
        {
            bool click = Input.GetMouseButton(0);
            Vector2 pos;
            if (click && gs.isMenuAcceptingInput)
            { //if the mouse has been clicked, and the menu is accepting input, get the coordinates
                pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                if (pos.x > button.leftBound && pos.x < button.rightBound && pos.y > button.topBound && pos.y < button.bottomBound)
                { //you have touched inside the button. Perform the button's functionality.
                    Sgs.ButtonHandler(button.function);
                }
            }
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Controller>() != null && !e.GetComponent<Controller>().isNPC)
            {  //find the player
                Controller eController = e.GetComponent<Controller>();
                Utilities.ResetControllerInputs(eController);
                HandleTouchInput(eController);
                HandleKeyboardInput(eController);
            }
            if (e.GetComponent<Button>() != null)
            {
                Button eButton = e.GetComponent<Button>();
                IsTouchscreenButtonPressed(eButton);
            }
        }
        if (!Input.GetMouseButton(0))
        { //patches a bug where a single keypress lasting longer than a frame can press multiple buttons on consecutive menus. This ensures that you have to release the mouse button first.
            gs.isMenuAcceptingInput = true;
        }
    }
    private void AnimationSystem(GameObject[] entities)
    {
        void DetermineAnimation(GameObject e, Animator animator)
        {
            bool isDark = e.GetComponent<Intelligence>() != null && e.GetComponent<Intelligence>().team == Intelligence.Team.Dark;
            if (isDark)
            {
                Utilities.Visual.PlayAnimation("StandardD", animator);  //Note: Have yet to import my Utilities script into this project.
            }
            else
            {
                Utilities.Visual.PlayAnimation("StandardL", animator);
            }
            if (e.GetComponent<Ailments>() != null && e.GetComponent<Ailments>().freezeCounter >= 0)
            {  //don't perform an animation if you are frozen
                Utilities.Visual.PauseAnimation(animator);
            }
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Animations>() != null)
            {
                Animations eAnimations = e.GetComponent<Animations>();
                if (e.GetComponent<Ailments>() != null && e.GetComponent<SpriteRenderer>() != null)
                {
                    Ailments eAilments = e.GetComponent<Ailments>();
                    SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    if (eAilments.freezeCounter > 0)
                    {  //if object is frozen, apply shader to visually indicate this.
                        sr.GetPropertyBlock(mpb);
                        mpb.SetFloat("_IsFrozen", 1);  // assumes your shader has a "_Brightness" property
                        sr.SetPropertyBlock(mpb);
                    }
                    else if (eAilments.freezeCounter == 0)
                    {  //at exact moment that obj becomes unfrozen, remove the shader
                        sr.GetPropertyBlock(mpb);
                        mpb.SetFloat("_IsFrozen", 0);
                        sr.SetPropertyBlock(mpb);
                    }
                }
                if (e.GetComponent<Kinematics>() != null)
                {
                    Kinematics eKinematics = e.GetComponent<Kinematics>();
                    if (eAnimations.sr != null && eAnimations.animator != null)
                    {
                        Utilities.Visual.DetermineReflection(eAnimations.sr, eKinematics);
                        DetermineAnimation(e, eAnimations.animator);
                        if (eAnimations.rotationTimer != null && eAnimations.rotationTimer.duration > 0)
                        {
                            Timer rotationTimer = eAnimations.rotationTimer;
                            if (rotationTimer.counter == 0)
                            {
                                e.transform.Rotate(0, 0, 90f);  //rotates the sprite 90 degrees every counter interval
                                rotationTimer.counter = rotationTimer.duration;
                            }
                            else
                            {
                                rotationTimer.counter--;
                            }
                        }
                    }
                }
                if (e.GetComponent<Target>() != null && e.transform.parent.GetComponent<SpecialActions>() != null)
                {  //if this is a target cursor, and it is attached to a parent with special abilities
                    SpecialActions eParentSA = e.transform.parent.GetComponent<SpecialActions>();
                    if (eParentSA.reloadCounter == 0)
                    {
                        eAnimations.sr.sprite = gr.targetReady;
                    }
                    else if (eParentSA.reloadCounter >= 0)
                    {
                        eAnimations.sr.sprite = gr.targetLoading;
                    }
                }
            }
        }
    }

    private void TextSystem(GameObject[] entities)
    {
        Sprite FindChar(Text text, char letter)
        {  //takes a char from message field, and maps it to the corresponding sprite
            foreach (Sprite sprite in text.letters)
            {
                if(letter == '/' && sprite.name == "slash")
                { //fixes a bug where you aren't allowed to name your sprite for the forward slash character "/" in the sprite editor
                    Debug.Log("gotcha");
                    return sprite;
                }
                if (sprite.name[0] == letter)
                {
                    return sprite;
                }
            }
            Debug.LogWarning("Jim. Should never return null");
            return null;  //should never return null
        }
        void UpdateCursorPosition(Text text)
        {
            //for now, I don't have to worry about text-wrapping and newlines, so this is a very easy implementation. In future games, though, I will have to update this, and it will likely tax performance...
            text.cursorX += text.LETTER_WIDTH;
        }
        void Clear(Text text)
        {
            foreach (Transform child in text.transform)
            {
                Destroy(child.gameObject);
            }
            text.cursorX = 0;
        }
        void Print(Text text)
        {
            Sprite letter;
            for (int i = 0; i < text.message.Length; i++)
            {
                letter = FindChar(text, text.message[i]);
                if (letter != null)
                {
                    GameObject letterGO = new GameObject(letter.name);
                    SpriteRenderer letterSR;
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    float inversionAsFloat = 0;
                    if (text.isInvertedColor)
                    {
                        inversionAsFloat = 1;
                    }
                    letterGO.transform.parent = text.transform;  //make letterGameObject a child of the overall "Text" prefab object
                    letterGO.transform.localPosition = new Vector2(text.cursorX, text.cursorY);  //position letterGameObject locally relative to its parent
                    letterSR = letterGO.AddComponent<SpriteRenderer>();
                    letterSR.sprite = letter;
                    letterSR.material = text.material;
                    letterSR.GetPropertyBlock(mpb);
                    mpb.SetFloat("_IsInverted", inversionAsFloat);  // assumes your shader has a "_Brightness" property
                    mpb.SetFloat("_IsText", 1);
                    mpb.SetColor("_Fill", text.fill);
                    mpb.SetColor("_Stroke", text.stroke);
                    letterSR.SetPropertyBlock(mpb);
                }  //else if letter is null, that means it is a spacebar, and all you have to do is move the cursor, without creating a new child obj letter representation.
                UpdateCursorPosition(text);
            }
        }
        foreach (GameObject e in entities)
        {
            if (e.GetComponent<Text>() != null)
            {
                Text eText = e.GetComponent<Text>();
                switch (eText.dataBind)
                {
                    case Text.DataBind.CurrentScore:
                        eText.message = gs.currentScore.ToString();
                        break;
                    case Text.DataBind.HighScoreA:
                        eText.message = "A:" + gs.highScoreA.ToString();
                        break;
                    case Text.DataBind.HighScoreB:
                        eText.message = "B:" + gs.highScoreB.ToString();
                        break;
                    case Text.DataBind.HighScoreC:
                        eText.message = "C:" + gs.highScoreC.ToString();
                        break;
                }
                if (eText.message != eText.oldMessage || eText.isFirstPrint)
                {
                    eText.oldMessage = eText.message;
                    Clear(eText);  //get rid of old letter sprites if they exist
                    Print(eText);  //print out the new letter sprites.
                    eText.isFirstPrint = false;
                }
                if (eText.cameraRelative != null)
                {  //this text has been asked to position itself relative to the camera 
                    Vector2 cameraCoordinates = eText.cameraRelative.gameObject.transform.position;
                    eText.transform.position = new Vector2(cameraCoordinates.x + eText.xPreCamera, cameraCoordinates.y + eText.yPreCamera);
                }
            }
        }
    }
}
