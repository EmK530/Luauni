using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

[Inspectable]
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

    [Inspectable]
    public bool ApplyAtCenterOfMass = true;

    [Inspectable] [SerializeField]
    private UnityEngine.Vector3 _force;

    public Vector3 Force {
        get { return _force; }
        set { _force = value; }
    }

    public static bool isObject = true;

    void FixedUpdate()
    {
        transform.parent.gameObject.GetComponent<Rigidbody>().AddForce(_force);
    }
}