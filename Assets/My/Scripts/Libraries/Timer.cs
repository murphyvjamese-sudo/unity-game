using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/*[System.Serializable] public class Timer
{  //defines standard interface of fields for the act of keeping time, while giving flexible access to these fields so that Systems.cs or a component's Awake() can handle the actual timing logic however they want. That means that timing actions like Start() Update(), Pause() and Reset() are not methods to be defined in this class.
    [SerializeField] private float durationSeconds;  //design end. exposed to inspector so that you can easily set a duration in seconds without worrying about framerate.
    [HideInInspector] public int duration;  //implementation end. duration is in frames, and initialized directly from durationSeconds. A counter in frames is much easier to work with at runtime.
    [HideInInspector] public int counter;  //implementation end. does the actual counting, and resets to duration.
    public Timer()
    {
        duration = Mathf.RoundToInt(GlobalValues.fps * durationSeconds);
        counter = duration;  //default is to start the hourglass at full, and directly modify the field in systems or Awake() if you desire a different starting location.
    }
}*/
[System.Serializable] public class Timer : MonoBehaviour
{  //defines standard interface of fields for the act of keeping time, while giving flexible access to these fields so that Systems.cs or a component's Awake() can handle the actual timing logic however they want. That means that timing actions like Start() Update(), Pause() and Reset() are not methods to be defined in this class.
    [SerializeField] private float durationSeconds;  //design end. exposed to inspector so that you can easily set a duration in seconds without worrying about framerate.
    [HideInInspector] public int duration;  //implementation end. duration is in frames, and initialized directly from durationSeconds. A counter in frames is much easier to work with at runtime.
    [HideInInspector] public int counter;  //implementation end. does the actual counting, and resets to duration.
    
    void Awake()
    {
        duration = Mathf.RoundToInt(GlobalValues.fps * durationSeconds);
        counter = duration;  //default is to start the hourglass at full, and directly modify the field in systems or Awake() if you desire a different starting location.
    }
}
