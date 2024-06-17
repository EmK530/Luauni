using System;
using System.Collections;
using UnityEngine;

public class ImageLabel : MonoBehaviour
{
    public readonly string ClassName = "ImageLabel";

    private RectTransform rt;

    [SerializeField]
    private UDim2 _position = new UDim2(0, 0, 0, 0);
    public UDim2 Position
    {
        get { return _position; }
        set
        {
            _position = value;
            rt.anchorMin = new Vector2(Convert.ToSingle(_position.Scale.x), Convert.ToSingle(1d-_position.Scale.y));
            rt.anchorMax = new Vector2(Convert.ToSingle(_position.Scale.x), Convert.ToSingle(1d-_position.Scale.y));
            rt.position = new Vector2(Convert.ToSingle(_position.Offset.x), Convert.ToSingle(_position.Offset.y));
        }
    }
    [SerializeField]
    private UDim2 _size = new UDim2(0, 0, 0, 0);
    public UDim2 Size
    {
        get { return _size; }
        set
        {
            rt.sizeDelta = new Vector2(Convert.ToSingle(_size.Offset.x),Convert.ToSingle(_size.Offset.y));
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
        gameObject.SetActive(_visible);
    }
}