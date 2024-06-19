using System;
using System.Collections;
using UnityEngine;

public class IntValue : MonoBehaviour
{
    public readonly string ClassName = "IntValue";

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    [SerializeField]
    private long _value = 0;
    public double Value
    {
        get { return _value; }
        set
        {
            _value = Convert.ToInt64(value);
        }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(false);
    }
}