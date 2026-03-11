using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{ //While the vast majority of my game will conform to ECS design principles, I will not have my startup functions (including showing my company logo, conform to this sytem. I want them to be their own isolated unit.)
    int splashScreenDuration;
    int splashScreenCounter;
    
    void Awake()
    {
        splashScreenDuration = 50;
        splashScreenCounter = splashScreenDuration;
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
