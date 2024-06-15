using System.Collections;
using UnityEngine;

public class TextButton : MonoBehaviour
{
    public readonly string ClassName = "TextButton";
    public string Name
    {
        get { return transform.name; }
        set { transform.name = value; }
    }
    
    public RBXScriptSignal MouseButton1Click = new RBXScriptSignal();
    public RBXScriptSignal InputBegan = new RBXScriptSignal();
    public RBXScriptSignal InputEnded = new RBXScriptSignal();
    public RBXScriptSignal InputChanged = new RBXScriptSignal();

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