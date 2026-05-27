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

    async void Start()
    {
        Application.targetFrameRate = 60;
        GlobalState gs = FindObjectOfType<GlobalState>();
        IAPs iaps = FindFirstObjectByType<IAPs>();

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
            iaps.InitializeIAP(); //could have performed this by adding an Await() method to the IAP game object that manages in app purchases, but apparently it is bad to make Await() and Start() asynchronous, so I am trying to isolate it to just this one StartupManager async void Start() call.
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
            if(splashScreenCounter == 0)
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }
}
