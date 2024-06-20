using System.Collections;
using TMPro;
using UnityEngine;

public class TextLabel : MonoBehaviour
{
    public readonly string ClassName = "TextLabel";

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

    private TextMeshProUGUI _element;
    public object Text
    {
        get {
            if (_element == null){_element = GetComponent<TextMeshProUGUI>();}
            return _element.text; }
        set
        {
            if (_element == null) { _element = GetComponent<TextMeshProUGUI>(); }
            _element.text = value.ToString();
        }
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
        _element = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(_visible);
    }
}