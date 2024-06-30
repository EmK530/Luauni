using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Decal : MonoBehaviour
{
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