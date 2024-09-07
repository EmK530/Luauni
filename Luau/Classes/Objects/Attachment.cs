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

    [SerializeField]
    public UnityEngine.Vector3 position;
    [SerializeField]
    public UnityEngine.Vector3 orientation;

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }
    public Vector3 Orientation
    {
        get { return orientation; }
        set { orientation = value; }
    }

    public CFrame CFrame
    {
        get { 
            return new CFrame(Position) * CFrame._angles(
                orientation.x * Mathf.Deg2Rad,
                orientation.y * Mathf.Deg2Rad,
                orientation.z * Mathf.Deg2Rad
            );
        }
    }
    public CFrame ToWorldSpace()
    {
        UnityEngine.Vector3 vec = transform.parent.rotation.eulerAngles;
        CFrame cf = new CFrame((Vector3)transform.parent.position + Position) * CFrame._angles(
            vec.x * Mathf.Deg2Rad,
            vec.y * Mathf.Deg2Rad,
            vec.z * Mathf.Deg2Rad
        ) * CFrame._angles(
            orientation.x * Mathf.Deg2Rad,
            orientation.y * Mathf.Deg2Rad,
            orientation.z * Mathf.Deg2Rad
        );
        return CFrame * cf;
    }

    Rigidbody rb;

    /*[Inspectable] [SerializeField]
    private Quaternion _cfrq {
        get { return _cframe; }
        set { _cframe = new CFrame(_cframe.p.X, _cframe.p.Y, _cframe.p.Z, value.x, value.y, value.z, value.w); }
    }*/

    public static bool isObject = true;
}