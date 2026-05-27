using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePersistent : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

