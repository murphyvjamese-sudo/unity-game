using UnityEngine;

public class StartSlowly : MonoBehaviour
{
    [HideInInspector] public int slowDuration;
    [HideInInspector] public int slowCounter;
    [HideInInspector] public Kinematics.Speed rememberedSpeed; //remember what this object's speed was originally (frazpow missile and invasion fighter will both use this, and you will thus need to remember two different speeds to return to after this component has finished slowing them down initially.) 

    void Awake()
    {
        slowDuration = 150;
        slowCounter = slowDuration;
        
        Kinematics kinematics = GetComponent<Kinematics>();
        if(kinematics != null)
        {
            rememberedSpeed = kinematics.speed;
            kinematics.speed = Kinematics.Speed.SlowPlayer; //(at time of comment) frazpow = 1.36, invasion fighter = 1.1, medium enemy = .42, slow player = .9
        }
    }

    void FixedUpdate()
    {
        
    }
}
