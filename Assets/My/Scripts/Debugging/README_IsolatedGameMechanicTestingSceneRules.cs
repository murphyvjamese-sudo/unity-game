using UnityEngine;

public class README_IsolatedGameMechanicTestingSceneRules : MonoBehaviour //disable the object in the scene that carries this, and don't use this in production. This is just a note that I will see in the scene so I remember which limitations exist when testing in an environment which isolates variables but is also less stable in the sense that it is removed from context. That context is detailed here.
{
    /*README
    - Camera not working? You probably need to set the z position to -10 or something. When you drag a new one into the scene, it automatically goes to 0, which doesn't work with the other objects in Cosmic Chase, at least
    - Make sure you use GlobalState.PlayerConfiguration (accessible in the scene through the Globals object) to establish which attributes the player ship should have. This global data is used to remember what you select using the menu buttons to load out your ship, and it overrides any inspector fields you set on the instance and the prefab. Since you don't use these menus / buttons when testing in isolation, you must configure the Globals instance in the scene manually, not the player's inspector fields! If you leave it as something like Navigator, it will spawn powerups directly on top of the player, since there is no World prefab when testing in isolation, leading to confusing bugs. These are the "unstable" limitations to consider / avoid when testing in isolation (but remember, the workflow of testing/debugging in isolation is much more efficient and simple if you know what you are doing and remember these limitations. Ideally in the future you might design your system to be more robust and capable of debugging situations like these, but that would also dampen performance of your production code, so idk exactly what the best approach is...)

    */
}
