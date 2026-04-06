using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public static class Sgs
{
    public static class TextColors
    { //creates ordered arrays containing 2 colors (body, outline), so that I can reuse a consistent color theme throughout my game's UI
        //dark purple, black
        public static Color[] information = {new Color(0.69f, 0, 1), new Color(0, 0, 0)};

        //yellow-orange, dark red-orange
        public static Color[] header = {new Color(1, 0.65f, 0), new Color(0.84f, 0.39f, 0)};

        //grey, dark-purple
        public static Color[] lockedButton = {new Color(0.55f, 0.55f, 0.55f), new Color(0.67f, 0, 1)};

        //yellow-green, dark-green
        public static Color[] button = {new Color(0, 0.37f, 0), new Color(0.37f, 1, 0)};

        //white, black (points are highscores and what appears over an enemy you destroy or a powerup you collect)
        public static Color[] points = {new Color(1, 1, 1), new Color(0, 0, 0)};
    }
    public enum Pages
    { //these define every possible "page" you can navigate to within the menu system
        Home = 0,
        AdvertiseGameB = 1,
        ShipUpgrade = 2,
        Ammo = 3,
        Copilot = 4,
        GameOver = 5,
        None = 6,
        AdvertiseGameC = 7,
        Tutorial = 8
    }
    public enum GameModes
    {
        GameA = 0,
        GameB = 1,
        GameC = 2
    }
    public static void StartMatch()
    {
        GlobalState gs = GameObject.FindObjectOfType<GlobalState>();
        SceneManager.LoadScene("Match");
        if (gs != null)
        { //save the current menu page (none, since a match is going, as well as the desired menu page you should return to once the match is over).
            gs.currentPage = Pages.None;
            gs.desiredPage = Pages.GameOver;
        }
    }
    public static void NewMenuPage(Pages page)
    { //destroy all buttons and texts, and then create new ones in their place.
        GlobalState gs = GameObject.FindObjectOfType<GlobalState>();
        GlobalReferences gr = GameObject.FindObjectOfType<GlobalReferences>();

        void DestroyCurrentPage()
        {
            MenuItem[] menuItems = GameObject.FindObjectsOfType<MenuItem>();
            foreach(MenuItem menuItem in menuItems)
            {
                GameObject.Destroy(menuItem.gameObject);
            }
        }
        GameObject MakeText(float x, float y, string message, Color[] colors)
        {
            //NOTE: I may remove this method come production time, since titles will probably not be actual Text prefabs
            GameObject textObj = GameObject.Instantiate(gr.Text);
            Text textCpnt;
            textCpnt = textObj.GetComponent<Text>();
            textObj.transform.position = new Vector2(x, y);
            if (textCpnt != null)
            {
                textCpnt.message = message;
                textCpnt.fill = colors[0];
                textCpnt.stroke = colors[1];
            }
            return textObj;
        }
        void MakeButton(float x, float y, string message, SgsButtonHandler function, Color[] colors)
        {
            GameObject buttonObj = GameObject.Instantiate(gr.Button);
            Button buttonCpnt = buttonObj.GetComponent<Button>();
            buttonObj.transform.position = new Vector2(x, y);
            if (buttonCpnt != null)
            {
                if (message != null)
                {
                    Text textCpnt = buttonCpnt.AddComponent<Text>();
                    textCpnt.message = message;
                    textCpnt.fill = colors[0];
                    textCpnt.stroke = colors[1];
                    textCpnt.material = gr.pixelBubbleMaterial; //dynamically created cpnts do not inherit serialized fields set in inspector. That's why you have to respecify that here
                }
                buttonCpnt.function = function;
            }
        }
        void IncludeHighScore()
        {
            Text highScore = MakeText(0, 90, "", TextColors.points).GetComponent<Text>();
            if (highScore != null)
            {
                if(gs.gameMode == GameModes.GameA)
                {
                    highScore.dataBind = Text.DataBind.HighScoreA;
                }
                else if(gs.gameMode == GameModes.GameB)
                {
                    highScore.dataBind = Text.DataBind.HighScoreB;
                }
                else if(gs.gameMode == GameModes.GameC)
                {
                    highScore.dataBind = Text.DataBind.HighScoreC;
                }
                //DEPRACATED: highScore.dataBind = Text.DataBind.HighScore;
            }
        }
        void CreateHomePage()
        {
            IncludeHighScore();

            GameObject.Instantiate(gr.Title, new Vector3(0, 60, 0), Quaternion.identity);

            MakeButton(-70, -20, "Tutorial", SgsButtonHandler.Tutorial, TextColors.button);

            if(gs.hasUnlockedGameC)
            {
                MakeButton(-70, -40, "Game A", SgsButtonHandler.PlayGameA, TextColors.button);
                MakeButton(-70, -60, "Game B", SgsButtonHandler.PlayGameB, TextColors.button);
                MakeButton(-70, -80, "Game C", SgsButtonHandler.PlayGameC, TextColors.button);
            }
            else if(gs.hasUnlockedGameB)
            {
                MakeButton(-70, -40, "Game A", SgsButtonHandler.PlayGameA, TextColors.button);
                MakeButton(-70, -60, "Game B", SgsButtonHandler.PlayGameB, TextColors.button);
            }
            else
            {  //unpaid player. base game
                if(PlayerPrefs.GetInt("HasCompletedTutorial") == 0)
                { //don't allow players to click buttons to play games when they first open the app. They must read the tutorial first
                    MakeText(-70, -40, "Game A", TextColors.lockedButton);
                    MakeText(-70, -60, "Game B", TextColors.lockedButton);
                }
                else
                {
                    MakeButton(-70, -40, "Game A", SgsButtonHandler.PlayGameA, TextColors.button);
                    MakeButton(-70, -60, "Game B", SgsButtonHandler.UnlockGameB, TextColors.lockedButton);
                }
            }
        }
        void CreateAdvertiseGameBPage()
        {
            //only type of page that doesn't include high score
            MakeText(-70, 80, "Unlock Game B?", TextColors.header);
            MakeText(-90, 40, "Features:", TextColors.information);
            MakeText(-70, 30, "- Two new spaceships!", TextColors.information);  //This sounds cooler, but you would probably only be able to call it this if you do in fact design new artwork for each type of spaceship, instead of just changing the stats. Otherwise you'd probably have to call them "spaceship upgrades", which I guess still sounds like decent marketing.
            MakeText(-70, 20, "- Three new blasters!", TextColors.information);
            MakeText(-70, 10, "- Four new copilots!", TextColors.information);
            MakeText(-70, 0, "- Access Game B", TextColors.information); //note that I might not actually have something like this...
            MakeText(-57, -10, "Leaderboard!", TextColors.information); //note that I might not actually have something like this...
            MakeButton(-80, -50, "BUY NOW!", SgsButtonHandler.BuyGameB, TextColors.button);
            MakeButton(10, -50, "Return Home", SgsButtonHandler.ReturnHome, TextColors.button);
        }
        void CreateAdvertiseGameCPage()
        {
            //only type of page that doesn't include high score
            MakeText(-70, 80, "Unlock Game C?", TextColors.header);
            MakeText(-90, 40, "Features:", TextColors.information);
            MakeText(-70, 30, "- Two new enemies!", TextColors.information);  //ufo with tractor beam
            MakeText(-70, 20, "- One new powerup!", TextColors.information);
            MakeText(-70, 10, "- One new spaceship!", TextColors.information); //the clear-bubble looking ship that has extra space for the copilot to operate more efficiently
            MakeText(-70, 0, "- Two new blasters!", TextColors.information);  //the bomb that you drop behind you and it explodes shortly thereafter. Also, a tractor beam that works just like the UFO (you can use it to collect powerups, or siphon in enemies then convert them)
            MakeText(-70, -10, "- One new copilot!", TextColors.information);  //come up with something
            MakeText(-70, -20, "- Access Game C Leaderboard!", TextColors.information);
            MakeButton(-80, -50, "BUY NOW!", SgsButtonHandler.BuyGameC, TextColors.button);
            MakeButton(10, -50, "Return Home", SgsButtonHandler.ReturnHome, TextColors.button);
        }
        void CreateTutorialPage()
        {
            int tbsy = 70;
            /*
                - Press and hold the right half of the screen to steer your ship counterclockwise. Release to turn clockwise.
                - Press the left half of the screen to shoot your blaster!
                                |                         |
            */
            MakeText(-35, 90, "TUTORIAL:", TextColors.header);

            MakeText(-90, tbsy, "- Press and hold the right", TextColors.information);
            MakeText(-77, tbsy - 10, "half of the screen to", TextColors.information);
            MakeText(-77, tbsy - 20, "steer your ship counter-", TextColors.information);
            MakeText(-77, tbsy - 30, "clockwise. Release to", TextColors.information);
            MakeText(-77, tbsy - 40, "turn clockwise.", TextColors.information);

            MakeText(-90, tbsy - 60, "- Press the left half of", TextColors.information);
            MakeText(-77, tbsy - 70, "the screen to shoot your", TextColors.information);
            MakeText(-77, tbsy - 80, "blaster!", TextColors.information);

            MakeButton(-30, -60, "Got it!", SgsButtonHandler.PlayGameA, TextColors.button);
            PlayerPrefs.SetInt("HasCompletedTutorial", 1);

        }
        void CreateChooseAndUpgradeShipPages()
        {
            /*DEPRACATED: How I implemented before I redesigned menu interface to account for both a game b and game c
            SgsButtonHandler param0 = SgsButtonHandler.UnlockGameB;
            SgsButtonHandler param1 = SgsButtonHandler.UnlockGameB;
            IncludeHighScore();
            MakeButton(-70, -40, "Speed", SgsButtonHandler.Speed);
            MakeButton(20, -40, "Turning", SgsButtonHandler.Turning);
            if (gs.isPaidPlayer)
            {
                param0 = SgsButtonHandler.Attack;
                param1 = SgsButtonHandler.Armor;
            }
            MakeButton(-70, -60, "Attack", param0, !gs.isPaidPlayer);
            MakeButton(20, -60, "Armor", param1, !gs.isPaidPlayer);
            if (gs.playerConfiguration.upgrades[0] == GlobalState.PlayerConfiguration.ShipUpgrade.None)
            {
                MakeText(-50, 70, "Choose Your Ship!");
            }
            else if (gs.playerConfiguration.upgrades[0] != GlobalState.PlayerConfiguration.ShipUpgrade.None)
            { //if this is the second time being asked to upgrade your ship (paid players)
                MakeText(-50, 70, "Upgrade Your Ship!");
                Button[] buttons = GameObject.FindObjectsOfType<Button>();
                foreach (Button button in buttons)
                {
                    if (button.function == gs.preventDuplicateShipUpgrades)
                    {
                        GameObject.Destroy(button.gameObject);
                    }
                }
            }*/
            IncludeHighScore();
            if(gs.gameMode == GameModes.GameA)
            {  //base options for game a
                MakeButton(-70, -40, "Speed", SgsButtonHandler.Speed, TextColors.button);
                MakeButton(20, -40, "Turning", SgsButtonHandler.Turning, TextColors.button);
                if(!gs.hasUnlockedGameB)
                {
                    MakeButton(-70, -60, "Attack", SgsButtonHandler.Locked, TextColors.lockedButton);
                    MakeButton(20, -60, "Armor", SgsButtonHandler.Locked, TextColors.lockedButton);
                }
            }
            if(gs.gameMode == GameModes.GameB)
            {  //add more options for game b
                MakeButton(-70, -40, "Speed", SgsButtonHandler.Speed, TextColors.button);
                MakeButton(20, -40, "Turning", SgsButtonHandler.Turning, TextColors.button);
                MakeButton(-70, -60, "Attack", SgsButtonHandler.Attack, TextColors.button);
                MakeButton(20, -60, "Armor", SgsButtonHandler.Armor, TextColors.button);
            }
            if(gs.gameMode == GameModes.GameC)
            {  //add more options for game c
                //implement later
            }

            //this func is called twice, both when you "choose" and when you "upgrade" your ship, because functionally they are very similar and for maintenance/design consistency's sake, I want to avoid redundant code
            if (gs.playerConfiguration.upgrades[0] == GlobalState.PlayerConfiguration.ShipUpgrade.None)
            {
                MakeText(-50, 70, "Choose Your Ship!", TextColors.header);
            }
            else if (gs.playerConfiguration.upgrades[0] != GlobalState.PlayerConfiguration.ShipUpgrade.None)
            { //if this is the second time being asked to upgrade your ship (paid players)
                MakeText(-50, 70, "Upgrade Your Ship!", TextColors.header);
                Button[] buttons = GameObject.FindObjectsOfType<Button>();
                foreach (Button button in buttons)
                {
                    if (button.function == gs.preventDuplicateShipUpgrades)
                    {
                        GameObject.Destroy(button.gameObject);
                    }
                }
            }
        }
        void CreateAmmoPage()
        {
            /*DEPRACATED: From old menu system (see CreateChooseAndUpgradeShipPages())
            SgsButtonHandler param0, param1, param2;
            param0 = SgsButtonHandler.UnlockGameB;
            param1 = SgsButtonHandler.UnlockGameB;
            param2 = SgsButtonHandler.UnlockGameB;
            IncludeHighScore();
            MakeText(-70, 70, "Choose Your Blaster!");
            MakeButton(-70, -40, "Bullets", SgsButtonHandler.Bullets);
            MakeButton(20, -40, "Cannon", SgsButtonHandler.Cannon);
            if (gs.isPaidPlayer)
            {
                param0 = SgsButtonHandler.Freeze;
                param1 = SgsButtonHandler.Poison;
                param2 = SgsButtonHandler.Conversion;
            }
            MakeButton(-70, -60, "Freeze", param0, !gs.isPaidPlayer);
            MakeButton(20, -60, "Poison", param1, !gs.isPaidPlayer);
            MakeButton(-40, -80, "Conversion", param2, !gs.isPaidPlayer);*/

            IncludeHighScore();
            MakeText(-70, 70, "Choose Your Blaster!", TextColors.header);
            if(gs.gameMode == GameModes.GameA)
            {   //base options for game a
                MakeButton(-70, -40, "Bullets", SgsButtonHandler.Bullets, TextColors.button);
                MakeButton(20, -40, "Cannon", SgsButtonHandler.Cannon, TextColors.button);
                if(!gs.hasUnlockedGameB)
                {
                    MakeButton(-70, -60, "Freeze", SgsButtonHandler.Locked, TextColors.lockedButton);
                    MakeButton(20, -60, "Poison", SgsButtonHandler.Locked, TextColors.lockedButton);
                    MakeButton(-70, -80, "Conversion", SgsButtonHandler.Locked, TextColors.lockedButton);
                }
            }
            else if(gs.gameMode == GameModes.GameB)
            {   //add more options for game b
                MakeButton(-70, -40, "Bullets", SgsButtonHandler.Bullets, TextColors.button);
                MakeButton(20, -40, "Cannon", SgsButtonHandler.Cannon, TextColors.button);
                MakeButton(-70, -60, "Freeze", SgsButtonHandler.Freeze, TextColors.button);
                MakeButton(20, -60, "Poison", SgsButtonHandler.Poison, TextColors.button);
                MakeButton(-70, -80, "Conversion", SgsButtonHandler.Conversion, TextColors.button);
            }
            else if(gs.gameMode == GameModes.GameC)
            {   //add more options for game c
                //implement later
            }
        }
        void CreateCopilotPage()
        {
            /*DEPRACATED: From old menu system (see CreateChooseAndUpgradeShipPages())
            IncludeHighScore();
            MakeText(-70, 70, "Choose Your Copilot!");
            MakeButton(-80, -40, "Mechanic", SgsButtonHandler.Mechanic);
            MakeButton(20, -40, "Navigator", SgsButtonHandler.Navigator);
            MakeButton(-90, -60, "BountyHunter", SgsButtonHandler.BountyHunter);
            MakeButton(20, -60, "Princess", SgsButtonHandler.Princess);*/
            IncludeHighScore();
            MakeText(-70, 70, "Choose Your Copilot!", TextColors.header);
            if(gs.gameMode == GameModes.GameB)
            {   //base options for game b
                MakeButton(-80, -40, "Mechanic", SgsButtonHandler.Mechanic, TextColors.button);
                MakeButton(20, -40, "Navigator", SgsButtonHandler.Navigator, TextColors.button);
                MakeButton(-80, -60, "BountyHunter", SgsButtonHandler.BountyHunter, TextColors.button);
                MakeButton(20, -60, "Princess", SgsButtonHandler.Princess, TextColors.button);
            }
            else if(gs.gameMode == GameModes.GameC)
            {   //add more options for game c
                //implement later
            }
        }
        void CreateGameOverPage()
        {
            IncludeHighScore();
            MakeText(-30, 20, "Game Over...", TextColors.points);
            MakeButton(-90, -60, "Play Again", SgsButtonHandler.PlayAgain, TextColors.button);
            MakeButton(20, -60, "Return Home", SgsButtonHandler.ReturnHome, TextColors.button);
        }
        void CreateNewPage(Pages page)
        {
            switch (page)
            {
                case Pages.Home:
                    CreateHomePage();
                    break;
                case Pages.Tutorial:
                    CreateTutorialPage();
                    break;
                case Pages.AdvertiseGameB:
                    CreateAdvertiseGameBPage();
                    break;
                case Pages.AdvertiseGameC:
                    CreateAdvertiseGameCPage();
                    break;
                case Pages.ShipUpgrade:
                    CreateChooseAndUpgradeShipPages();
                    break;
                case Pages.Ammo:
                    CreateAmmoPage();
                    break;
                case Pages.Copilot:
                    CreateCopilotPage();
                    break;
                case Pages.GameOver:
                    CreateGameOverPage();
                    break;
            }
            gs.currentPage = page;
        }
        //this is the body of NewMenuPage()
        gs.isMenuAcceptingInput = false;  //this only gets set back to true once Systems.UserInputSystem() detects that no mousepress event(s) are occuring. Hopefully this works well with touch input too.
        DestroyCurrentPage();
        CreateNewPage(page);
    }
    public static void ButtonHandler(SgsButtonHandler sgs)
    {
        GlobalState gs = GameObject.FindObjectOfType<GlobalState>();
        GlobalReferences gr = GameObject.FindObjectOfType<GlobalReferences>();
        void HandleShipUpgradesPage(GlobalState.PlayerConfiguration.ShipUpgrade upgrade)
        {
            if (gs.playerConfiguration.upgrades[0] == GlobalState.PlayerConfiguration.ShipUpgrade.None)
            { //this is the first upgrade / time seeing this page
                gs.playerConfiguration.upgrades[0] = upgrade;
                switch(upgrade)
                {
                    case GlobalState.PlayerConfiguration.ShipUpgrade.Speed:
                        gs.playerConfiguration.shipAppearance = GlobalState.PlayerConfiguration.ShipAppearance.Speed;
                        break;
                    case GlobalState.PlayerConfiguration.ShipUpgrade.Turns:
                        gs.playerConfiguration.shipAppearance = GlobalState.PlayerConfiguration.ShipAppearance.Turns;
                        break;
                    case GlobalState.PlayerConfiguration.ShipUpgrade.Armor:
                        gs.playerConfiguration.shipAppearance = GlobalState.PlayerConfiguration.ShipAppearance.Armor;
                        break;
                    case GlobalState.PlayerConfiguration.ShipUpgrade.Attack:
                        gs.playerConfiguration.shipAppearance = GlobalState.PlayerConfiguration.ShipAppearance.Attack;
                        break;
                }
                if (gs.gameMode == GameModes.GameB || gs.gameMode == GameModes.GameC)
                { //paid players can upgrade their ship a second time
                    gs.preventDuplicateShipUpgrades = sgs;
                    NewMenuPage(Pages.ShipUpgrade);
                }
                else
                { //demo players go right into picking their ammo
                    NewMenuPage(Pages.Ammo);
                }
            }
            else
            { //this is the second upgrade / time seeing this page
                gs.playerConfiguration.upgrades[1] = upgrade;
                NewMenuPage(Pages.Ammo);
            }
        }
        void HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo ammo)
        {
            gs.playerConfiguration.ammo = ammo;  //select the ammo of choice
            if (gs.gameMode == GameModes.GameB || gs.gameMode == GameModes.GameC)
            { //paid players get one extra menu to select a copilot
                NewMenuPage(Pages.Copilot);
            }
            else
            { //demo players jump right into the match without picking a copilot
                gs.playerConfiguration.copilot = GlobalState.PlayerConfiguration.Copilot.None;
                StartMatch();
            }
        }
        void HandleShipCopilotPage(GlobalState.PlayerConfiguration.Copilot copilot)
        {
            gs.playerConfiguration.copilot = copilot; //select the copilot of choice.
            StartMatch();
        }
        void HandleReturnHomePage()
        { //if you do decide to return home, reset necessary player configurations. If you play again, keep the ones from before.
            gs.playerConfiguration.upgrades[0] = GlobalState.PlayerConfiguration.ShipUpgrade.None;
            gs.playerConfiguration.upgrades[1] = GlobalState.PlayerConfiguration.ShipUpgrade.None;
            NewMenuPage(Pages.Home);
        }
        void HandleLockedButton()
        {
            if(gs.gameMode == GameModes.GameA)
            {
                NewMenuPage(Pages.AdvertiseGameB);
            }
            else if(gs.gameMode == GameModes.GameB)
            {
                //UNCOMMENT this once you are ready to implement game c: NewMenuPage(Pages.AdvertiseGameC);
            }
        }

        switch (sgs)
        {
            case SgsButtonHandler.Tutorial:
                NewMenuPage(Pages.Tutorial);
                break;
            case SgsButtonHandler.PlayGameA:
                gs.gameMode = GameModes.GameA;
                NewMenuPage(Pages.ShipUpgrade);
                break;
            case SgsButtonHandler.PlayGameB:
                gs.gameMode = GameModes.GameB;
                NewMenuPage(Pages.ShipUpgrade);
                break;
            case SgsButtonHandler.PlayGameC:
                gs.gameMode = GameModes.GameC;
                NewMenuPage(Pages.ShipUpgrade);
                break;
            case SgsButtonHandler.UnlockGameB:
                //show text for what you can get by buying the whole game, as well as a button to bring up a platform-specific payment method
                NewMenuPage(Pages.AdvertiseGameB);
                break;
            case SgsButtonHandler.UnlockGameC:
                NewMenuPage(Pages.AdvertiseGameC);
                break;
            case SgsButtonHandler.BuyGameB:
                //IMPLEMENT: trigger a platform-specific payment method
                //IMPORTANT: Don't say `gs.isPaidPlayer = true;` quite yet. This will prompt whatever interface apple uses for in app purchases, and after they have actually paid via these means, THEN you can do the aforementioned line of code.
                //but for now, I will ignore the above comment for testing purposes
                gs.hasUnlockedGameB = true;
                NewMenuPage(Pages.Home);
                break;
            case SgsButtonHandler.BuyGameC:
                gs.hasUnlockedGameC = true;
                NewMenuPage(Pages.Home);
                break;
            case SgsButtonHandler.Armor:
                HandleShipUpgradesPage(GlobalState.PlayerConfiguration.ShipUpgrade.Armor);
                break;
            case SgsButtonHandler.Speed:
                HandleShipUpgradesPage(GlobalState.PlayerConfiguration.ShipUpgrade.Speed);
                break;
            case SgsButtonHandler.Turning:
                HandleShipUpgradesPage(GlobalState.PlayerConfiguration.ShipUpgrade.Turns);
                break;
            case SgsButtonHandler.Attack:
                HandleShipUpgradesPage(GlobalState.PlayerConfiguration.ShipUpgrade.Attack);
                break;
            case SgsButtonHandler.Bullets:
                HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo.Bullet);
                break;
            case SgsButtonHandler.Cannon:
                HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo.Cannon);
                break;
            case SgsButtonHandler.Conversion:
                HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo.Conversion);
                break;
            case SgsButtonHandler.Freeze:
                HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo.Freeze);
                break;
            case SgsButtonHandler.Poison:
                HandleShipAmmoPage(GlobalState.PlayerConfiguration.Ammo.Poison);
                break;
            case SgsButtonHandler.Navigator:
                HandleShipCopilotPage(GlobalState.PlayerConfiguration.Copilot.Navigator);
                break;
            case SgsButtonHandler.BountyHunter:
                HandleShipCopilotPage(GlobalState.PlayerConfiguration.Copilot.BountyHunter);
                break;
            case SgsButtonHandler.Mechanic:
                HandleShipCopilotPage(GlobalState.PlayerConfiguration.Copilot.Mechanic);
                break;
            case SgsButtonHandler.Princess:
                HandleShipCopilotPage(GlobalState.PlayerConfiguration.Copilot.Princess);
                break;
            case SgsButtonHandler.ReturnHome:
                HandleReturnHomePage();
                break;
            case SgsButtonHandler.PlayAgain:
                StartMatch();  //this only works if you don't change the game state from previous round, which I believe won't be a problem.
                break;
            case SgsButtonHandler.Locked:
                HandleLockedButton();
                break;
        }
    }
    public enum SgsButtonHandler
    {  //I will try to organize each button into groupings of the same menus via indentation of this enum body
        Tutorial = 21,
        PlayGameA = 0,  //from main menu
        PlayGameB = 20,
        PlayGameC = 23,
        UnlockGameB = 1,  //from main menu
        UnlockGameC = 19,

        BuyGameB = 2,  //after you click the unlock full game button from the main menu, you will enter a page that describes what the full game offers, as well as a second button (this one) giving you the option to open up an android or ios specific payment interface
        BuyGameC = 22,

        //from select ship menu
        Armor = 3,
        Speed = 4,
        Turning = 5,
        Attack = 6,

        //from select ammo menu
        Bullets = 7,
        Cannon = 8,
        Conversion = 9,
        Freeze = 10,
        Poison = 11,

        //from select copilot menu
        Navigator = 12,  //increases view radius
        BountyHunter = 13,  //?allows you to see which enemies can be destroyed for loot? or ?increases the likelihood that enemies will drop loot? or ?increases points rewarded for destroying enemies?
        Mechanic = 14,  //heals or powers up your ship every so often
        Princess = 15, //occasionally calls in NPC allies to fight against your enemies

        //from game over menu
        ReturnHome = 16,
        PlayAgain = 17, // this will allow you to play again without having to reselect all your ship configurations again. If you do want to change this, simply return to the home menu and go through the process again.

        //other
        Locked = 18

        //IMPORTANT: Elements out of order! Don't create a duplicate number and mess up serialization!
    }
}
