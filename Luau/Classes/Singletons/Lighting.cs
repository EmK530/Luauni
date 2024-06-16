using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    public static Lighting instance;
    public static bool isObject = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
