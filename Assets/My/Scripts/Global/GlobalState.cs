using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{  //defines data that should persist between scene changes. Unlike GlobalReferences, whose fields remain constant, GlobalState's fields change at runtime. This too requires an object instance.
    public bool isFirstTimePlayer;  //this is only true for the very first time you open this app. (It is used to force people to read the tutorial)
    public bool hasUnlockedGameB;
    public bool hasUnlockedGameC;
    public Sgs.GameModes gameMode;  //starts off on game mode a when first downloaded, but as you unlock the other game mode(s), this will change at runtime to which one you left off playing based on what you choose from the home menu, and persist after closing app
    public MyScenes currentScene;  //this is the scene you are actually on. If you want to change scenes, set a new value to newScene, and Systems.ManageScenes will detect the inequality between the fields and trigger a scene change accordingly, restoring synchronicity between currentScene and newScene.
    public MyScenes newScene;
    public Sgs.Pages currentPage;  //tells which menu page you are currently on. Sgs.NewMenuPage() handles this.
    public Sgs.Pages desiredPage;  //when the menu scene is first loaded, it doesn't know which of its pages to load. However, if you set this value externally via other scripts, and compare it to currentPage, you can trigger a page load without having to hit a button.
    public int currentScore;
    public int highScoreA;
    public int highScoreB;
    public int highScoreC;
    public bool isMenuAcceptingInput;  //patches a bug. it is much easier to listen for a mouse keypress event than a keyup/down event. However, this can cause one keypress to trigger multiple buttons all at once if the press lasts longer than a frame and the new menu page triggered by the first button has a button in the same spot, etc. This bool is set to false every time a new menu page is generated. Then, Systems.UserInputSystem() will set it back to true as soon as there is no press event occuring. Hopefully this works well for touch and mouse inputs.
    public Sgs.SgsButtonHandler preventDuplicateShipUpgrades;  //after the first ship upgrade, set this to that function value, so that when the menu appears a second time, the button with that function value will be disabled and not able to be reselected.
    public PlayerConfiguration playerConfiguration;

    public enum MyScenes
    {
        Menus = 0,  //an uninitialized enum defaults not to null but to the 0th element, which in this case is correctly mapped to the menu scene. Also, all menu setups will be represented by the same scene. A separate manager will change to different "pages" of the menu, if you will.
        Match = 1
    }

    [System.Serializable] public class PlayerConfiguration
    {
        public ShipUpgrade[] upgrades = new ShipUpgrade[1];
        public Ammo ammo;
        public Copilot copilot;
        public ShipAppearance shipAppearance;  //while upgrades handles the actual behavior changes that your one or two upgrades give you, only the first time you are asked to upgrade your ship will determine the visual appearance of what your ship looks like

        public enum ShipAppearance
        {
            Speed = 1,
            Turns = 2,
            Armor = 3,
            Attack = 4
        }

        public enum ShipUpgrade
        {
            Speed = 4,
            Turns = 1,
            Armor = 2,
            Attack = 3,
            None = 0
        }
        public enum Ammo
        {
            Bullet = 0,
            Cannon = 1,
            Poison = 2,
            Freeze = 3,
            Conversion = 4
        }
        public enum Copilot
        {
            Navigator = 4,
            BountyHunter = 1,
            Princess = 2,
            Mechanic = 3,
            None = 0
        }
    }
}
