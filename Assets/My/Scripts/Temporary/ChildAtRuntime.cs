using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildAtRuntime : MonoBehaviour
{  //applying this to an empty prefab proves that a child object appended at runtime will indeed appear in the object hierarchy window after you hit play
    void Awake()
    {
        GameObject child = new GameObject();
        child.transform.parent = transform;
    }
}
