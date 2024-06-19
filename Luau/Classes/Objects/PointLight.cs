using System;
using System.Collections;
using UnityEngine;

public class PointLight : MonoBehaviour
{
    public readonly string ClassName = "PointLight";

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    Light src;

    public double Brightness
    {
        get
        {
            if(src == null) { src = GetComponent<Light>(); }
            return src.intensity;
        }
        set
        {
            if (src == null) { src = GetComponent<Light>(); }
            src.intensity = Convert.ToSingle(value);
        }
    }

    public Color3 Color
    {
        get
        {
            if (src == null) { src = GetComponent<Light>(); }
            return new Color3(src.color.r, src.color.g, src.color.b);
        }
        set
        {
            if (src == null) { src = GetComponent<Light>(); }
            src.color = new Color(value.r, value.g, value.b);
        }
    }

    public bool Enabled
    {
        get
        {
            if (src == null) { src = GetComponent<Light>(); }
            return src.enabled;
        }
        set
        {
            if (src == null) { src = GetComponent<Light>(); }
            src.enabled = value;
        }
    }

    public double Range
    {
        get
        {
            if (src == null) { src = GetComponent<Light>(); }
            return src.range;
        }
        set
        {
            if (src == null) { src = GetComponent<Light>(); }
            src.range = Convert.ToSingle(value);
        }
    }

    public bool Shadows
    {
        get
        {
            if (src == null) { src = GetComponent<Light>(); }
            return src.shadows == LightShadows.Soft;
        }
        set
        {
            if (src == null) { src = GetComponent<Light>(); }
            src.shadows = value ? LightShadows.Soft : LightShadows.None;
        }
    }

    public static bool isObject = true;

    void Start()
    {
        src = GetComponent<Light>();
    }
}