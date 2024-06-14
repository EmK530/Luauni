using System.Collections;
using UnityEngine;

public class ImageButton : MonoBehaviour
{
    public RBXScriptSignal InputBegan = new RBXScriptSignal();
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