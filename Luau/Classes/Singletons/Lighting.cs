using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    [SerializeField]
    private Light Light;

    public double Brightness
    {
        get { return Light.intensity; }
        set
        {
            Light.intensity = (float)value;
        }
    }

    private Color3 _colorshift_top;
    public Color3 ColorShift_Top
    {
        get { return _colorshift_top; }
        set
        {
            Logging.Warn("Use of unsupported property, Lighting.ColorShift_Top", "Lighting:Property");
            _colorshift_top = value;
        }
    }

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
