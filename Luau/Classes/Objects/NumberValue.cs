using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberValue : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "NumberValue";

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

    [SerializeField]
    private double _value;
    public double Value
    {
        get { return _value; }
        set
        {
            _value = value;
        }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(false);
    }
}