using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class VectorForce : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "VectorForce";

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

    public bool ApplyAtCenterOfMass = true;

    public Vector3 Force = new Vector3();

    public static bool isObject = true;

    void Update()
    {
        transform.parent.gameObject.GetComponent<Rigidbody>().velocity = Force;
    }
}