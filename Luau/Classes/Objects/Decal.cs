using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Decal : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Decal";

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

    [SerializeField]
    private string _texture;
    public string Texture
    {
        get { return _texture; }
        set
        {
            if (value != _texture)
            {
                SetTexture(value);
            }
        }
    }

    DecalProjector src;

    private double _transparency;
    public double Transparency
    {
        get { return _transparency; }
        set
        {
            if (value != _transparency)
            {
                _transparency = value;
                src.enabled = _transparency == 0;
            }
        }
    }

    void SetTexture(string value)
    {
        _texture = value;
        
        if (value.Contains("rbxassetid://"))
        {
            if (src == null) { src = GetComponent<DecalProjector>(); }
            string id = value.Split("rbxassetid://")[1];
            Texture m = Resources.Load<Texture>("rbxassetid/Decal/" + id);
            if (m != null)
            {
                src.material.SetTexture("Base_Map", m);
            }
            else
            {
                Logging.Warn("Could not load asset ID: " + id, "Decal:Texture");
            }
        }
    }

    public static bool isObject = true;

    void Start()
    {
        
    }
}