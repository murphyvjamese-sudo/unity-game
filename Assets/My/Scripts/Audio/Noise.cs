using UnityEngine;

public class Noise : MonoBehaviour
{
    //just a flag for now, used in Systems.AudioSystem(), to ensure that objects with this component AND an AudioSource component will be destroyed once the audio clip stops playing. (See Noise prefab). This is a much more robust way to implement sound in future games. Since you need a game object to carry an AudioSource component, to play an audio clip, you should create one "Noise" game object any time you need to play any kind of music or sound effect. Then, deletion of these objects is purely based on a stopped audio clip from within Systems.AudioSystem, but how you stop playing the audio could be determined by inspector (setting if this loops or not), or in the script itself. For example, you will play a sound on loop until a powerup runs out, then dynamically Stop() the audio clip, indirectly triggering the Noise game object to soon delete itself.
}
