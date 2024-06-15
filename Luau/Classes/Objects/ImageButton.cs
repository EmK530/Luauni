using System.Collections;
using UnityEngine;

public class ImageButton : MonoBehaviour
{
    public readonly string ClassName = "ImageButton";

    public RBXScriptSignal InputBegan = new RBXScriptSignal();
    public RBXScriptSignal MouseButton1Down = new RBXScriptSignal();
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

    void Start()
    {
        gameObject.SetActive(_visible);
    }
}