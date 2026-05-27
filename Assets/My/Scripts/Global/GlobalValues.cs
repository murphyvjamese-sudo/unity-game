using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValues
{  //values that can be accessed anywhere, and don't even require an instance to exist.
    public const int pixelsInUnit = 4;  //should really probably be calculated dynamically in relation to camera orthographic size vs screen height, but I am being lazy as I do not anticipate using this variable often or changing my camera / logical pixel dimensions this late into project dev (3/26/26)
    public const int unitsInTile = 100;
    public const int unitsInScreen = 200;
    public static readonly int fps = Mathf.RoundToInt(1f/Time.fixedDeltaTime);  //how many frames there are per second. For CCh, (and probably every game I make) this will be 50
}
