using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextButton : MonoBehaviour, IPointerEnterHandler
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
    public RBXScriptSignal MouseEnter = new RBXScriptSignal();
    public RBXScriptSignal MouseLeave = new RBXScriptSignal();

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseEnter._fire(new object[0]);
    }

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