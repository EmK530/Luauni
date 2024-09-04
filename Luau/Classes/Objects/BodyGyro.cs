using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

[Inspectable]
public class BodyGyro : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "BodyGyro";

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

    public CFrame CFrame = new CFrame();

    Rigidbody rb;

    /*[Inspectable] [SerializeField]
    private Quaternion _cfrq {
        get { return _cframe; }
        set { _cframe = new CFrame(_cframe.p.X, _cframe.p.Y, _cframe.p.Z, value.x, value.y, value.z, value.w); }
    }*/

    public static bool isObject = true;

    void Awake()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = transform.parent.gameObject.AddComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        rb.rotation = ((Quaternion)CFrame).normalized;
    }
}