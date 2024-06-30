using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunService : MonoBehaviour
{
    public static RBXScriptSignal RenderStepped = new RBXScriptSignal();
    public static RBXScriptSignal Heartbeat = new RBXScriptSignal();

    public static RunService instance;
    public static bool isObject = false;
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
