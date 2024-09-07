using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

// THIS IS A DUMMY SCRIPT UNTIL TRAIL GETS DEVELOPED

public class Trail : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Trail";

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

    public bool Enabled
    {
        get { return gameObject.activeSelf; }
        set { gameObject.SetActive(value); }
    }

    public static bool isObject = true;
}