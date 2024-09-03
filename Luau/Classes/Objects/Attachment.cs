using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Attachment : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Attachment";

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

    public Vector3 Position = new Vector3();

    Rigidbody rb;

    /*[Inspectable] [SerializeField]
    private Quaternion _cfrq {
        get { return _cframe; }
        set { _cframe = new CFrame(_cframe.p.X, _cframe.p.Y, _cframe.p.Z, value.x, value.y, value.z, value.w); }
    }*/

    public static bool isObject = true;
}