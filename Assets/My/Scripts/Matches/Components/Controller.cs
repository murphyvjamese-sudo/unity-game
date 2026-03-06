using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{  //a standard interface for NPCs and PCs
    public bool isNPC = true;
    [HideInInspector] public bool isLeftButtonPressed;  //recall left btn used for attacking
    [HideInInspector] public bool isRightButtonPressed;  //recall right btn used for movement
}
