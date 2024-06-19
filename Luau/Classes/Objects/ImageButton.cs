using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageButton : MonoBehaviour
{
    public readonly string ClassName = "ImageButton";

    [SerializeField]
    public Sprite _sprite;
    [SerializeField]
    public Vector2Int _imageRectOffset;
    [SerializeField]
    public Vector2Int _imageRectSize;

    public Vector2 ImageRectOffset
    {
        get { return new Vector2(_imageRectOffset.x, _imageRectOffset.y); }
        set
        {
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