using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Camera";
    public string CameraType = "Scriptable";

    public UnityEngine.Camera component;

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    public double FieldOfView
    {
        get { return component.fieldOfView; }
        set
        {
            component.fieldOfView = Convert.ToSingle(value);
        }
    }

    private CFrame _cframe;
    public CFrame CFrame
    {
        get { return _cframe; }
        set
        {
            _cframe = value;
            transform.position = _cframe.Position;
            transform.rotation = _cframe.Rotation;
        }
    }

    public static Camera instance;
    public static bool isObject = true;
    public static GameObject source;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
            component = gameObject.GetComponent<UnityEngine.Camera>();
            _cframe = new CFrame(transform.position, transform.rotation.eulerAngles);
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}