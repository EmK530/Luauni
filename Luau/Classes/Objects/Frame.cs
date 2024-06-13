using System.Collections;
using UnityEngine;

public class Frame : MonoBehaviour
{
    private bool _visible = true;
    public bool Visible
    {
        get { return _visible; }
        set
        {
            _visible = value;
            gameObject.SetActive(value);
        }
    }

    public static bool isObject = true;
    public static bool isStatic = false;

    void Start()
    {
        _visible = gameObject.activeSelf;
    }
}