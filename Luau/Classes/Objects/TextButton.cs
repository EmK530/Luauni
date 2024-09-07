using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Enum;

[ExecuteInEditMode]
public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    RectTransform rt;

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

    public static bool isObject = true;

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
        _anchorPoint = new Vector2(rt.pivot.x, 1f - rt.pivot.y);
        _size = new UDim2(__size.x, __size.y, __size.z, __size.w);
        _position = new UDim2(__position.x, __position.y, __position.z, __position.w);
        CalculateTransform();
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

    void Start()
    {
        gameObject.SetActive(_visible);
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
        if (!Application.isPlaying)
        {
            PerformStartup();
            CalculateTransform();
        }
    }
}