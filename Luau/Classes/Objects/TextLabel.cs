using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static Enum;

[ExecuteInEditMode]
public class TextLabel : MonoBehaviour
{
    public readonly string ClassName = "TextLabel";

    private RectTransform rt;

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
    private SizeConstraint _sizeConstraint;
    public SizeConstraint SizeConstraint
    {
        get { return _sizeConstraint; }
        set
        {
            _sizeConstraint = value;
            CalculateTransform();
        }
    }

    [SerializeField]
    private UnityEngine.Vector2 _anchorOffset = new UnityEngine.Vector2(0, 0);
    private Vector2 _anchorPoint = new Vector2(0, 0);
    public Vector2 AnchorPoint
    {
        get { return _anchorPoint; }
        set
        {
            _anchorPoint = value;
            CalculateTransform();
        }
    }

    [SerializeField]
    private Vector4 __position = new Vector4(0.5f, 0, 0.5f, 0);
    private UDim2 _position = new UDim2(0, 0, 0, 0);
    public UDim2 Position
    {
        get { return _position; }
        set
        {
            _position = value;
            CalculateTransform();
        }
    }

    [SerializeField]
    private Vector4 __size = new Vector4(1f, 0, 1f, 0);
    private UDim2 _size = new UDim2(0, 0, 0, 0);
    public UDim2 Size
    {
        get { return _size; }
        set
        {
            _size = value;
            CalculateTransform();
        }
    }

    UnityEngine.Vector2 previousScreenResolution = new UnityEngine.Vector2(0, 0);

    void CalculateTransform()
    {
        previousScreenResolution = new UnityEngine.Vector2(Screen.width, Screen.height);
        if (rt == null) { PerformStartup(); }
        float sx = (float)_size.Scale.X,
            sy = (float)_size.Scale.Y,
            osx = (float)_size.Offset.X,
            osy = (float)_size.Offset.Y,
            ax = (float)_anchorPoint.X,
            ay = (float)_anchorPoint.Y,
            px = (float)_position.Scale.X,
            py = (float)_position.Scale.Y;
        UnityEngine.Vector2 parentSize = rt.parent.GetComponent<RectTransform>().rect.size;
        switch (_sizeConstraint)
        {
            case SizeConstraint.RelativeYY:
                sx *= parentSize.y / parentSize.x;
                osx = osy;
                break;
            case SizeConstraint.RelativeXX:
                sy *= parentSize.x / parentSize.y;
                osy = osx;
                break;
        }
        float xMin = px - (sx * ax),
            xMax = xMin + sx,
            yMin = 1f - py - (sy * (1f - ay)),
            yMax = yMin + sy;
        rt.anchorMin = new UnityEngine.Vector2(Mathf.Min(xMin, xMax), Mathf.Min(yMin, yMax));
        rt.anchorMax = new UnityEngine.Vector2(Mathf.Max(xMin, xMax), Mathf.Max(yMin, yMax));
        rt.offsetMin = new UnityEngine.Vector2((float)_position.Offset.X - (osx * ax), -(float)_position.Offset.Y - (osy * (1f - ay)));
        rt.offsetMax = new UnityEngine.Vector2(rt.offsetMin.x + osx, rt.offsetMin.y + osy);
        rt.sizeDelta = new UnityEngine.Vector2(osx, osy);
    }

    void PerformStartup()
    {
        rt = GetComponent<RectTransform>();
        gameObject.SetActive(Application.isPlaying ? _visible : true);
        _anchorPoint = new Vector2(rt.pivot.x + _anchorOffset.x, 1f - rt.pivot.y - _anchorOffset.y);
        _size = new UDim2(__size.x, __size.y, __size.z, __size.w);
        _position = new UDim2(__position.x, __position.y, __position.z, __position.w);
        CalculateTransform();
    }

    private double _rotation = 0;
    public double Rotation
    {
        get { return _rotation; }
        set
        {
            if (rt == null) { rt = GetComponent<RectTransform>(); }
            _rotation = value;
            float offset = 0f;
            RectTransform p = rt.parent.GetComponent<RectTransform>();
            if(p != null)
            {
                offset = -p.rotation.eulerAngles.z;
            }
            transform.rotation = Quaternion.Euler(0, 0, Convert.ToSingle(-_rotation) - offset);
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
        rt = GetComponent<RectTransform>();
        PerformStartup();
    }

    void Update()
    {
        UnityEngine.Vector2 currentScreenResolution = new UnityEngine.Vector2(Screen.width, Screen.height);
        if (currentScreenResolution != previousScreenResolution)
        {
            CalculateTransform();
            previousScreenResolution = currentScreenResolution;
        }
        if (Application.isPlaying)
            return;
        PerformStartup();
        CalculateTransform();
    }
}