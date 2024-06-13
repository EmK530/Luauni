using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicatedFirst : MonoBehaviour
{
    public static ReplicatedFirst instance;
    public static bool isObject = true;
    public static bool isStatic = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
            source.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
