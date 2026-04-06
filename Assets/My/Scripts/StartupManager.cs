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

    async Task ConnectToLeaderboards()
    {
        await UnityServices.InitializeAsync();
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch
        {
            Debug.Log("no internet");
        }
    }

    async Task FetchHighScores(GlobalState gs)
    { //uncomment code related to game c once ready for that update to your app
        LeaderboardEntry leA = await LeaderboardsService.Instance.GetPlayerScoreAsync("gameA");
        LeaderboardEntry leB = await LeaderboardsService.Instance.GetPlayerScoreAsync("gameB");
        //LeaderboardEntry leC = await LeaderboardsService.Instance.GetPlayerScoreAsync("gameC");
        if(leA != null)
        {
            gs.highScoreA = (int)leA.Score;
        }
        if(leB != null)
        {
            gs.highScoreB = (int)leB.Score;
        }
        /*if(leC != null)
        {
            gs.highScoreC = (int)leC.Score;
        }*/
    }
    
    async void Awake()
    {
        await ConnectToLeaderboards();
        splashScreenDuration = 50;
        splashScreenCounter = splashScreenDuration;
    }

    async Task Start()
    {
        GlobalState gs = FindObjectOfType<GlobalState>();

        if(Application.internetReachability == NetworkReachability.NotReachable && gs != null)
        { //load offline scores if not connected to internet
            gs.highScoreA = SecurePlayerPrefs.GetInt("gameA");
            gs.highScoreB = SecurePlayerPrefs.GetInt("gameB");
            //gs.highScoreC = SecurePlayerPrefs.GetInt("gameC");  //uncomment once you are ready for the game c update
        }
        else if(Application.internetReachability != NetworkReachability.NotReachable)
        { //load online highscore if connected to internet.
            Debug.Log("fetched high scores");
            await FetchHighScores(gs);
        }
    }

    void FixedUpdate()
    {
        splashScreenCounter--;
        if(splashScreenCounter == 0)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
