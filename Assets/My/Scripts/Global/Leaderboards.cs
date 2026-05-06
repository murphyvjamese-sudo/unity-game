using UnityEngine;
using Unity.Services.Leaderboards;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderboardUIData
{
    public double worldRecordA;
    public double scoreA;
    public int rankA;
    public int totalPlayersA;
    public float percentileA;

    public double worldRecordB;
    public double scoreB;
    public int rankB;
    public int totalPlayersB;
    public float percentileB;

    public double worldRecordC;
    public double scoreC;
    public int rankC;
    public int totalPlayersC;
    public float percentileC;
}
public static class Leaderboards
{  //handles how to connect to PlayerPrefs via SecurePlayerPrefs for offline high scores, as well as online high scores via Unity Dashboard
    public static async Task ConnectToLeaderboards()
    {
        await UnityServices.InitializeAsync();  //connects to the unity dashboard so you can access the leaderboards. Because this method call uses await, you can safely assume this is a Task (or other custom awaitable that implements the same pattern as Task), not a regular synchronous method
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            //D\ebug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch
        {
            //D\ebug.Log("no internet");
        }
    }

    public static async Task SyncLeaderboards(GlobalState gs, string chosenId = "none", int score = 0)
    { //there are 3 leaderboards for each game a, b, and c. Each of these leaderboards has 3 sources to pull from: offline, online, and thisGame. This method aims to compare these three numbers for each leaderboard, and update all three sources with the highest number once it is found.
        async Task<int> GetLeaderboard(string id, int thisGame = 0)
        { //collects data from the three sources that define a given leaderboard, compares them, and returns the highest of the three
            int[] sources = new int[3]; //ordered array: [online, offline, thisGame]

            //populate sources from online and offline avenues
            if(Application.internetReachability != NetworkReachability.NotReachable)
            { //if connected to the internet
                try
                { //if you successfully connect to leaderboard data, use that to populate high scores on screen
                    LeaderboardEntry leaderboardEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(id);  //get online source
                    sources[0] = (int)leaderboardEntry.Score;
                }
                catch
                { //usually happens when you can connect to leaderboard, but you haven't submitted any entries yet as this is your first time playing
                    //do nothing, online source remains 0
                }
            }
            sources[1] = SecurePlayerPrefs.GetInt(id, 0);  //get offline source, defaulting to 0 if unavailable
            sources[2] = thisGame;

            //compare sources to return the highest value
            if(sources[0] > sources[1])
            {
                if(sources[0] > sources[2])
                {
                    return sources[0];
                }
                else
                {
                    return sources[2];
                }
            }
            else
            {
                if(sources[1] > sources[2])
                {
                    return sources[1];
                }
                else
                {
                    return sources[2];
                }
            }
        }
        async Task SetLeaderboard(GlobalState gs, string id, int score)
        { //updates all three sources that define a given leaderboard with the new highest score.
            if(Application.internetReachability != NetworkReachability.NotReachable)
            { //if connected to the internet
                try
                { //if you successfully connect to leaderboard data, use that to populate high scores on screen
                    if(score > 0)
                    { //don't submit scores less than 0. Otherwise you'd get added to the game b leaderboards with a score of zero after playing game a
                        LeaderboardEntry leaderboardEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(id, score); //update online (Unity Dashboard) score
                    }
                }
                catch
                { //if you can't connect to leaderboard data, even though an internet connection has been made (this usually happens when the player has yet to add a high score entry to the leaderboard), use the offline, locally stored leaderboard, which already has a default to set to 0 if no entries exist there yet either.
                    //do nothing
                }
            }
            SecurePlayerPrefs.SetInt(id, score);  //update offline (SecurePlayerPrefs) score
            switch(id)
            { //update global state
                case "gameA":
                gs.highScoreA = score;
                break;
                case "gameB":
                gs.highScoreB = score;
                break;
                case "gameC":
                gs.highScoreC = score;
                break;
            }
        }
        switch(chosenId)
        {
            case "gameA":
            await SetLeaderboard(gs, "gameA", await GetLeaderboard("gameA", score));
            await SetLeaderboard(gs, "gameB", await GetLeaderboard("gameB"));
            await SetLeaderboard(gs, "gameC", await GetLeaderboard("gameC"));
            break;
            case "gameB":
            await SetLeaderboard(gs, "gameA", await GetLeaderboard("gameA"));
            await SetLeaderboard(gs, "gameB", await GetLeaderboard("gameB", score));
            await SetLeaderboard(gs, "gameC", await GetLeaderboard("gameC"));
            break;
            case "gameC":
            await SetLeaderboard(gs, "gameA", await GetLeaderboard("gameA"));
            await SetLeaderboard(gs, "gameB", await GetLeaderboard("gameB"));
            await SetLeaderboard(gs, "gameC", await GetLeaderboard("gameC", score));
            break;
            default: //used when you first open the app. No new score will be given from the game that just finished, so you don't have to consider that for comparison.
            await SetLeaderboard(gs, "gameA", await GetLeaderboard("gameA"));
            await SetLeaderboard(gs, "gameB", await GetLeaderboard("gameB"));
            await SetLeaderboard(gs, "gameC", await GetLeaderboard("gameC"));
            break;
        }
    }

    public static async Task<LeaderboardUIData> GetLeaderboardUIData()
    {
        //helper methods
        async Task<double> GetWorldRecord(string leaderboardId)
        {
            try
            {
                var leaderboardScores = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
                return leaderboardScores.Results[0].Score;
            }
            catch
            {
                return 0;
            }
        }
        async Task<double> GetScore(string leaderboardId)
        {
            try
            {
                LeaderboardEntry leaderboardEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
                return leaderboardEntry.Score;
            }
            catch
            {
                return 0;
            }
        }
        async Task<int> GetRank(string leaderboardId)
        {
            try
            {
                LeaderboardEntry leaderboardEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
                return leaderboardEntry.Rank + 1;  //almost positive these are 0-indexed
            }
            catch
            {
                return 0;
            }
        }
        async Task<int> GetTotalPlayers(string leaderboardId)
        {
            try
            {
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 1 });
                return scoresResponse.Total;
            }
            catch 
            { //catches any error, most likely ones might be that the leaderboard got misconfigured, doesn't exist, or potentially thrown if no entries exist yet
                return 0;
            }
        }

        LeaderboardUIData luid = new LeaderboardUIData();

        luid.worldRecordA = await GetWorldRecord("gameA");
        luid.scoreA = await GetScore("gameA");
        luid.rankA = await GetRank("gameA");
        luid.totalPlayersA = await GetTotalPlayers("gameA");
        if(luid.totalPlayersA > 0)
        { //avoid divide by zero
            luid.percentileA = (float)luid.rankA / (float)luid.totalPlayersA;
        }

        luid.worldRecordB = await GetWorldRecord("gameB");
        luid.scoreB = await GetScore("gameB");
        luid.rankB = await GetRank("gameB");
        luid.totalPlayersB = await GetTotalPlayers("gameB");
        if(luid.totalPlayersB > 0)
        { //avoid divide by zero
            luid.percentileB = (float)luid.rankB / (float)luid.totalPlayersB;
        }

        /*UNCOMMENT once ready for game c app update
        luid.worldRecordC = await GetWorldRecord("gameC");
        luid.scoreC = await GetScore("gameC");
        luid.rankC = await GetRank("gameC");
        luid.totalPlayersC = await GetTotalPlayers("gameC");
        if(luid.totalPlayersC > 0)
        { //avoid divide by zero
            luid.percentileC = (float)luid.rankC / (float)luid.totalPlayersC;
        }*/

        return luid;
    }
}
