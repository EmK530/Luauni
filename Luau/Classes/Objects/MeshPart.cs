using System;
using System.Collections;
using UnityEngine;

public class MeshPart : MonoBehaviour
{
    public readonly string ClassName = "MeshPart";

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