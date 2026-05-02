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

        if(isTestingIsolated)
        {
            
        }
        else
        {
            splashScreenDuration = 50;
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
            if(splashScreenCounter == 0)
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }
}
