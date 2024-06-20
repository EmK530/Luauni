using System.Collections;
using UnityEngine;

public class StringValue : MonoBehaviour
{
    public readonly string ClassName = "StringValue";

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
    private string _value = "";
    public string Value
    {
        get { return _value; }
        set
        {
            _value = value;
        }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(false);
    }
}