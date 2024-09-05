using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPart : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "MeshPart";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    private MeshRenderer mr;

    public Color3 BrickColor
    {
        get
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            Color c = mr.material.GetColor("_BaseColor");
            return new Color3(c.r, c.g, c.b);
        }
        set
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            mr.material.SetColor("_BaseColor", new Color(value.r, value.g, value.b));
        }
    }
    public Color3 Color
    {
        get {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            Color c = mr.material.GetColor("_BaseColor");
            return new Color3(c.r, c.g, c.b);
        }
        set
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            mr.material.SetColor("_BaseColor", new Color(value.r, value.g, value.b));
        }
    }

    public CFrame CFrame
    {
        get
        {
            UnityEngine.Vector3 vec = transform.rotation.eulerAngles;
            CFrame cf = new CFrame(transform.position) * CFrame._angles(
                vec.x * Mathf.Deg2Rad,
                vec.y * Mathf.Deg2Rad,
                vec.z * Mathf.Deg2Rad
            );
            return cf;
        }
        set
        {
            transform.position = value.Position;
            transform.rotation = new Quaternion(value);
        }
    }

    private double _reflectance;
    public double Reflectance
    {
        get { return _reflectance; }
        set
        {
            _reflectance = value;
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            mr.material.SetFloat("_Smoothness", Convert.ToSingle(value));
        }
    }

    [SerializeField]
    private string _textureID;
    public string TextureID
    {
        get
        {
            if(_textureID == null)
            {
                _textureID = "rbxassetid://" + mr.material.name.Split(" ")[0];
            }
            return _textureID;
        }
        set
        {
            SetTexture(value);
        }
    }

    void SetTexture(string value)
    {
        if (value == null)
        {
            Logging.Warn("Attempt to set property to nil", "MeshPart:TextureID");
        }
        if (value.Contains("rbxassetid://"))
        {
            if (mr == null) { mr = GetComponent<MeshRenderer>(); }
            string id = value.Split("rbxassetid://")[1];
            Material m = new Material(Resources.Load<Material>("rbxassetid/MeshTexture/" + id));
            if (m != null)
            {
                mr.material = m;
                _textureID = value;
            }
            else
            {
                Logging.Warn("Could not load asset ID: " + id, "MeshPart:TextureID");
            }
        }
    }

    public Enum.Material Material
    {
        get { return Enum.Material.Plastic; }
        set
        {
            Logging.Warn("Ignoring unsupported property.", "MeshPart:Material");
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

    public static bool isObject = true;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        _textureID = "rbxassetid://" + mr.material.name.Split(" ")[0];
    }
}