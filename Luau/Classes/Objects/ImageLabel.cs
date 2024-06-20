using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Enum;

[ExecuteInEditMode]
public class ImageLabel : MonoBehaviour
{
    public readonly string ClassName = "ImageLabel";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    private RectTransform rt;

    [SerializeField]
    public Sprite _sprite;
    [SerializeField]
    public Vector2Int _imageRectOffset;
    [SerializeField]
    public Vector2Int _imageRectSize;

    public Vector2 ImageRectOffset
    {
        get { return new Vector2(_imageRectOffset.x, _imageRectOffset.y); }
        set {
            _imageRectOffset.x = Convert.ToInt32(value.X);
            _imageRectOffset.y = Convert.ToInt32(value.Y);
            SetImageRect();
        }
    }

    public Vector2 ImageRectSize
    {
        get { return new Vector2(_imageRectSize.x, _imageRectSize.y); }
        set
        {
            _imageRectSize.x = Convert.ToInt32(value.X);
            _imageRectSize.y = Convert.ToInt32(value.Y);
            SetImageRect();
        }
    }

    void SetImageRect()
    {
        Image img = GetComponent<Image>();
        if (_sprite != null)
        {
            Vector2Int sizeCopy = _imageRectSize;
            if (sizeCopy.x <= 0) { sizeCopy.x = _sprite.texture.width; }
            if (sizeCopy.y <= 0) { sizeCopy.y = _sprite.texture.height; }
            Rect rect = new Rect(_imageRectOffset.x, _sprite.texture.height - _imageRectOffset.y - sizeCopy.y, sizeCopy.x, sizeCopy.y);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            img.sprite = Sprite.Create(_sprite.texture, rect, pivot);
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
    private Vector4 __position = new Vector4(0.5f,0,0.5f,0);
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
            _rotation = value;
            transform.rotation = Quaternion.Euler(0,0,Convert.ToSingle(-_rotation));
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
        rt = GetComponent<RectTransform>();
        SetImageRect();
        PerformStartup();
        gameObject.SetActive(_visible);
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
        SetImageRect();
    }
}