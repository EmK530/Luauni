using System;
using System.Collections;
using UnityEngine;

public class Color3Value : MonoBehaviour
{
    public readonly string ClassName = "Color3Value";

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
    private Color _value;
    public Color3 Value
    {
        get { return new Color3(_value.r, _value.g, _value.b); }
        set
        {
            _value = new Color(value.r, value.g, value.b);
        }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(false);
    }
}