using System.Collections;
using UnityEngine;

public class Frame : MonoBehaviour
{
    [SerializeField]
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
    [HideInInspector]
    public GameObject source;

    void Start()
    {
        source = gameObject;
        gameObject.SetActive(_visible);
    }
}