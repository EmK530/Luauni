using System.Collections;
using TMPro;
using UnityEngine;

public class ScreenGui : MonoBehaviour
{
    public readonly string ClassName = "ScreenGui";

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

    public Vector2 AbsoluteSize
    {
        get { return new Vector2(Screen.width, Screen.height); }
    }

    [SerializeField]
    private bool _enabled;
    public bool Enabled
    {
        get { return _enabled; }
        set { _enabled = value; gameObject.SetActive(value); }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(_enabled);
    }
}