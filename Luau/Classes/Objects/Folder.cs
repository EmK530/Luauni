using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Folder : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Decal";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    public dynamic Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    public RBXScriptSignal ChildAdded = new RBXScriptSignal();
    public RBXScriptSignal ChildRemoved = new RBXScriptSignal();

    public static bool isObject = true;
    
    void Start()
    {
        
    }
}
