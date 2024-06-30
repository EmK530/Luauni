using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public readonly string ClassName = "TextButton";
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

    public RBXScriptSignal MouseButton1Click = new RBXScriptSignal();
    public RBXScriptSignal InputBegan = new RBXScriptSignal();
    public RBXScriptSignal InputEnded = new RBXScriptSignal();
    public RBXScriptSignal InputChanged = new RBXScriptSignal();
    public RBXScriptSignal MouseEnter = new RBXScriptSignal();
    public RBXScriptSignal MouseLeave = new RBXScriptSignal();

    public void OnPointerEnter(PointerEventData eventData) { StartCoroutine(Misc.ExecuteCoroutine(MouseEnter._fire(new object[0]))); }
    public void OnPointerClick(PointerEventData eventData) { StartCoroutine(Misc.ExecuteCoroutine(MouseButton1Click._fire(new object[0]))); }

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