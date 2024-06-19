using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageLabel : MonoBehaviour
{
    public readonly string ClassName = "ImageLabel";

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
            _imageRectOffset.x = Convert.ToInt32(value.x);
            _imageRectOffset.y = Convert.ToInt32(value.y);
            SetImageRect();
        }
    }

    public Vector2 ImageRectSize
    {
        get { return new Vector2(_imageRectSize.x, _imageRectSize.y); }
        set
        {
            _imageRectSize.x = Convert.ToInt32(value.x);
            _imageRectSize.y = Convert.ToInt32(value.y);
            SetImageRect();
        }
    }

    void SetImageRect()
    {
        Image img = GetComponent<Image>();
        Vector2Int sizeCopy = _imageRectSize;
        if (sizeCopy.x <= 0) { sizeCopy.x = _sprite.texture.width; }
        if (sizeCopy.y <= 0) { sizeCopy.y = _sprite.texture.height; }
        Rect rect = new Rect(_imageRectOffset.x, _sprite.texture.height-_imageRectOffset.y-sizeCopy.y, sizeCopy.x, sizeCopy.y);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        img.sprite = Sprite.Create(_sprite.texture, rect, pivot);
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
    private UDim2 _position = new UDim2(0, 0, 0, 0);
    public UDim2 Position
    {
        get { return _position; }
        set
        {
            _position = value;
            rt.anchorMin = new UnityEngine.Vector2(Convert.ToSingle(_position.Scale.x), Convert.ToSingle(1d-_position.Scale.y));
            rt.anchorMax = new UnityEngine.Vector2(Convert.ToSingle(_position.Scale.x), Convert.ToSingle(1d-_position.Scale.y));
            rt.position = new UnityEngine.Vector2(Convert.ToSingle(_position.Offset.x), Convert.ToSingle(_position.Offset.y));
        }
    }
    [SerializeField]
    private UDim2 _size = new UDim2(0, 0, 0, 0);
    public UDim2 Size
    {
        get { return _size; }
        set
        {
            rt.sizeDelta = new UnityEngine.Vector2(Convert.ToSingle(_size.Offset.x),Convert.ToSingle(_size.Offset.y));
        }
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
        _size = new UDim2(0, rt.sizeDelta.x, 0, rt.sizeDelta.y);
        SetImageRect();
        gameObject.SetActive(_visible);
    }

    void Update()
    {
        if (Application.isPlaying)
            return;
        SetImageRect();
    }
}