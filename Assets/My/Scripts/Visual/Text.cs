using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Text : MonoBehaviour
{  //Each text component will have its own instance of chars to pull from. I wonder if that is good for performance or bad?
    
    //public int width;  //width of entire text box (text won't bleed past this boundary. Also used for button selection purposes)
    //public int height;  //height of entire text box (long messages can bleed past. Primarily for button selection purposes)
    //INCLUDE when you do a full-color game: public Color32 fillColor;
    //INCLUDE when you do a full-color game: public Color32 strokeColor;

    public bool isCameraRelative;  //set to true to ask this text to try and find a camera to sync its position relative to.
    public bool isInvertedColor;  //helpful for black and white games
    public string message;  //can set in inspector or at runtime
    public DataBind dataBind;  //set in inspector. This is an sgs mapping, so you might update this enum for each new game you make
    public Material material;  //set in inspector
    
    [HideInInspector] public float xPreCamera;  //remember original coordinates so you can correctly place the text relative to the camera.
    [HideInInspector] public float yPreCamera;
    [HideInInspector] public CameraController cameraRelative;  //stays in the same place on the screen even when the camera moves
    [HideInInspector] public bool isFirstPrint = true;
    [HideInInspector] public Sprite[] letters;
    [HideInInspector] public int LETTER_WIDTH;
    [HideInInspector] public int LETTER_HEIGHT;
    [HideInInspector] public int cursorX;  //x coord for next char in message
    [HideInInspector] public int cursorY;  //y coord for next char in message
    [HideInInspector] public string oldMessage;  //if this is updated to something different than the message field, Systems.cs will know to reprint the letter sprites

    public enum DataBind
    {
        None = 0,
        CurrentScore = 1,
        HighScoreA = 2,
        HighScoreB = 3,
        HighScoreC =4
    }

    void Awake()
    {
        oldMessage = message;
        LETTER_WIDTH = 7;  //this might need to be changed
        LETTER_HEIGHT = 12;  //definitely accurate
        cursorX = 0;/*Mathf.RoundToInt(transform.position.x);*/
        cursorY = 0;/*Mathf.RoundToInt(transform.position.y);*/
        xPreCamera = transform.position.x;
        yPreCamera = transform.position.y;
        try
        {
            letters = FindObjectOfType<GlobalReferences>().typography;
        }
        catch
        {
            Debug.LogWarning("Jim. Didn't find typography from PixelBubble.png");
        }
    }

    void Start()
    {
        if(isCameraRelative)
        {
            cameraRelative = FindObjectOfType<CameraController>();
        }
    }
}
