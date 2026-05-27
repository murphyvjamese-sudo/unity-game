using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetCloud : MonoBehaviour
{ //a jet cloud is the small visual left behind a game object that is moving fast because it has the jet powerup enabled. This script simply applies a timer that tells the jet cloud when to get smaller and then disappear altogether. Do not confuse this class with the JetTrailVisual class, which is applied to objects that will generate jet clouds.
    [HideInInspector] public int jetCloudDuration;
    [HideInInspector] public int jetCloudCounter;
    [HideInInspector] public SpriteRenderer sr;

    void Awake()
    {
        jetCloudDuration = Mathf.RoundToInt(GlobalValues.fps * .5f);  //make a jet cloud exist for half a second.
        jetCloudCounter = jetCloudDuration;
        sr = GetComponent<SpriteRenderer>();
    }
}
