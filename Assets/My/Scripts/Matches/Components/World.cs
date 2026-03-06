using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class World : MonoBehaviour
{
    public int gameOverCounter;
    public int level;  //as this increases, new enemies will start to appear, and spawn speed will decrease.
    
    public int spawnRadius;  //defines where enemies start spawning from. This is off the screen, so probably like 150 or something. Not to be confused with the radius of visibility. That is defined as an attribute of the player.
    public int deathRadius;  //defines where enemies get deleted from being too far offscreen.
    [HideInInspector] public CameraController camera;
    [HideInInspector] public GameObject[] spawnSelections;  //map these strings to specific prefab instantiations and rules for how they should spawn, and how long before the next one can spawn.
    [HideInInspector] public int levelCounter;  //after each level ends, reset this to a certain value to determine when the next level will occur.
    [HideInInspector] public int index;  //increase this to indicate how many prefabs you can randomly pull from spawnSelections by making a local sub-array in Systems.cs
    [HideInInspector] public int nextSpawnCounter;

    void Awake()
    {
        GlobalReferences gr = FindObjectOfType<GlobalReferences>();
        gameOverCounter = 100;
        if (gr != null)
        {
            //order of spawnSelections matters, as you will make a sub-array for which game objects can appear in the scene.
            spawnSelections = new GameObject[] { gr.AsteroidSmall, gr.AsteroidLarge, gr.FrazpowMissile, gr.InvasionFighter, gr.CometSmall, gr.SpaceSquid, gr.CometLarge, gr.Terriloomer };
        }
        else
        {  //shouldn't happen. You should always have the global references in your scene
            Debug.LogWarning("jim. null array");
            spawnSelections = new GameObject[8];
        }

        //the next three lines of code help initialize level 0, where you only encounter small asteroids for the first 10s.
        levelCounter = GlobalValues.fps * 10;  //level 0 will last for 10s
        index = 1;  //the sub-array of enemies you can spawn will consist of the first 1 element in spawnSelections
        nextSpawnCounter = 0;  //spawn something right away
    }
    void Start()
    {
        GlobalState gs = FindObjectOfType<GlobalState>();
        camera = FindObjectOfType<CameraController>();
        Text[] scores = Utilities.Searches.FindByComponent<Text>();
        foreach(Text text in scores)
        {
            if(text.dataBind != Text.DataBind.CurrentScore)
            { //depends on what's in the scene, but there should only be two text objects - one for the current score, and another for the highscore. The highscore one should be found, and set to the proper a b or c highscore data bind
                if(gs.gameMode == Sgs.GameModes.GameA)
                {
                    text.dataBind = Text.DataBind.HighScoreA;
                }
                else if(gs.gameMode == Sgs.GameModes.GameB)
                {
                    text.dataBind = Text.DataBind.HighScoreB;
                }
                else if(gs.gameMode == Sgs.GameModes.GameC)
                {
                    text.dataBind = Text.DataBind.HighScoreC;
                }
            }
        }
    }
}
