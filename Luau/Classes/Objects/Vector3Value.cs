using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Value : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Vector3Value";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    public UnityEngine.Vector3 initialValue;

    [SerializeField]
    private Vector3 _value;
    public Vector3 Value
    {
        get { if (_value == null) { _value = initialValue; } return _value; }
        set
        {
            _value = value;
        }
    }

    public static bool isObject = true;

    void Awake()
    {
        gameObject.SetActive(false);
    }
}