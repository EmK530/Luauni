using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Beam";

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

    LineRenderer lr;

    double w0 = 0;
    double w1 = 0;

    public double Width0
    {
        get { return w0; }
        set { w0 = value; lr.startWidth = (float)value; }
    }
    public double Width1
    {
        get { return w1; }
        set { w1 = value; lr.endWidth = (float)value; }
    }

    public static bool isObject = true;

    void Awake()
    {
        if (lr == null) { lr = GetComponent<LineRenderer>(); }
    }
}