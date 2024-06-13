using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workspace : MonoBehaviour
{
    private static double _gravity = 196.2;
    public static double Gravity
    {
        get { return _gravity; }
        set
        {
            _gravity = value;
            Physics.gravity = new Vector3(0, (float)value, 0);
        }
    }


    public static Workspace instance;
    public static bool isObject = true;
    public static bool isStatic = true;
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
