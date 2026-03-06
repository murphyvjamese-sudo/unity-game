using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetTrailVisual : MonoBehaviour
{ //applied to the game object that will spawn a trail of jet clouds behind it, not the jet cloud object itself.
    GlobalReferences gr;
    public int moduloInterval;  //idea is to make a new jetCloud appear behind the spaceship every half second or so, so it visually indicates that you are moving fast.

    void Awake()
    {
        gr = FindObjectOfType<GlobalReferences>();
        moduloInterval = Mathf.RoundToInt(GlobalValues.fps * .1f);
    }
    public void SpawnJetCloud()
    {
        if (gr != null)
        {
            GameObject jetCloud = Instantiate(gr.JetCloud);
            jetCloud.transform.position = new Vector2(transform.position.x, transform.position.y);
        }
        else
        {
            Debug.LogWarning("Jim. Why is gr null?");
        }
    }
}
