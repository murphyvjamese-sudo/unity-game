using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Leaderboards;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using System.Threading.Tasks;

public class StartupManager : MonoBehaviour
{ //While the vast majority of my game will conform to ECS design principles, I will not have my startup functions (including showing my company logo, conform to this sytem. I want them to be their own isolated unit.)
    int splashScreenDuration;
    int splashScreenCounter;
    public bool isTestingIsolated;
    private IAPs iaps;
    public bool isMenuSceneLoaded; //this is false, so that IAPs.cs can create the menu page right after the scene has been loaded, but the bulk of async IAP initialization (which can take a bit of time) can still take place in the background while the splashscreen is being shown

    async void Start()
    {
        Application.targetFrameRate = 60;
        GlobalState gs = FindObjectOfType<GlobalState>();
        iaps = FindObjectOfType<IAPs>();

        if(isTestingIsolated)
        {
            //this might not be necessary. I implemented this if/else because I was having trouble getting a scene to work in isolation for quick and easy debugging without menus and the likes
        }
        else
        {
            splashScreenDuration = 100;
            splashScreenCounter = splashScreenDuration;

            await Leaderboards.ConnectToLeaderboards();
            await Leaderboards.SyncLeaderboards(gs);
        }
    }

    void FixedUpdate()
    {
        if(isTestingIsolated)
        {
            
        }
        else
        {
            splashScreenCounter--;
            if(splashScreenCounter == 2)
            { //load menu scene a little bit early, so that InitializeIAP() can operate on a scene that is fully initialized with Start() and Awake() and the likes
                SceneManager.LoadScene("Menu");
            }
            if(splashScreenCounter == 0)
            {
                if(iaps != null)
                {
                    //important: InitializeIAP() chains down to call GrantProduct() which will change the scene to the menu scene so that you can create and destroy menu pages without creating bugs
                    iaps.InitializeIAP(); //asynchronous, make sure the object that holds this method is not destroyed between scene changes.
                }
            }
        }
    }
}
