using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    public static Mouse instance;

    public static double X;
    public static double Y;

    public static RBXScriptSignal Button1Down = new RBXScriptSignal();
    public static RBXScriptSignal Button1Up = new RBXScriptSignal();
    public static RBXScriptSignal Button2Down = new RBXScriptSignal();
    public static RBXScriptSignal Button2Up = new RBXScriptSignal();
    public static RBXScriptSignal Idle = new RBXScriptSignal();
    public static RBXScriptSignal Move = new RBXScriptSignal();
    public static RBXScriptSignal WheelBackward = new RBXScriptSignal();
    public static RBXScriptSignal WheelForward = new RBXScriptSignal();

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

    void Update()
    {
        X = Input.mousePosition.x;
        Y = Input.mousePosition.y;
    }
}
